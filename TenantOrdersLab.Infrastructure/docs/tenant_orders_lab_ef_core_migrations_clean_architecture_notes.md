# TenantOrdersLab – EF Core & Migrations (Clean Architecture + DDD)

> **Purpose**  
This document captures *what was built*, *why it was built this way*, and *what to remember before interviews*.  
It is intentionally practical, experience‑driven, and publication‑ready.

---

## 1. Architectural Principles Used

### Clean Architecture Boundaries
- **Domain**: Pure business logic (no EF, no attributes, no persistence concerns)
- **Infrastructure**: EF Core, DbContext, mappings, migrations, policies
- **App / Startup**: Composition only (may not even reference DbContext)

**Rule:** Domain must compile even if EF Core is removed.

---

## 2. DbContext Design (Step 3.1)

### Key Decisions
- `OrdersDbContext` lives **only** in Infrastructure
- No EF attributes in Domain
- All mapping done via **Fluent API**

```csharp
public sealed class OrdersDbContext : DbContext
{
    public OrdersDbContext(DbContextOptions<OrdersDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrdersDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
```

### Why `ApplyConfigurationsFromAssembly`
- Automatically loads all `IEntityTypeConfiguration<T>`
- DbContext never changes when entities grow
- Supports modular, scalable mapping

---

## 3. Fluent API vs Attributes

**Why Fluent API was chosen**:
- Keeps Domain clean
- Better separation of concerns
- Supports advanced mappings (Owned Types, Shadow Properties, Filters)
- Aligns with DDD principles

> Attributes are convenient but leak persistence into Domain.

---

## 4. Entity Mapping Example – Order

### Concepts Used
- Explicit primary key
- Enum stored as int
- Value Object mapped as Owned Type
- Domain Events ignored by EF

```csharp
builder.HasKey(x => x.Id);
builder.Property(x => x.Id).ValueGeneratedNever();
builder.Property(x => x.Status).HasConversion<int>();

builder.OwnsOne(x => x.Total, money =>
{
    money.Property(p => p.Amount).HasColumnName("TotalAmount");
    money.Property(p => p.Currency).HasColumnName("TotalCurrency");
});

builder.Ignore("_domainEvents");
```

---

## 5. Shadow Properties (Step 3.3)

### Why Shadow Properties
- Infrastructure concerns only
- No Domain pollution
- Ideal for multi‑tenancy and auditing

### Implemented Shadows
- `TenantId`
- `CreatedAtUtc`
- `UpdatedAtUtc`
- `RowVersion` (optimistic concurrency)

```csharp
builder.Property<string>("TenantId").IsRequired();
builder.Property<DateTime>("CreatedAtUtc").IsRequired();
builder.Property<DateTime>("UpdatedAtUtc").IsRequired();
builder.Property<byte[]>("RowVersion").IsRowVersion();
```

---

## 6. Design‑Time vs Runtime (Critical Learning)

### The Problem Encountered
EF Core migrations failed with:
> Unable to resolve DbContextOptions

**Reason:**
- DbContext lives in Infrastructure
- Startup project did not register it
- EF Tools could not construct DbContext at design‑time

### The Correct Solution
`IDesignTimeDbContextFactory<T>`

```csharp
public sealed class OrdersDbContextFactory
    : IDesignTimeDbContextFactory<OrdersDbContext>
{
    public OrdersDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<OrdersDbContext>()
            .UseSqlServer("(localdb)\\MSSQLLocalDB")
            .Options;

        return new OrdersDbContext(options);
    }
}
```

**Key Insight:**
> Migration is a *design‑time build*, not runtime execution.

---

## 7. Null‑Safety in OnModelCreating

### Issue
Query filters depended on runtime services (TenantProvider).

### Fix
Guard runtime‑only logic:
```csharp
if (_tenantProvider is null) return;
```

**Rule:**
> `OnModelCreating` must be safe without runtime dependencies.

---

## 8. Migration Lifecycle (Mental Model)

Migration = **Model Diff**

1. Modify model (Domain / Configuration)
2. Build project
3. EF creates DbContext (design‑time)
4. EF diffs current model vs snapshot
5. Generates migration (`Up` / `Down`)
6. Applies to database

```powershell
dotnet ef migrations add <Name>
dotnet ef database update
```

---

## 9. Adding New Changes – What Happens?

### Add Column to Existing Table
- Change mapping (or domain)
- New migration
- `ALTER TABLE`

### Add New Table
- New entity
- New configuration
- New migration
- `CREATE TABLE`

**Golden Rule:**
> Never modify applied migrations. Always add a new one.

---

## 10. Relationship Mapping – Customer ↔ Order

### Chosen Model (DDD‑friendly)
- `Order` references `Customer`
- `Customer` does NOT own Orders collection

```csharp
builder.HasOne(o => o.Customer)
       .WithMany()
       .HasForeignKey(o => o.CustomerId)
       .OnDelete(DeleteBehavior.Restrict);
```

### Why No `Customer.Orders`
- Preserves Aggregate boundaries
- Avoids large object graphs
- Prevents accidental eager loading

Read‑side uses projections instead of navigation graphs.

---

## 11. Key Interview‑Level Takeaways

- EF Core is **not just an ORM**, it is a *modeling engine*
- Design‑time and runtime are different worlds
- Fluent API enables clean DDD
- Migrations are snapshots + diffs
- Relationships affect **memory graphs**, not just DB schema
- Shadow properties are ideal for cross‑cutting concerns

---

## 12. When to Use This Document

- Pre‑interview refresh
- Explaining EF Core decisions to a team
- README for portfolio project
- Teaching juniors *why*, not just *how*

---

**Status:** Migration phase complete ✔  
**Next logical steps:** Read‑side projections, concurrency tests, policy enforcement

