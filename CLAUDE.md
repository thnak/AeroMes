# AeroMes — Project Instructions for Claude

## Coding standards

All code must follow **[code-style.md](./code-style.md)**. Read it before writing any new feature or modifying existing code. Key points:

- Project prefix is `AeroMes.*` — never `Mes.*`.
- Host layer is `AeroMes.Api`, not `Mes.Web`.
- Domain is organized by bounded context: `Master/`, `Production/`, `Quality/`, `Integration/`.
- Each use-case lives in its own folder: `Commands/{UseCaseName}/` with Command + Handler + Validator co-located.
- Handlers use **Repository interfaces + IUnitOfWork** — never inject `AppDbContext` directly.
- Return strongly-typed result records from Commands. No `Result<T>` wrapper.
- Error handling is via **throw** (`DomainException`, `EntityNotFoundException`) — `ExceptionMiddleware` converts to RFC 7807. No try/catch in Handlers.
- Only one MediatR pipeline behavior: `ValidationBehavior`. Do not add logging or transaction behaviors without discussion.

## Project layout

```
src/
  AeroMes.Api/           ← Controllers, Middleware, Program.cs, Identity/TokenService
  AeroMes.Application/   ← CQRS handlers, validators, interfaces, behaviors
  AeroMes.Infrastructure/← DbContext, repositories, migrations
  AeroMes.Domain/        ← Entities, domain events, exceptions
web/                     ← Vite + React frontend
```

## Tech stack

- .NET 10 / C# 13, ASP.NET Core MVC (not Minimal APIs)
- MediatR + FluentValidation
- EF Core 10 + SQL Server (JSON columns via `.ToJson()`, soft delete via `HasQueryFilter`)
- ASP.NET Core Identity — dual auth: Cookie (Web) + JWT Bearer (PDA/API)
- No Dapper yet — use `AsNoTracking()` for read queries

## Rules

- File-scoped namespaces everywhere.
- Primary constructors for DI injection.
- Collection expressions `[...]` instead of `new List<T>()`.
- No comments unless the WHY is non-obvious.
- No `Result<T>` — throw domain exceptions instead.
- Vietnamese log/error messages are acceptable; English preferred for code identifiers.

## Cold-start & serialization (reduce reflection at startup)

- **No anonymous types** in LINQ projections, Handler return values, or anywhere a type crosses a method boundary. Use named `record` types.
- **JSON source generators required** for all API response types. Every new DTO/response record must be registered in `AeroMes.Api/Serialization/AeroMesJsonContext.cs` with `[JsonSerializable(typeof(T))]`.
- **No `dynamic`, no `Activator.CreateInstance`, no `Type.GetProperties()`** in application code.
- **No AutoMapper** — use explicit constructor-based mapping or `record` copy expressions.
- See **Section 8** in `code-style.md` for patterns and examples.
