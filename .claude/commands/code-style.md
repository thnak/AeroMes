---
description: AeroMes coding standards and architecture reference (.NET 10 / C# 13). Apply before writing or modifying any C# code — covers solution structure, CQRS patterns, EF Core config, error handling, Identity, and cold-start/AOT rules.
---

Apply the full AeroMes coding standards to the current task. The complete reference is below.

---

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
│   │   ├── 📁 Controllers/                 # MVC Controllers — call LiteBus mediators, return IActionResult
│   │   ├── 📁 Extensions/                  # ValidationResultExtensions (ToErrorResult)
│   │   ├── 📁 Identity/                    # TokenService (JWT generation)
│   │   ├── 📁 Middleware/                  # ExceptionMiddleware (RFC 7807 ProblemDetails)
│   │   └── 📁 Serialization/               # AeroMesJsonContext (STJ source generator)
│   │
│   ├── 📁 AeroMes.Infrastructure/          # Physical infrastructure: DB, Repositories, Migrations
│   │   ├── 📁 Data/                        # AppDbContext, IdempotencyStore, Configurations/
│   │   ├── 📁 Repositories/                # IRepository implementations from Domain
│   │   └── 📁 Migrations/                  # EF Core migrations
│   │
│   ├── 📁 AeroMes.Application/             # Application logic: CQRS (LiteBus), Validators
│   │   ├── 📁 Common/
│   │   │   ├── ValidationResult.cs         # ValidationResult<T>, Unit
│   │   │   └── ApiResponse.cs              # record ApiResponse<T> (legacy — Integration/Jobs only)
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

## 3. CQRS Design with LiteBus v5

Strict separation between **Write (Commands)** and **Read (Queries)**. The project uses **LiteBus**, not MediatR.

```
                  ┌───────────────┐
                  │  Controller   │
                  └───────┬───────┘
                          │ commandMediator.SendAsync(cmd, null, ct)
            ┌─────────────┴─────────────┐
            ▼                           ▼
    ┌───────────────┐           ┌───────────────┐
    │   Commands    │           │    Queries    │
    │ (Create/Update│           │  (View/Report)│
    └───────┬───────┘           └───────┬───────┘
            │ IUnitOfWork                │ AsNoTracking
            ▼                           ▼
    ┌───────────────┐           ┌───────────────┐
    │  Primary DB   │           │  Read store   │
    └───────────────┘           └───────────────┘
```

### LiteBus interface mapping

| Concept | Interface |
|---|---|
| Command (with result) | `: ICommand<ValidationResult<TResult>>` |
| Command (void / no result) | `: ICommand<ValidationResult<Unit>>` |
| Query | `: IQuery<TResult>` |
| Command handler | `: ICommandHandler<TCmd, ValidationResult<TResult>>` |
| Query handler | `: IQueryHandler<TQuery, TResult>` |
| Handler method | `HandleAsync(T message, CancellationToken ct)` |
| Inject in controllers | `ICommandMediator` / `IQueryMediator` (never `IMediator`) |
| Send command | `await commandMediator.SendAsync(cmd, null, ct)` |
| Send query | `await queryMediator.QueryAsync(query, null, ct)` |

### Validation pattern

Validators are registered via `services.AddValidatorsFromAssembly(assembly)` in `Application/DependencyInjection.cs`. There is **no pre-handler** — each command handler injects `IValidator<TCommand>` and runs it manually.

```csharp
// at the top of HandleAsync — always first
var validation = await validator.ValidateAsync(cmd, ct);
if (!validation.IsValid)
    return ValidationResult<TResult>.Invalid(validation.ToErrorDictionary());
```

### ValidationResult<T> — result type for all commands

`ValidationResult<T>` (in `Application/Common/`) is a discriminated union:

| Factory | When to use |
|---|---|
| `ValidationResult<T>.Ok(value)` | Success |
| `ValidationResult<T>.Invalid(dict)` | FluentValidation failures |
| `ValidationResult<T>.Failure(message)` | `DomainException` caught |
| `ValidationResult<T>.NotFound(message)` | `EntityNotFoundException` caught |

Command handlers **catch** domain exceptions instead of letting them propagate:

```csharp
try { /* domain logic */ }
catch (EntityNotFoundException ex) { return ValidationResult<T>.NotFound(ex.Message); }
catch (DomainException ex)         { return ValidationResult<T>.Failure(ex.Message); }
```

### Implementation rules

**Commands (Write):**
- Must have a co-located `Validator` in the same use-case folder.
- Handler injects `IValidator<TCommand>` + Repository interfaces + `IUnitOfWork` — never inject `AppDbContext` directly.
- Return `ValidationResult<T>` — check then unwrap in the controller with `result.ToErrorResult()` / `result.Value!`.

**Queries (Read):**
- Use `AsNoTracking()`. No transactions.
- Return the DTO/record directly — no result wrapper needed.

### Use-case folder structure

```
Commands/
  SubmitOutput/
    SubmitOutputCommand.cs    ← record Command + record Result
    SubmitOutputHandler.cs    ← ICommandHandler
    SubmitOutputValidator.cs  ← AbstractValidator<Command>
```

---

## 4. EF Core Configuration & JSON Columns

### 4.1. JSON columns for dynamic attributes

```csharp
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

### 5.1. Primary path — ValidationResult<T> in handlers

Command handlers **catch** domain exceptions internally and return a `ValidationResult<T>`:

```csharp
try { /* domain logic */ }
catch (EntityNotFoundException ex) { return ValidationResult<T>.NotFound(ex.Message); }
catch (DomainException ex)         { return ValidationResult<T>.Failure(ex.Message); }
```

Controllers unwrap using the `ToErrorResult()` extension (`Api/Extensions/ValidationResultExtensions.cs`):

```csharp
var result = await commandMediator.SendAsync(cmd, null, ct);
if (!result.IsSuccess) return result.ToErrorResult();
return Ok(result.Value!);
```

`ToErrorResult()` maps the result state to RFC 7807 `application/problem+json`:

| Result state | HTTP Status |
|---|---|
| `IsNotFound` | 404 Not Found |
| `Errors` set | 422 Unprocessable Entity (`ValidationProblemResponse`) |
| `ErrorMessage` set | 422 Unprocessable Entity (`SimpleProblemResponse`) |

### 5.2. Fallback — ExceptionMiddleware

`ExceptionMiddleware` is the last-resort handler for anything that escapes a handler (e.g. infrastructure errors). Do not rely on it for expected domain failures — use the catch pattern above.

| Exception (unhandled) | HTTP Status |
|---|---|
| `EntityNotFoundException` | 404 |
| `DomainException` | 422 |
| `Exception` | 500 |

### 5.3. ApiResponse (legacy — Integration / Jobs controllers only)

```csharp
public record ApiResponse<T>(bool Success, string Message, T? Data = default);
```

`ApiResponse<T>` is used in `IntegrationController` and `JobsController` for backward compatibility. **New controllers must not use it** — return DTOs or named result records directly via `IActionResult`.

### 5.4. Domain exceptions in Domain layer

Entities and domain services throw these; handlers are responsible for catching them:

```csharp
throw new EntityNotFoundException(nameof(Job), cmd.JobId);
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

### 7.1. Command & Result

```csharp
// AeroMes.Application/Production/Commands/SubmitOutput/SubmitOutputCommand.cs
namespace AeroMes.Application.Production.Commands.SubmitOutput;

public record SubmitOutputCommand(
    long JobId,
    int QtyOk,
    int QtyNg,
    string? Notes,
    string? IdempotencyKey,
    List<DefectEntry> Defects) : ICommand<ValidationResult<SubmitOutputResult>>;

public record DefectEntry(string DefectCode, int Qty);
public record SubmitOutputResult(long LogId, int WorkOrderOk, int WorkOrderNg, bool IsDuplicate = false);
```

### 7.2. Validator

Validators are plain FluentValidation `AbstractValidator<T>` — registered via DI and injected into the handler. Keep validators to structural/format rules; defer existence checks (e.g. "does this job exist?") to the handler where you already load the entity.

```csharp
namespace AeroMes.Application.Production.Commands.SubmitOutput;

public class SubmitOutputValidator : AbstractValidator<SubmitOutputCommand>
{
    public SubmitOutputValidator()
    {
        RuleFor(x => x.JobId).GreaterThan(0);
        RuleFor(x => x.QtyOk).GreaterThanOrEqualTo(0);
        RuleFor(x => x.QtyNg).GreaterThanOrEqualTo(0);

        RuleFor(x => x)
            .Must(x => x.QtyOk + x.QtyNg > 0)
            .WithMessage("At least one unit (OK or NG) must be submitted.");

        When(x => x.Defects is { Count: > 0 }, () =>
        {
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
namespace AeroMes.Application.Production.Commands.SubmitOutput;

public class SubmitOutputHandler(
    IJobRepository jobRepo,
    IWorkOrderRepository workOrderRepo,
    IProductionLogRepository productionLogRepo,
    IDefectCodeRepository defectCodeRepo,
    IUnitOfWork uow,
    IValidator<SubmitOutputCommand> validator)
    : ICommandHandler<SubmitOutputCommand, ValidationResult<SubmitOutputResult>>
{
    public async Task<ValidationResult<SubmitOutputResult>> HandleAsync(
        SubmitOutputCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<SubmitOutputResult>.Invalid(validation.ToErrorDictionary());

        try
        {
            if (cmd.IdempotencyKey is not null &&
                await productionLogRepo.ExistsByIdempotencyKeyAsync(cmd.IdempotencyKey, ct))
                return ValidationResult<SubmitOutputResult>.Ok(
                    new SubmitOutputResult(-1, -1, -1, IsDuplicate: true));

            var job = await jobRepo.GetByIdAsync(cmd.JobId, ct)
                ?? throw new EntityNotFoundException(nameof(Job), cmd.JobId);

            if (job.Status != JobStatus.Active)
                throw new DomainException($"Job {job.JobID} must be Active. Current: {job.Status}.");

            var workOrder = await workOrderRepo.GetByIdAsync(job.WOID, ct)
                ?? throw new EntityNotFoundException(nameof(WorkOrder), job.WOID);

            workOrder.AccumulateOutput(cmd.QtyOk, cmd.QtyNg, job.OperatorID);

            var log = ProductionLog.Create(cmd.JobId, cmd.QtyOk, cmd.QtyNg, cmd.IdempotencyKey, cmd.Notes);

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

            return ValidationResult<SubmitOutputResult>.Ok(
                new SubmitOutputResult(log.LogID, workOrder.ActualQtyOK.Value, workOrder.ActualQtyNG.Value));
        }
        catch (EntityNotFoundException ex)
        {
            return ValidationResult<SubmitOutputResult>.NotFound(ex.Message);
        }
        catch (DomainException ex)
        {
            return ValidationResult<SubmitOutputResult>.Failure(ex.Message);
        }
    }
}
```

### 7.4. Controller action

```csharp
[HttpPost]
[ProducesResponseType<SubmitOutputResult>(StatusCodes.Status200OK)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
public async Task<IActionResult> SubmitOutput([FromBody] SubmitOutputRequest req, CancellationToken ct)
{
    var result = await commandMediator.SendAsync(
        new SubmitOutputCommand(req.JobId, req.QtyOk, req.QtyNg, req.Notes, req.IdempotencyKey, req.Defects),
        null, ct);
    if (!result.IsSuccess) return result.ToErrorResult();
    return Ok(result.Value!);
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

Every type used in an API response **must** be registered in `AeroMes.Api/Serialization/AeroMesJsonContext.cs`:

```csharp
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

**Mandatory:** when creating a new endpoint, add `[JsonSerializable(typeof(ApiResponse<YourDto>))]` to `AeroMesJsonContext` in the same PR.

### 8.3. No reflection in application code

| Forbidden | Replace with |
|---|---|
| `dynamic` | Explicit type or generic |
| `Activator.CreateInstance(type)` | Factory method / DI |
| `Type.GetProperties()` / `PropertyInfo` | Not needed — use record constructor |
| AutoMapper (reflection-based) | Explicit mapping in a static extension method |
| `Enum.GetName(value)` in hot path | Pre-built `static readonly Dictionary<TEnum, string>` |

### 8.4. Extension method mapping instead of AutoMapper

All Entity → DTO mappings use a static extension method co-located with the DTO:

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
