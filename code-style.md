# Code Style & Architecture Standards — AeroMes (.NET 10)

This document defines the folder structure, coding standards, CQRS design, EF Core configuration, Identity, error handling, and cold-start patterns for the AeroMes MES project on .NET 10 / C# 13.

---

## 1. Solution Structure

The project follows **Clean Architecture + Modular Monolith** with 4 projects:

```
📁 AeroMes/
│
├── 📁 src/
│   ├── 📁 AeroMes.Api/                     # Host layer: Controllers, Middleware, Program.cs
│   │   ├── 📁 Controllers/                 # MVC Controllers — call MediatR, return ApiResponse
│   │   ├── 📁 Identity/                    # TokenService (JWT generation)
│   │   ├── 📁 Middleware/                  # ExceptionMiddleware (RFC 7807 ProblemDetails)
│   │   └── 📁 Serialization/               # AeroMesJsonContext (STJ source generator)
│   │
│   ├── 📁 AeroMes.Infrastructure/          # Physical infrastructure: DB, Repositories, Migrations
│   │   ├── 📁 Data/                        # AppDbContext, IdempotencyStore, Configurations/
│   │   ├── 📁 Repositories/                # IRepository implementations from Domain
│   │   └── 📁 Migrations/                  # EF Core migrations
│   │
│   ├── 📁 AeroMes.Application/             # Application logic: CQRS, MediatR, Validators
│   │   ├── 📁 Common/
│   │   │   ├── 📁 Behaviors/               # ValidationBehavior (MediatR pipeline)
│   │   │   └── ApiResponse.cs              # record ApiResponse<T> used by Controllers
│   │   ├── 📁 Interfaces/                  # IUnitOfWork, ITokenService
│   │   └── 📁 {Module}/                    # One folder per bounded context
│   │       ├── 📁 Commands/
│   │       │   └── 📁 {UseCaseName}/       # Command.cs + Handler.cs + Validator.cs co-located
│   │       └── 📁 Queries/
│   │           └── 📁 {QueryName}/         # Query.cs + Handler.cs co-located
│   │
│   └── 📁 AeroMes.Domain/                  # Entities, Value Objects, Domain Events, Exceptions
│       ├── 📁 Common/                      # Entity, AuditableEntity, AggregateRoot, IDomainEvent
│       ├── 📁 Exceptions/                  # DomainException, EntityNotFoundException
│       ├── 📁 Master/                      # Product, Machine, WorkCenter, Operation, Routing, BomItem...
│       ├── 📁 Production/                  # WorkOrder, Job, ProductionLog, DowntimeLog...
│       ├── 📁 Quality/                     # DefectCode, DefectDetail
│       └── 📁 Integration/                 # SalesOrder, ProductionOrder (ERP sync)
│
└── 📁 web/                                 # Vite + React frontend
```

**Application module mapping:**

| Module folder | Bounded context |
|---|---|
| `Production/` | Output recording, OEE |
| `WorkOrders/` | Work order management |
| `Jobs/` | Start/Finish job at machine |
| `Downtime/` | Downtime recording |
| `Master/` | Reference data (Products, Machines, WorkCenters, Operations, Routings...) |

---

## 2. Coding Style (C# 13 / .NET 10)

Apply these modern language features consistently:

- **File-scoped namespaces** — eliminate unnecessary indentation.
- **Primary constructors** — use for DI in Controllers, Handlers, Repositories.
- **Collection expressions (`[...]`)** — replace `new List<T>()` and `new[] {}`.
- **Required members + init-only** — enforce mandatory data at construction time.
- **Record types** — use for Commands, Queries, DTOs, and results (immutable by default).

```csharp
// Standard entity — file-scoped namespace, required, init, collection expression
namespace AeroMes.Domain.Master;

public class Product : AuditableEntity
{
    public required string ProductCode { get; init; }
    public required string ProductName { get; set; }
    public required string PrimaryUnit { get; set; }
    public string? SecondaryUnit { get; set; }
    public required string TrackingMethod { get; set; } // NONE, LOT, SERIAL
    public bool IsActive { get; set; } = true;
    public List<string> DynamicTags { get; set; } = [];
}
```

---

## 3. CQRS Design with MediatR

Strict separation between **Write (Commands)** and **Read (Queries)**.

