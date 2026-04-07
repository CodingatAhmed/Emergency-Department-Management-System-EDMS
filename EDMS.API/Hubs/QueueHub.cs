using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace EDMS.API.Hubs;

[Authorize]
public class QueueHub : Hub
{
    public Task JoinGroup(string groupName) => Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    public Task LeaveGroup(string groupName) => Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
}
