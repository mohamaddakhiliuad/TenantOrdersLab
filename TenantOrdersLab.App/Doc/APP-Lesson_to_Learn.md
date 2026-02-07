# Phase 2 — Why So Many Classes for One Insert/Update?
*(English — simple, clear, interview‑ready)*

---

## The One Sentence That Explains Everything

**You’re not building CRUD.**  
**You’re building a system where business intent is explicit and EF Core is only a persistence engine.**

If you write one “dirty service class”, you may get fast output today… but you will also get hidden problems later:

- Accidental graph persistence
- Tenant leakage
- Unclear transaction boundaries
- Untestable spaghetti flows
- Code that is impossible to explain in interviews

This structure exists to make **intent, boundaries, and responsibility** obvious.

---

# 1) The Cast of Characters (What Each Part Is FOR)

## A) Command = Input DTO  
**“What the caller wants”**

Example:
```
CreateOrderCommand(CustomerId, TotalAmount, Currency)
```

Characteristics:
- Contains only input data
- No EF Core
- No business logic
- Stable contract for the use case

**Think:** A request form.

---

## B) Handler = Use Case Orchestrator  
**“How we execute this intent”**

Example: `CreateOrderHandler`

The handler performs the sequence:

1. Load required data (Customer / Order)
2. Call domain behavior
3. Persist and commit

**Important rule:**
> Handler contains orchestration — not EF tricks and not business rules.

**Think:** A checklist executor.

---

## C) Domain (Entities / Value Objects)  
**“What is allowed”**

Example:
```
order.Place()
```

Domain contains:
- Invariants (cannot place twice)
- State transitions (New → Placed)
- Domain events (something meaningful happened)

**Think:** The law of the system.

---

## D) IOrdersDbContext = Boundary  
**“Application talks to persistence without EF Core”**

Application **must not depend on EF Core**.

Application only knows:
- How to load aggregates
- How to add aggregates
- How to commit

Infrastructure implements this interface using EF Core.

**Think:** A power socket.  
Application plugs in. Infrastructure provides electricity.

---

## E) Result / DTO = Output Contract  
**“What the caller receives”**

Example:
```
CreateOrderResult(OrderId)
```

Benefits:
- Predictable output
- Easy testing
- Easy logging

**Think:** A receipt.

---

# 2) The Real Flow (Runtime Behavior)

## Use Case 1 — CreateOrder (Write Side)

Caller builds `CreateOrderCommand`

Handler steps:

1. `GetCustomerForUpdateAsync(customerId)`
   - Tenant boundary enforced in Infrastructure

2. Build domain objects
   - `Money(...)`
   - `Order.CreateNew(...)`

3. `_db.AddOrder(order)`
   - Stage for persistence

4. `_db.SaveChangesAsync()`
   - Transaction commit

Output: `OrderId`

This is **not just an insert**.

It is:
> Create an order according to business rules, safely, for the correct tenant, and commit it.

---

## Use Case 2 — PlaceOrder (Write Side)

Caller builds `PlaceOrderCommand(orderId)`

Handler steps:

1. `GetOrderForUpdateAsync(orderId)`
   - Tracked entity load

2. `order.Place()`
   - Domain rule + state transition
   - Domain event created

3. `_db.SaveChangesAsync()`
   - Commit transaction

4. Collect domain events (dispatch later in Phase 3/4)

This is **not just an update**.

It is:
> Perform a business state transition safely and commit it.

---

# 3) Why We Separate Read and Write

## Write Side
- Tracked entities
- Domain methods
- Changes + commit

## Read Side
- Projection (`Select` into DTO)
- `AsNoTracking()`
- `TagWith(...)`
- No `Include`

Why?

- Reads must be fast and safe
- Writes must be controlled and consistent

---

# 4) Why the “One Dirty Class” Feels Easier (But Hurts Later)

A dirty service usually mixes:

- Queries
- State changes
- Mapping
- EF tracking behavior
- Business rules

This causes:

- Implicit behavior (hard to reason about)
- Accidental persistence
- No clear transaction boundary
- Poor testability
- Weak interview explanation

Phase 2 uses more files — but far less chaos.

---

# 5) The Mental Model

## Application = Use Case Scripts

“When X happens, do steps 1 → 2 → 3”

- Talks to Domain
- Commits through abstraction

---

## Domain = The Rules

- Decides what is allowed
- Manages state transitions
- Produces domain events

---

## Infrastructure = The Plumbing

- EF Core
- Mappings
- Migrations
- Query filters (tenant)
- Implements `IOrdersDbContext`

---

# 6) The 10‑Second Interview Explanation

> We model use cases explicitly using Commands and Handlers. Domain contains business rules and state transitions. Application orchestrates the flow and commits through an abstraction so it doesn’t depend on EF Core. The read side uses projection with NoTracking and avoids Include to prevent tracking issues and keep queries fast.

---

# 7) Quick Self‑Check

**Q: Where does `order.Place()` belong?**  
→ Domain (Order aggregate)

**Q: Where does `SaveChanges` belong?**  
→ Application calls it via abstraction; Infrastructure implements it

**Q: Where does projection belong?**  
→ Read query handler (executed by EF in Infrastructure)

**Q: Should DbContext interface contain `PayOrder` or `CancelOrder`?**  
→ No. Those are use cases or domain behaviors, not persistence.