```
                  ┌───────────────┐
                  │  Controller   │
                  └───────┬───────┘
                          │ mediator.Send(...)
            ┌─────────────┴─────────────┐
            ▼                           ▼
    ┌───────────────┐           ┌───────────────┐
    │   Commands    │           │    Queries    │
    │ (Create/Update│           │  (View/Report)│
    └───────┬───────┘           └───────┬───────┘
            │ IUnitOfWork                │ AsNoTracking / Dapper
            ▼                           ▼
    ┌───────────────┐           ┌───────────────┐
    │  Primary DB   │           │  Read replica │
    └───────────────┘           └───────────────┘
```

### Implementation rules

**Commands (Write):**
- Must have a co-located `Validator` in the same use-case folder.
- Handler uses **Repository + IUnitOfWork** — never inject `AppDbContext` directly.
- Return a strongly-typed result record — no `Result<T>` wrapper.
- Throw `DomainException` or `EntityNotFoundException` — `ExceptionMiddleware` catches and returns RFC 7807.

**Queries (Read):**
- Use `AsNoTracking()` or Dapper. No transactions.
- May return DTO/record directly — no result wrapper needed.

**Pipeline Behavior (MediatR):**
- Only **`ValidationBehavior`** — runs FluentValidation automatically before every Handler.
- `ValidationBehavior` throws `ValidationException` → `ExceptionMiddleware` → HTTP 422.

### Use-case folder structure

```
Commands/
  SubmitOutput/
    SubmitOutputCommand.cs    ← record Command + record Result
    SubmitOutputHandler.cs    ← IRequestHandler
    SubmitOutputValidator.cs  ← AbstractValidator<Command>
```

---

## 4. EF Core Configuration & JSON Columns

### 4.1. JSON columns for dynamic attributes

```csharp
// Domain entity
public class Machine : AuditableEntity
{
    public required string MachineCode { get; set; }
    public required string MachineName { get; set; }
    public required string MachineType { get; set; }
    public required MachineAttributes CustomAttributes { get; set; }
    public bool IsDeleted { get; set; } = false;
}

public class MachineAttributes
{
    public int? ClampingForceTons { get; set; }
    public int? MaxShotWeightGrams { get; set; }
    public int? CapacityLiters { get; set; }
    public bool? IsSterile { get; set; }
}
```

### 4.2. Fluent API — native JSON + soft delete

```csharp
namespace AeroMes.Infrastructure.Data.Configurations;

public class MachineConfiguration : IEntityTypeConfiguration<Machine>
{
    public void Configure(EntityTypeBuilder<Machine> builder)
    {
        builder.ToTable("Machines", "master");
        builder.HasKey(m => m.MachineCode);

        builder.OwnsOne(m => m.CustomAttributes, nav => nav.ToJson());

        builder.HasQueryFilter(m => !m.IsDeleted);
    }
}
```

### 4.3. Repository + IUnitOfWork pattern

Handlers never inject `AppDbContext` directly. All writes go through a repository interface and `IUnitOfWork.SaveChangesAsync()`.

```csharp
public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken ct = default);
}
```

---

## 5. Error Handling & API Response

### 5.1. ExceptionMiddleware (RFC 7807 ProblemDetails)

`AeroMes.Api/Middleware/ExceptionMiddleware.cs` catches all exceptions and returns `application/problem+json`:

| Exception | HTTP Status |
|---|---|
| `ValidationException` (FluentValidation) | 422 Unprocessable Entity |
| `EntityNotFoundException` | 404 Not Found |
| `DomainException` | 422 Unprocessable Entity |
| Unhandled `Exception` | 500 Internal Server Error |

No try/catch in Handlers — throw the exception, middleware handles it.

### 5.2. ApiResponse for Controllers

```csharp
// AeroMes.Application/Common/ApiResponse.cs
public record ApiResponse<T>(bool Success, string Message, T? Data = default);
public record ApiResponse(bool Success, string Message);
```

Controllers return `ApiResponse<T>` when wrapping data is needed. Simple actions may return a DTO directly.

### 5.3. Domain exceptions

```csharp
// When entity is not found in a Handler
throw new EntityNotFoundException(nameof(Job), cmd.JobId);

// When a business rule is violated in the Domain
throw new DomainException($"Job {job.JobID} must be Active. Current: {job.Status}.");
```

---

## 6. Identity & Authentication

Dual-scheme authentication: **Cookie** (Web UI) + **JWT Bearer** (PDA / API clients).

