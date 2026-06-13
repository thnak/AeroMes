using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace AeroMes.Api.Hubs;

[Authorize]
public class IotHub : Hub
{
    public Task SubscribeMachine(string machineCode) =>
        Groups.AddToGroupAsync(Context.ConnectionId, $"machine:{machineCode}");

    public Task UnsubscribeMachine(string machineCode) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, $"machine:{machineCode}");

    public override Task OnConnectedAsync() => base.OnConnectedAsync();
}
