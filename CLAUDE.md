# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

### Backend (.NET)
```bash
dotnet build src/AeroMes.Api/               # build everything
dotnet run --project src/AeroMes.Api/       # API on http://localhost:5170, Scalar at /scalar
dotnet test tests/AeroMes.IntegrationTests/ # run all integration tests (requires Docker for SQL Server)
dotnet test tests/AeroMes.IntegrationTests/ --filter "FullyQualifiedName~AuthFlowTests"  # single test class
dotnet ef migrations add <Name> --project src/AeroMes.Infrastructure --startup-project src/AeroMes.Api
dotnet ef database update --project src/AeroMes.Infrastructure --startup-project src/AeroMes.Api
```

### Frontend (web/)
```bash
cd web && npm install
npm run dev          # Vite dev server on http://localhost:5173 (proxies /api → :5170)
npm run build        # TypeScript compile + Vite build → outputs to src/AeroMes.Api/wwwroot
npm run lint         # ESLint
npm run fetch:spec   # Pull live OpenAPI spec from running API to ../aeromes-openapi.json
npm run generate:api # Run Orval codegen from spec snapshot → src/api/
```

> Integration tests spin up a real SQL Server via Testcontainers — Docker must be running.
> The API auto-migrates and seeds the database on first `dotnet run`.

## Architecture

**Clean Architecture** with four layers. The dependency rule is strict: Domain ← Application ← Infrastructure, API.

```
src/AeroMes.Domain/         Zero external dependencies. Entities, value objects, domain exceptions.
src/AeroMes.Application/    CQRS handlers + validators + repository interfaces.
src/AeroMes.Infrastructure/ EF Core DbContext, repository implementations, Identity store.
src/AeroMes.Api/            Controllers, middleware, Program.cs, TokenService.
web/                        Vite + React 19 + MUI v9. Codegen via Orval.
tests/AeroMes.IntegrationTests/ xUnit + WebApplicationFactory + Testcontainers SQL Server.
```

### CQRS — LiteBus v5 (not MediatR)

The project uses **LiteBus**, not MediatR. Key interface mapping:

| Concept | Interface |
|---|---|
| Command (no result) | `: ICommand` |
| Command (with result) | `: ICommand<TResult>` |
| Query | `: IQuery<TResult>` |
| Command handler | `: ICommandHandler<TCmd>` or `ICommandHandler<TCmd, TResult>` |
| Query handler | `: IQueryHandler<TQuery, TResult>` |
| Handler method | `HandleAsync(T message, CancellationToken ct)` |
| Inject in controllers | `ICommandMediator` / `IQueryMediator` (separate — never `IMediator`) |
| Send command | `await commandMediator.SendAsync(cmd, null, ct)` |
| Send query | `await queryMediator.QueryAsync(query, null, ct)` |

**Validation pattern (no pre-handler):** Validators are registered via `AddValidatorsFromAssembly` in `Application/DependencyInjection.cs`. Handlers inject `IValidator<TCommand>` and call it explicitly at the top of `HandleAsync`. There is no LiteBus pre-handler — do not add one.

Domain events are dispatched in `AppDbContext.SaveChangesAsync` via `IEventMediator.PublishAsync`. `IDomainEvent` is a plain marker interface (no LiteBus inheritance needed — events are POCOs).

### Repository + UnitOfWork pattern

- Application layer defines interfaces in `Application/Interfaces/`.
- Infrastructure implements them; `AppDbContext` also implements `IUnitOfWork`.
- Handlers inject repository interfaces + `IUnitOfWork`. Never inject `AppDbContext` directly.
- Read queries use `.AsNoTracking()`.

### Domain structure (bounded contexts)

```
Domain/Auth/        Permissions, roles, refresh tokens, audit log
Domain/Master/      Products, BOM, machines, work centers, operations, routings
Domain/Production/  Work orders, jobs, downtime, production logs, inventory
Domain/Quality/     Defect codes and details
Domain/Integration/ Sales orders and production orders (ERP boundary)
```

### Auth

Dual-scheme: Cookie for the web frontend, JWT Bearer for PDA/API clients. Both share the same Identity pipeline. Three middleware gates run in order: `ExceptionMiddleware` → `ForcePasswordChangeMiddleware` → `MfaEnforcementMiddleware`. Authorization uses per-permission policies (`permission:{code}`) via `[RequirePermission(...)]`.

### Frontend API client

Generated hooks live in `web/src/api/` (one file per controller tag). All calls go through `web/src/lib/apiClient.ts` (Axios + JWT interceptor + auto-refresh). The Orval mutator at `web/src/lib/orvalMutator.ts` bridges codegen to that client. After changing API contracts, run `npm run fetch:spec && npm run generate:api`.

## Coding standards

Run `/code-style` before writing any new feature. Key rules:

- Project prefix is `AeroMes.*` — never `Mes.*`.
- Each use-case lives in its own folder: `Application/{Context}/{Commands|Queries}/{UseCaseName}/` — Command/Query record + Handler + Validator co-located.
- Commands return `ValidationResult<T>` (defined in `Application/Common/ValidationResult.cs`). Use `ValidationResult<Unit>` for void commands. Queries return their result type directly — no wrapper.
- Error handling in command handlers: run `validator.ValidateAsync` → return `ValidationResult<T>.Invalid(...)` on failure. Wrap domain logic in `try/catch (EntityNotFoundException)` → `NotFound(ex.Message)` and `try/catch (DomainException)` → `Failure(ex.Message)`. Return `ValidationResult<T>.Ok(result)` on success. No unhandled throws from handlers.
- Controllers unwrap results: `if (!result.IsSuccess) return result.ToErrorResult();` then `return Ok(result.Value!);`. Extension method lives in `Api/Extensions/ValidationResultExtensions.cs`.
- File-scoped namespaces. Primary constructors for DI. Collection expressions `[...]` over `new List<T>()`.
- Vietnamese log/error messages are acceptable; English preferred for identifiers.

## Serialization (cold-start / AOT safety)

- **No anonymous types** in LINQ projections or across method boundaries — use named `record` types.
- **JSON source generators required**: every new DTO/response record must be registered in `src/AeroMes.Api/Serialization/AeroMesJsonContext.cs` with `[JsonSerializable(typeof(T))]`.
- No `dynamic`, `Activator.CreateInstance`, or `Type.GetProperties()` in application code.
- No AutoMapper — explicit constructor-based mapping or `record` copy expressions only.