- `TokenService` lives at `AeroMes.Api/Identity/TokenService.cs`.
- Scheme selector in `Program.cs`: if the request has `Authorization: Bearer ...` → JWT, otherwise → Cookie.
- Cookie name: `AeroMes.Auth`, HttpOnly, SlidingExpiration 8h.

---

## 7. Complete Blueprint — SubmitOutput Use Case

A working end-to-end example: recording OK/NG output from a PDA device.

### 7.1. Command & Result

```csharp
// AeroMes.Application/Production/Commands/SubmitOutput/SubmitOutputCommand.cs
using MediatR;

namespace AeroMes.Application.Production.Commands.SubmitOutput;

public record SubmitOutputCommand(
    long JobId,
    int QtyOk,
    int QtyNg,
    string? DeviceIp,
    string? Notes,
    string? IdempotencyKey,
    DateTime? Timestamp,
    List<DefectEntry> Defects) : IRequest<SubmitOutputResult>;

public record DefectEntry(string DefectCode, int Qty);

public record SubmitOutputResult(long LogId, int WorkOrderOK, int WorkOrderNG, bool IsDuplicate = false);
```

### 7.2. Validator

```csharp
// AeroMes.Application/Production/Commands/SubmitOutput/SubmitOutputValidator.cs
using FluentValidation;

namespace AeroMes.Application.Production.Commands.SubmitOutput;

public class SubmitOutputValidator : AbstractValidator<SubmitOutputCommand>
{
    public SubmitOutputValidator(IJobRepository jobRepo, IDefectCodeRepository defectRepo)
    {
        RuleFor(x => x.JobId)
            .GreaterThan(0).WithMessage("Job id is required.")
            .MustAsync(async (id, ct) => await jobRepo.GetByIdAsync(id, ct) is not null)
            .WithMessage(x => $"Job {x.JobId} does not exist.");

        RuleFor(x => x.QtyOk).GreaterThanOrEqualTo(0);
        RuleFor(x => x.QtyNg).GreaterThanOrEqualTo(0);

        RuleFor(x => x)
            .Must(x => x.QtyOk + x.QtyNg > 0)
            .WithMessage("At least one unit (OK or NG) must be submitted.");

        When(x => x.Defects is { Count: > 0 }, () =>
        {
            RuleFor(x => x.Defects)
                .Must(d => d.All(e => e.Qty > 0))
                .WithMessage("Each defect entry must have a positive quantity.");

            RuleForEach(x => x.Defects).ChildRules(d =>
            {
                d.RuleFor(x => x.DefectCode).NotEmpty();
                d.RuleFor(x => x.Qty).GreaterThan(0);
            });
        });
    }
}
```

### 7.3. Handler

```csharp
// AeroMes.Application/Production/Commands/SubmitOutput/SubmitOutputHandler.cs
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Production;
using MediatR;

namespace AeroMes.Application.Production.Commands.SubmitOutput;

public class SubmitOutputHandler(
    IJobRepository jobRepo,
    IWorkOrderRepository workOrderRepo,
    IProductionLogRepository productionLogRepo,
    IDefectCodeRepository defectCodeRepo,
    IUnitOfWork uow)
    : IRequestHandler<SubmitOutputCommand, SubmitOutputResult>
{
    public async Task<SubmitOutputResult> Handle(SubmitOutputCommand cmd, CancellationToken ct)
    {
        // Idempotency guard
        if (cmd.IdempotencyKey is not null &&
            await productionLogRepo.ExistsByIdempotencyKeyAsync(cmd.IdempotencyKey, ct))
            return new SubmitOutputResult(-1, -1, -1, IsDuplicate: true);

        var job = await jobRepo.GetByIdAsync(cmd.JobId, ct)
            ?? throw new EntityNotFoundException(nameof(Job), cmd.JobId);

        if (job.Status != JobStatus.Active)
            throw new DomainException($"Job {job.JobID} must be Active. Current: {job.Status}.");

        var workOrder = await workOrderRepo.GetByIdAsync(job.WOID, ct)
            ?? throw new EntityNotFoundException(nameof(WorkOrder), job.WOID);

        workOrder.AccumulateOutput(cmd.QtyOk, cmd.QtyNg, job.OperatorID);

        var log = ProductionLog.Create(
            cmd.JobId, cmd.QtyOk, cmd.QtyNg,
            cmd.DeviceIp, cmd.IdempotencyKey, cmd.Notes, cmd.Timestamp);

        if (cmd.QtyNg > 0 && cmd.Defects.Count > 0)
        {
            var codes = await defectCodeRepo.GetByCodesAsync(
                cmd.Defects.Select(d => d.DefectCode), ct);

            foreach (var entry in cmd.Defects)
            {
                if (!codes.TryGetValue(entry.DefectCode, out var code))
                    throw new EntityNotFoundException("DefectCode", entry.DefectCode);
                log.AddDefect(code.DefectCodeID, entry.Qty);
            }
        }

        await productionLogRepo.AddAsync(log, ct);
        await uow.SaveChangesAsync(ct);

        return new SubmitOutputResult(log.LogID, workOrder.ActualQtyOK.Value, workOrder.ActualQtyNG.Value);
    }
}
```

