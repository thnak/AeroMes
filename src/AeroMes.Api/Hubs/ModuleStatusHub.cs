using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace AeroMes.Api.Hubs;

[Authorize]
public class ModuleStatusHub : Hub
{
    public override Task OnConnectedAsync() => base.OnConnectedAsync();
}
