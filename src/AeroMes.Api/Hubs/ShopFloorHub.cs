using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace AeroMes.Api.Hubs;

[Authorize]
public class ShopFloorHub : Hub
{
    public Task JoinFactory() =>
        Groups.AddToGroupAsync(Context.ConnectionId, "factory");

    public Task JoinWorkCenter(int workCenterId) =>
        Groups.AddToGroupAsync(Context.ConnectionId, $"wc:{workCenterId}");

    public Task LeaveWorkCenter(int workCenterId) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, $"wc:{workCenterId}");

    public Task JoinMachine(string machineCode) =>
        Groups.AddToGroupAsync(Context.ConnectionId, $"machine:{machineCode}");

    public Task LeaveMachine(string machineCode) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, $"machine:{machineCode}");

    public override Task OnConnectedAsync() => JoinFactory();
}