---

## 8. Reducing Cold Start — Avoid Reflection (AOT-Friendly Patterns)

Goal: prevent the runtime from using reflection to discover types at startup or when serialising API responses.

### 8.1. No anonymous types

Anonymous types (`new { ... }`) force the compiler to emit hidden types and force the serialiser to use reflection. **Never use them** in:

- LINQ projections whose result leaves the method scope
- Handler return values
- Parameters or return types of any public method

```csharp
// ❌ WRONG — anonymous type
var result = await _db.Products
    .Select(p => new { p.ProductCode, p.ProductName })
    .ToListAsync(ct);

// ✓ CORRECT — named record
var result = await _db.Products
    .Select(p => new ProductListItem(p.ProductCode, p.ProductName))
    .ToListAsync(ct);

public record ProductListItem(string ProductCode, string ProductName);
```

### 8.2. JSON source generator

Every type used in an API response **must** be registered in a `JsonSerializerContext`. Single file at `AeroMes.Api/Serialization/AeroMesJsonContext.cs`:

```csharp
// AeroMes.Api/Serialization/AeroMesJsonContext.cs
using System.Text.Json.Serialization;
using AeroMes.Application.Common;
using AeroMes.Application.Master.Queries.GetProducts;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Serialization;

[JsonSerializable(typeof(ProblemDetails))]
[JsonSerializable(typeof(ValidationProblemDetails))]
[JsonSerializable(typeof(ApiResponse))]
[JsonSerializable(typeof(ApiResponse<ProductListItem>))]
[JsonSerializable(typeof(ApiResponse<List<ProductListItem>>))]
// ... add every new type here when creating a new endpoint
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal sealed partial class AeroMesJsonContext : JsonSerializerContext;
```

Wire into MVC in `Program.cs`:

```csharp
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
        opts.JsonSerializerOptions.TypeInfoResolverChain.Insert(0, AeroMesJsonContext.Default));
```

**Mandatory rule:** when creating a new endpoint, add `[JsonSerializable(typeof(ApiResponse<YourDto>))]` to `AeroMesJsonContext` in the same PR.

### 8.3. No reflection in application code

| Forbidden | Replace with |
|---|---|
| `dynamic` | Explicit type or generic |
| `Activator.CreateInstance(type)` | Factory method / DI |
| `Type.GetProperties()` / `PropertyInfo` | Not needed — use record constructor |
| AutoMapper (reflection-based) | Explicit mapping in a static method or primary constructor |
| `Enum.GetName(value)` in hot path | Pre-built `static readonly Dictionary<TEnum, string>` |

```csharp
// ❌ WRONG — AutoMapper reflection
CreateMap<Product, ProductDto>();

// ✓ CORRECT — explicit static mapping
public static ProductDto ToDto(this Product p) =>
    new(p.ProductCode, p.ProductName, p.IsActive);
```

### 8.4. Extension method mapping instead of AutoMapper

All Entity → DTO mappings use an extension method co-located with the DTO file:

```csharp
// AeroMes.Application/Master/Queries/GetProducts/ProductListItem.cs
namespace AeroMes.Application.Master.Queries.GetProducts;

public record ProductListItem(string ProductCode, string ProductName, bool IsActive);

public static class ProductMappings
{
    public static ProductListItem ToListItem(this Product p) =>
        new(p.ProductCode, p.ProductName, p.IsActive);
}
```

### 8.5. Avoid lazy reflection-based initialisation

- Do not use `[FromServices]` attribute injection — use constructor injection.
- Do not use `IServiceLocator` / `HttpContext.RequestServices` in a Handler.
- Do not call `Assembly.GetTypes()` or perform assembly scanning in the startup path unless strictly necessary; if needed (e.g. MediatR, FluentValidation registration), ensure it runs exactly once and caches the result.
