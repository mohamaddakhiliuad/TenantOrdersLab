# Idempotent CreateOrder – Technical & Educational Documentation

## Overview

This document explains the design, architectural decisions, and implementation details of the Idempotent POST mechanism added to the `CreateOrder` use case in TenantOrdersLab.

The goal is to make `POST /api/orders` idempotent using an `Idempotency-Key` header while preserving Clean Architecture boundaries.

---

# 1️⃣ Problem Statement

HTTP POST is **non-idempotent by default**.

If a client retries a request due to:
- network timeout
- mobile connectivity issues
- reverse proxy retry
- frontend double-click

The server may create duplicate Orders.

We need a production-safe mechanism that ensures:

• Same logical request → same result
• No duplicate Orders
• Safe under concurrency
• Clean Architecture remains intact

---

# 2️⃣ Design Principles Applied

This implementation follows three core principles:

## 1. HTTP Semantics
- Route identifies resource
- Query refines request
- Body carries state change
- Headers carry transport metadata

`Idempotency-Key` belongs in the **HTTP Header**, not the request body.

## 2. Clean Architecture

Dependency flow:

API → Application → Domain
         ↑
     Infrastructure (implements abstractions)

Domain remains unaware of idempotency.
Application depends only on abstractions.
Infrastructure implements persistence.

## 3. Database-Level Safety

True idempotency requires:

• Unique DB constraint
• Race condition handling
• Safe retry behavior

---

# 3️⃣ Architecture Changes Introduced

## New Application Abstractions

### IIdempotencyStore

Located in:

```
TenantOrdersLab.App.Abstractions.Idempotency
```

Responsibilities:

- Try to begin a request
- Detect duplicate/conflict/in-progress
- Mark request as completed
- Cleanup expired records

---

## New Infrastructure Components

### IdempotencyRecord (EF Entity)

Located in:

```
TenantOrdersLab.Infrastructure.Persistence.Idempotency
```

Stored in SQL Server table:

```
IdempotencyRecords
```

Key fields:

- TenantId
- Key (Idempotency-Key header)
- RequestHash
- OrderId
- Status (InProgress / Completed)
- CreatedAtUtc
- ExpiresAtUtc (TTL = 2 days)


### Unique Index

```
UNIQUE (TenantId, Key)
```

This guarantees database-level deduplication.

---

# 4️⃣ Request Flow (High-Level)

```
Client
  ↓ (Header: Idempotency-Key)
API Endpoint
  ↓
Application (CreateOrderHandler)
  ↓
IIdempotencyStore.TryBeginAsync
  ↓
Decision
  ├── Duplicate → Return existing OrderId
  ├── Conflict → 409
  ├── InProgress → 409
  └── New → Continue normal flow
```

---

# 5️⃣ CreateOrder Handler Logic (Conceptual)

1. Extract TenantId
2. Validate Idempotency-Key
3. Generate stable request hash
4. Call TryBeginAsync
5. Branch based on decision:

   • Duplicate → return previous result
   • Conflict → return 409
   • InProgress → return 409
   • New → execute normal business logic

6. Persist Order
7. Mark idempotency as Completed

---

# 6️⃣ Stable Request Hash

We compute SHA256 hash of:

```
CustomerId | TotalAmount | Currency
```

Why?

If the same Idempotency-Key is reused with a different payload:

→ We return 409 Conflict.

This prevents malicious or accidental key reuse.

---

# 7️⃣ Concurrency Safety

Race condition scenario:

```
Request A ─┐
            ├── Same Idempotency-Key
Request B ─┘
```

Behavior:

1. First insert succeeds
2. Second insert hits UNIQUE constraint
3. Store re-reads record
4. Returns correct decision

No duplicate Order is created.

---

# 8️⃣ Time-To-Live (TTL)

TTL configured: 2 days

Each record stores:

```
ExpiresAtUtc = CreatedAtUtc + 2 days
```

Cleanup can be performed:

- via background job
- or via periodic cleanup method

---

# 9️⃣ Why This Design Is Production-Grade

✔ Multi-tenant safe
✔ Database-enforced uniqueness
✔ Race-condition resilient
✔ Clean Architecture preserved
✔ HTTP standard compliant
✔ Does not pollute Domain
✔ Suitable for payment-like systems

---

# 🔟 What We Did NOT Do (Intentionally)

❌ Did not put IdempotencyKey in request body
❌ Did not leak Infrastructure into Application
❌ Did not modify Domain model
❌ Did not rely only on memory cache

---

# 1️⃣1️⃣ Testing Strategy

Test scenarios:

1. First request → 201 Created
2. Same key + same body → 201 Created (same OrderId)
3. Same key + different body → 409 Conflict
4. Concurrent requests → single Order created

---

# 1️⃣2️⃣ Interview-Level Explanation

"POST is non-idempotent by default. To make it safe for retries, we introduced an Idempotency-Key header enforced by a unique database constraint per tenant. The Application layer coordinates deduplication through an abstraction, while Infrastructure guarantees persistence safety. The Domain remains unaffected."

---

# Final Result

CreateOrder is now:

• Idempotent
• Concurrency-safe
• Multi-tenant aware
• Architecturally clean
• Production-ready

---

End of Documentation

