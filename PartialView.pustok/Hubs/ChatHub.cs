using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using PartialView.pustok.Models;

namespace PartialView.pustok.Hubs
{
    public class ChatHub(UserManager<AppUser>userManager):Hub
    {
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public override Task OnConnectedAsync()
        {
            if (Context.User.Identity.IsAuthenticated)
            {
                var user = userManager.FindByNameAsync(Context.User.Identity.Name).Result;
                user.ConnectionId = Context.ConnectionId;
                var result = userManager.UpdateAsync(user).Result;
                Clients.All.SendAsync("UserConnect", user.Id);
            }
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            if (Context.User.Identity.IsAuthenticated)
            {
                var user = userManager.FindByNameAsync(Context.User.Identity.Name).Result;
                user.ConnectionId = null;   
                var result = userManager.UpdateAsync(user).Result;
                Clients.All.SendAsync("UserDisConnect", user.Id);

            }
            return base.OnDisconnectedAsync(exception);
        }


    }
}
