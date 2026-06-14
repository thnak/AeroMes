using AeroMes.Api.Hubs;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Alert;
using Microsoft.AspNetCore.SignalR;

namespace AeroMes.Api.Services;

public class AlertHubNotifier(IHubContext<ShopFloorHub> hub) : IAlertNotifier
{
    public Task PushAsync(AlertEventDto alert, CancellationToken ct = default) =>
        hub.Clients.Group("factory").SendAsync("AlertTriggered", alert, ct);
}
