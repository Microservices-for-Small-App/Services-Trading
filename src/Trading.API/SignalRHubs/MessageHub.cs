using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Trading.API.StateMachines;

namespace Trading.API.SignalRHubs;

[Authorize]
public class MessageHub : Hub
{
    public async Task SendStatusAsync(PurchaseState status)
    {
        if (Clients is not null)
        {
            await Clients.User(Context.UserIdentifier!).SendAsync("ReceivePurchaseStatus", status);
        }
    }
}