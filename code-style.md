# TIÊU CHUẨN MÃ NGUỒN & THIẾT KẾ KIẾN TRÚC — AeroMes (.NET 10)

Tài liệu này định nghĩa cấu trúc thư mục, quy chuẩn viết code (Coding Styles), thiết kế CQRS, EF Core, Identity, và hệ thống xử lý lỗi cho dự án AeroMes MES chạy trên nền tảng .NET 10 và C# 13.

---

## 1. Cấu Trúc Giải Pháp (Solution Structure)

Dự án tổ chức theo mô hình **Clean Architecture + Modular Monolith** với 4 project:

```
📁 AeroMes/
│
├── 📁 src/
│   ├── 📁 AeroMes.Api/                     # Host layer: Controllers, Middleware, Program.cs
│   │   ├── 📁 Controllers/                 # MVC Controllers — gọi MediatR, trả ApiResponse
│   │   ├── 📁 Identity/                    # TokenService (JWT generation)
│   │   └── 📁 Middleware/                  # ExceptionMiddleware (RFC 7807 ProblemDetails)
│   │
│   ├── 📁 AeroMes.Infrastructure/          # Hạ tầng vật lý: DB, Repositories, Migrations
│   │   ├── 📁 Data/                        # AppDbContext, IdempotencyStore, Configurations/
│   │   ├── 📁 Repositories/                # Triển khai IRepository từ Domain
│   │   └── 📁 Migrations/                  # EF Core migrations
│   │
│   ├── 📁 AeroMes.Application/             # Logic ứng dụng: CQRS, MediatR, Validators
│   │   ├── 📁 Common/
│   │   │   ├── 📁 Behaviors/               # ValidationBehavior (pipeline MediatR)
│   │   │   └── ApiResponse.cs              # record ApiResponse<T> dùng cho Controllers
│   │   ├── 📁 Interfaces/                  # IUnitOfWork, ITokenService
│   │   └── 📁 {Module}/                    # Mỗi bounded context là một thư mục
│   │       ├── 📁 Commands/
│   │       │   └── 📁 {UseCaseName}/       # Command.cs + Handler.cs + Validator.cs cùng chỗ
│   │       └── 📁 Queries/
│   │           └── 📁 {QueryName}/         # Query.cs + Handler.cs cùng chỗ
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

**Quy tắc module trong Application:**

| Module folder | Bounded context |
|---|---|
| `Production/` | Ghi nhận sản lượng, OEE |
| `WorkOrders/` | Quản lý lệnh sản xuất |
| `Jobs/` | Start/Finish job tại máy |
| `Downtime/` | Ghi nhận downtime |
| `Master/` | Danh mục (Products, Machines, WorkCenters, Operations, Routings...) |

---

## 2. Tiêu Chuẩn Coding Style (C# 13 / .NET 10)

Áp dụng triệt để các tiêu chuẩn hiện đại:

- **File-scoped Namespaces** — tránh thụt lề không cần thiết.
- **Primary Constructors** — dùng cho DI tại Controllers, Handlers, Repositories.
- **Collection Expressions (`[...]`)** — thay thế `new List<T>()` và `new[] {}`.
- **Required Members + Init-only** — ép buộc dữ liệu bắt buộc lúc khởi tạo.
- **Record types** — dùng cho Commands, Queries, DTOs, kết quả (immutable by default).

```csharp
// Ví dụ Entity chuẩn — file-scoped namespace, required, init, collection expression
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

## 3. Thiết Kế CQRS Với MediatR

Phân tách tuyệt đối giữa **Write (Commands)** và **Read (Queries)**.

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

### Quy tắc triển khai

**Commands (Ghi):**
- Phải có `Validator` kèm cùng thư mục use-case.
- Handler dùng **Repository + IUnitOfWork** (không inject `AppDbContext` trực tiếp).
- Trả về strongly-typed result record (không dùng `Result<T>` wrapper).
- Throw `DomainException` hoặc `EntityNotFoundException` — `ExceptionMiddleware` sẽ bắt và trả RFC 7807.

**Queries (Đọc):**
- Dùng `AsNoTracking()` hoặc Dapper. Không qua transaction.
- Có thể trả thẳng DTO/record, không cần result wrapper.

**Pipeline Behavior (MediatR):**
- Chỉ có **`ValidationBehavior`** — tự động chạy FluentValidation trước mọi Handler.
- `ValidationBehavior` throw `ValidationException` → `ExceptionMiddleware` → HTTP 422.

### Cấu trúc use-case folder

```
Commands/
  SubmitOutput/
    SubmitOutputCommand.cs    ← record Command + record Result
    SubmitOutputHandler.cs    ← IRequestHandler
    SubmitOutputValidator.cs  ← AbstractValidator<Command>
```

---

## 4. Cấu Hình EF Core & JSON Column

### 4.1. JSON Column cho thuộc tính động

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

### 4.2. Fluent API — Native JSON + Soft Delete

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

Handlers không inject `AppDbContext` trực tiếp. Mọi write đều qua repository interface và `IUnitOfWork.SaveChangesAsync()`.

```csharp
public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken ct = default);
}
```

---

## 5. Xử Lý Lỗi & API Response

### 5.1. ExceptionMiddleware (RFC 7807 ProblemDetails)

`AeroMes.Api/Middleware/ExceptionMiddleware.cs` bắt toàn bộ exception và trả `application/problem+json`:

| Exception | HTTP Status |
|---|---|
| `ValidationException` (FluentValidation) | 422 Unprocessable Entity |
| `EntityNotFoundException` | 404 Not Found |
| `DomainException` | 422 Unprocessable Entity |
| Unhandled `Exception` | 500 Internal Server Error |

Không cần try/catch trong Handler — throw exception, middleware xử lý.

### 5.2. ApiResponse cho Controllers

```csharp
// AeroMes.Application/Common/ApiResponse.cs
public record ApiResponse<T>(bool Success, string Message, T? Data = default);
public record ApiResponse(bool Success, string Message);
```

Controllers trả `ApiResponse<T>` khi cần bọc data. Các action đơn giản có thể trả trực tiếp DTO.

### 5.3. Domain Exceptions

```csharp
// Dùng trong Handler khi không tìm thấy entity
throw new EntityNotFoundException(nameof(Job), cmd.JobId);

// Dùng trong Domain khi vi phạm business rule
throw new DomainException($"Job {job.JobID} must be Active. Current: {job.Status}.");
```

---

## 6. Identity & Authentication

Dual-scheme authentication: **Cookie** (Web UI) + **JWT Bearer** (PDA / API clients).

- `TokenService` nằm tại `AeroMes.Api/Identity/TokenService.cs`.
- Scheme selector trong `Program.cs`: nếu header có `Authorization: Bearer ...` → JWT, ngược lại → Cookie.
- Cookie name: `AeroMes.Auth`, HttpOnly, SlidingExpiration 8h.

---

## 7. Blueprint Hoàn Chỉnh — SubmitOutput Use Case

Ví dụ thực tế, chạy được: ghi nhận sản lượng OK/NG từ PDA.

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
