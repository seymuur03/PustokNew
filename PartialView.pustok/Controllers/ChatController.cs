using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using PartialView.pustok.Hubs;
using PartialView.pustok.Models;

namespace PartialView.pustok.Controllers
{
    public class ChatController(UserManager<AppUser>userManager,
        IHubContext<ChatHub> hubContext) : Controller
    {
        public IActionResult Index()
        {
            ViewBag.Users =userManager.Users.ToList();
            return View();
        }

        public IActionResult SendUser(string id)
        {
            var eUser = userManager.FindByIdAsync(id).Result;
            if (eUser == null) 
                return NotFound();
            hubContext.Clients.Client(eUser.ConnectionId).SendAsync("SendMessage",id);
            return Content("sended");
        }

        public IActionResult SendUserGroup(List<string> ids)
        {
            foreach (var id in ids)
            {
                var eUser = userManager.FindByIdAsync(id).Result;
                if (eUser == null)
                    return NotFound();
                hubContext.Clients.Client(eUser.ConnectionId).SendAsync("SendMessage", id);
                return Content("sended");

            }
            return Content("not sended");

        }
    }
}
