# TenantOrdersLab

> **A portfolio‑grade EF Core + DDD lab demonstrating how to keep persistence inside strict architectural boundaries while maintaining performance, clarity, and multi‑tenancy safety.**

---

## 🧭 What is TenantOrdersLab?

**TenantOrdersLab** is a hands‑on learning and demonstration project that shows how EF Core should be used in **real systems** — not as a CRUD shortcut, but as an infrastructure concern inside a **Clean Architecture + Domain‑Driven Design (DDD)** solution.

This project is intentionally built as a **lab**:
- Concepts are isolated
- Trade‑offs are explicit
- Every architectural decision is explainable in an interview

It is not a toy CRUD app.  
It is a *thinking engineer’s* EF Core project.

---

## 🎯 Core Goals

- Aggregate‑centric domain behavior (no anemic models)
- Prevent ORM leakage into Domain and Application
- Proper Read / Write separation
- Enforce multi‑tenancy and auditing at infrastructure boundary
- Migration‑safe EF Core configuration
- A system that is interview‑ready and production‑oriented

---

# 🧱 Clean Architecture Overview

```
API (Composition Root)
        ↓
Application (Use Cases)
        ↓
Domain (Business Rules)
        ↓
Infrastructure (EF Core / Persistence)
```

### Dependency Rules
- Domain → depends on nothing
- Application → depends only on Domain
- Infrastructure → depends on Application + Domain
- API → depends on Application + Infrastructure

---

# 🔌 Composition Root & Module Wiring

This project follows a **modular registration strategy** to keep `Program.cs` thin and maintain separation of concerns.

```
                    ┌───────────────────────────────┐
                    │         TenantOrdersLab.Api    │
                    │   (Host / Composition Root)    │
                    └───────────────┬───────────────┘
                                    │
                                    │  Only composes modules
                                    │  services.AddInfrastructure(...)
                                    ▼
┌─────────────────────────────────────────────────────────────────────┐
│          TenantOrdersLab.Infrastructure (Module Owner)              │
│                                                                     │
│  AddInfrastructure() registers:                                     │
│                                                                     │
│  Cross‑Cutting Services                                             │
│   - ITenantProvider  → HttpTenantProvider                           │
│   - IClock           → SystemClock                                  │
│   - IDomainEventDispatcher → NoOpDomainEventDispatcher              │
│                                                                     │
│  Persistence                                                         │
│   - OrdersDbContext (tenant filter, auditing, concurrency)         │
│   - IOrderRepository    → OrdersRepository                          │
│   - ICustomerRepository → CustomersRepository                       │
│   - IOrdersUnitOfWork   → OrdersUnitOfWork                          │
│   - IOrderReadQueries   → EfOrderReadQueries                        │
└─────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────┐
│               TenantOrdersLab.App (Use Cases)                       │
│                                                                     │
│  Handlers depend only on abstractions                               │
│   Example: CreateOrderHandler                                       │
│    uow.Customers.GetForUpdateAsync(...)                             │
│    uow.Orders.Add(...)                                              │
│    uow.SaveChangesAsync(...)                                        │
└─────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────┐
│                TenantOrdersLab.Domain (Pure)                        │
│                                                                     │
│  - Entities: Order, Customer                                        │
│  - Value Objects: Money                                             │
│  - Domain rules + domain events                                     │
│                                                                     │
│  NO EF Core · NO Http · NO DI · NO external dependencies            │
└─────────────────────────────────────────────────────────────────────┘
```

---

# 🔄 End‑to‑End Request Flow

```
HTTP Request
   ↓
API Endpoint (Minimal API)
   ↓  (maps Request → Command)
Application Handler
   ↓  (uses IOrdersUnitOfWork)
Infrastructure (Repositories + DbContext)
   ↓
Database
   ↓
DbContext.SaveChanges()
   ↓  (auditing + tenant + domain events)
Response → Client
```

---

# 🏢 Multi‑Tenancy Flow (Request → SQL)

Multi‑tenancy is enforced automatically using a tenant provider and EF Core global filters.

```
HTTP Request
Headers: X-TenantId = tenant-1
        │
        ▼
API Endpoint
        │ (DI scope per request)
        ▼
HttpTenantProvider
        │ reads header via IHttpContextAccessor
        ▼
OrdersDbContext
        │ receives ITenantProvider in constructor
        ▼
EF Global Query Filter
EF.Property("TenantId") == CurrentTenantId
        ▼
Generated SQL
WHERE TenantId = 'tenant-1'
```

**Result:**
- Cross‑tenant data access is structurally impossible
- Tenant isolation is enforced automatically
- No tenant logic inside Application or Domain

---

# 🚧 Phase 1 — Domain + EF Core Foundations

### Domain
- Aggregates: Order, Customer
- Value Object: Money
- Domain events stored in‑memory
- State transitions enforced via domain methods

### Infrastructure
- Fluent API only (no data annotations)
- Money mapped as Owned Type
- Shadow properties:
  - TenantId
  - CreatedAtUtc
  - UpdatedAtUtc
  - RowVersion

### SaveChanges Pipeline
Automatically:
- Injects TenantId
- Updates audit fields
- Applies optimistic concurrency

---

# 🚀 Phase 2 — Application + Read/Write Separation

### Write Side
- Tracking enabled
- Aggregates loaded explicitly
- State changes via domain methods
- Single transaction boundary

### Read Side
- Projection to DTOs
- AsNoTracking()
- No entity graphs
- SQL visibility via TagWith

### No‑Include Policy
| Scenario | Include |
|---|---|
| Read side | ❌ Never |
| Write side | ⚠️ Rare |
| Performance demos | ✅ Explicit |

---

## 📌 Current Status

✔ Phase 1 — Completed  
✔ Phase 2 — Completed  
🚧 Phase 3 — API (Composition Root) in progress

---

## 🏁 Why This Project Matters

TenantOrdersLab answers a key engineering question:

> *Can you build a real EF Core system without letting EF Core design your system?*

This project demonstrates:
- Clean architectural boundaries
- Infrastructure‑controlled persistence
- Tenant‑safe data access
- Performance‑aware query design
- Interview‑ready architecture decisions

---

## 🔜 Roadmap

- Phase 3: ASP.NET Core API completion
- Phase 4: Integration tests & concurrency scenarios
- Phase 5: Performance benchmarks (Projection vs Include)

---

**Author:** Mohammad Dakhilitarghi  
**Focus:** .NET · EF Core · DDD · Clean Architecture · Multi‑Tenancy

