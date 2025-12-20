using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Kantin_Paramadina.Hubs;

[Authorize]
public class TransactionHub : Hub
{
    // Client can call this to join group for specific outlet
    public async Task JoinOutletGroup(string outletId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, GetGroupName(outletId));
    }

    // Client can leave group
    public async Task LeaveOutletGroup(string outletId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetGroupName(outletId));
    }

    private string GetGroupName(string outletId) => $"Outlet_{outletId}";
}
