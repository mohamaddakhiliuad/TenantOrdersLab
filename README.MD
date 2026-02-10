# TenantOrdersLab

> **A portfolio-grade EF Core + DDD lab demonstrating how to keep persistence inside strict architectural boundaries while maintaining performance, clarity, and multi-tenancy safety.**

---

## 🧭 What is TenantOrdersLab?

**TenantOrdersLab** is a hands-on learning and demonstration project designed to show **how EF Core should be used in real systems**, not as a CRUD shortcut but as an infrastructure concern inside a **Clean Architecture + Domain‑Driven Design (DDD)** setup.

This repository is intentionally built as a **lab**:
- concepts are isolated,
- trade-offs are explicit,
- and every architectural decision is explainable in an interview.

It is not a toy CRUD app.
It is a *thinking engineer’s* EF Core project.

---

## 🎯 Core Goals

- Design **Aggregate-centric domain behavior** (not anemic models)
- Prevent **ORM leakage** into Domain and Application layers
- Separate **Read Side vs Write Side** properly
- Enforce **multi-tenancy and auditing** at the infrastructure boundary
- Build **migration-safe** EF Core configurations
- Produce a project that can be confidently discussed, defended, and extended to an API

---

## 🧱 Architecture Overview

TenantOrdersLab follows **Clean Architecture** with strict dependency direction:

```
┌─────────────────────────────┐
│         API (Phase 3)       │
│  (Composition Root only)    │
└────────────▲────────────────┘
             │
┌────────────┴────────────────┐
│        Application           │
│   (Use Cases & Orchestration)│
└────────────▲────────────────┘
             │
┌────────────┴────────────────┐
│           Domain             │
│ (Aggregates, Rules, Events)  │
└────────────▲────────────────┘
             │
┌────────────┴────────────────┐
│        Infrastructure        │
│ (EF Core, DB, Persistence)   │
└─────────────────────────────┘
```

### Dependency Rules
- **Domain** depends on nothing
- **Application** depends only on Domain
- **Infrastructure** depends on Application + Domain
- **API** depends on Application + Infrastructure

---

## 🧠 Key Design Principles

- ❌ No EF Core attributes in Domain
- ❌ No DbContext usage in Application logic
- ❌ No `Include()` in normal read queries
- ❌ No business logic inside DbContext or repositories

- ✅ All business rules live inside Aggregates
- ✅ All state transitions are explicit methods
- ✅ Read models are projection-based and no‑tracking
- ✅ Multi-tenancy and auditing are enforced automatically

---

# 🚧 Phase 1 — Domain + EF Core Foundations

## 🎯 Objective

Build a **rock-solid persistence foundation** while keeping the Domain completely pure and independent.

---

## 🧩 Domain Layer (Pure Business Core)

### Aggregates
- **Order** (Aggregate Root)
- **Customer** (Aggregate Root)

### Value Objects
- **Money** (Amount + Currency)

### Domain Behavior
- Order lifecycle managed *inside the aggregate*
- Invalid state transitions prevented via `DomainException`
- Domain Events recorded in-memory using a pull-based pattern

### Key Guarantee
> The Domain has **zero knowledge** of EF Core, databases, or persistence concerns.

---

## 🗄️ Infrastructure Layer — EF Core Mapping

### DbContext
- `OrdersDbContext` lives **only** in Infrastructure
- All configuration is done via **Fluent API**
- No data annotations

### Value Object Mapping
- `Money` mapped as an **Owned Type**
- Flattened into Order table columns:
  - `TotalAmount`
  - `TotalCurrency`

---

## 👻 Shadow Properties (Cross‑Cutting Concerns)

Implemented as **EF Core shadow properties** (not visible in Domain):

- `TenantId`
- `CreatedAtUtc`
- `UpdatedAtUtc`
- `RowVersion` (optimistic concurrency)

These are:
- enforced automatically
- never manually set in Domain or Application
- applied consistently across entities

---

## 🧭 Multi‑Tenancy

- Tenant isolation enforced at the **query level**
- Global Query Filter based on `TenantId`
- Implemented using `EF.Property` (safe for shadow properties)

> Cross-tenant data access is structurally impossible.

---

## 🔁 SaveChanges Pipeline

On every commit:
- TenantId is injected
- Audit fields are updated
- Concurrency is enforced via RowVersion

All without polluting business logic.

---

## 🧪 Migration Reliability

Phase 1 explicitly tackles real EF Core problems:
- design-time vs runtime DbContext creation
- EF version alignment
- safe migrations without breaking previous history

This ensures migrations are:
- predictable
- repeatable
- and production-safe

---

# 🚀 Phase 2 — Application Layer + Read/Write Separation

## 🎯 Objective

Move from **CRUD thinking** to **Use‑Case‑Driven design**, and demonstrate how EF Core should be *controlled*, not abused.

---

## ✍️ Application Layer (Use Case Orchestration)

The Application layer expresses **system intent**, not data access.

Responsibilities:
- Orchestrate use cases
- Coordinate domain behavior
- Control transaction boundaries

It does **not**:
- expose IQueryable
- contain EF Core logic
- make persistence decisions

---

## 🧱 Write Side (Commands)

Characteristics:
- Tracking enabled
- Aggregates loaded explicitly
- State transitions executed via domain methods
- Commit happens in a single, clear boundary

Write side exists to **change the system safely**.

---

## 👀 Read Side (Queries)

Read side is designed for **performance and safety**:

- Projection directly into DTOs
- `AsNoTracking()` everywhere
- No entity graphs
- No implicit lazy loading
- SQL observability via `TagWith`

This avoids:
- N+1 queries
- over-fetching
- memory explosions

---

## 🚫 No-Include Policy

| Scenario | Include |
|--------|---------|
| Read side (normal) | ❌ Never |
| Write side (rare) | ⚠️ With caution |
| Performance demos | ✅ Explicit |

This rule alone eliminates most EF Core production issues.

---

## 🧠 Learning Outcomes

After Phase 2, this project demonstrates:

- EF Core used as **Infrastructure**, not a design driver
- Real separation of Read vs Write models
- Aggregate-centric domain behavior
- Tenant-safe persistence by construction
- A system that can grow into an API without refactoring

---

## 📌 Current Status

✔ Phase 1 — Completed & Locked  
✔ Phase 2 — Completed & Locked

The project is now ready for:

> **Phase 3 — API / Composition Root / End‑to‑End Flow**

---

## 🏁 Why This Project Matters

TenantOrdersLab is designed to answer the question:

> “Can you build a real EF Core system without letting EF Core design your system?”

This repository proves that the answer is **yes**.

--- 

## 🔜 Next Phases (Roadmap)

- Phase 3: ASP.NET Core API (Composition Root)
- Phase 4: Integration tests & concurrency demos
- Phase 5: Performance comparison (Projection vs Include)

---

**Author:** Mohammad Dakhilitarghi  
**Focus:** EF Core · DDD · Clean Architecture · Multi‑Tenancy

