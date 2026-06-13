using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Modules.Queries.GetModuleStatus;

public record GetModuleStatusQuery(IReadOnlySet<string>? AllowedModuleIds = null)
    : IQuery<ModuleStatusResponse>;

public record ModuleStatusResponse(
    DateTime GeneratedAt,
    IReadOnlyList<ModuleStatusDto> Modules,
    IReadOnlyList<AlertItemDto> Alerts);

public record ModuleStatusDto(string Id, IReadOnlyList<BadgeDto> Badges);

public record BadgeDto(string Key, int Count, string Severity, string Label);

public record AlertItemDto(string Severity, string Message, string Href);
