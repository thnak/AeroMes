using AeroMes.Application.Modules.Queries.GetModuleStatus;

namespace AeroMes.Application.Interfaces;

public interface IModuleStatusNotifier
{
    Task NotifyModuleUpdatedAsync(string moduleId, IReadOnlyList<BadgeDto> badges, CancellationToken ct = default);
}
