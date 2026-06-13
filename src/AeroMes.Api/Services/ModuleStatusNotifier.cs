using AeroMes.Application.Interfaces;
using AeroMes.Application.Modules.Queries.GetModuleStatus;
using AeroMes.Api.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace AeroMes.Api.Services;

public record ModuleStatusUpdatedPayload(string ModuleId, IReadOnlyList<BadgeDto> Badges);

public class ModuleStatusNotifier(IHubContext<ModuleStatusHub> hub) : IModuleStatusNotifier
{
    public Task NotifyModuleUpdatedAsync(string moduleId, IReadOnlyList<BadgeDto> badges, CancellationToken ct) =>
        hub.Clients.All.SendAsync(
            "ModuleStatusUpdated",
            new ModuleStatusUpdatedPayload(moduleId, badges),
            ct);
}
