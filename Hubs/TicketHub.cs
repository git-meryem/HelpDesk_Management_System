using Microsoft.AspNetCore.SignalR;

namespace UserApp.Hubs
{
    public class TicketHub : Hub
    {
        // Cette méthode permet de trier les utilisateurs dans des groupes
        // pour que l'Agent A ne reçoive pas les notifications de l'Agent B.
        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);
            }
            await base.OnConnectedAsync();
        }
    }
}