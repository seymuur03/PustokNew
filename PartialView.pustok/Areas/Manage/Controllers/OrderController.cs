using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PartialView.pustok.DATA;
using PartialView.pustok.Helpers;
using PartialView.pustok.Hubs;
using PartialView.pustok.Models;

namespace PartialView.pustok.Areas.Manage.Controllers
{
    [Area("Manage")]
    public class OrderController(PustokDbContext pustokDbContext,
        IHubContext<ChatHub> hubContext ): Controller
    {
        public IActionResult Index(int page = 1, int take = 2)
        {
                var query = pustokDbContext.Orders.Include(o=>o.AppUser).AsQueryable();
                PaginatedList<Order> paginatedList = PaginatedList<Order>.PaginationMethod(query, take, page);
                return View(paginatedList);
            
        }

        public IActionResult Accept(int? id)
        {
           if (id == null)
            {
                return NotFound();
            }

            Order order = pustokDbContext.Orders.Include(o=>o.AppUser).FirstOrDefault(o=>o.Id==id);
            if (order == null)
            {
                return NotFound();
            }

            order.OrderStatus = OrderStatus.Accepted;
            hubContext.Clients.Client(order.AppUser.ConnectionId)
                .SendAsync("OrderAccepted");
            pustokDbContext.SaveChanges();
            return RedirectToAction(nameof(Index));

        }

        public IActionResult Reject(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Order order = pustokDbContext.Orders.Include(o=>o.AppUser).FirstOrDefault(o=>o.Id== id);
            if (order == null)
            {
                return NotFound();
            }

            order.OrderStatus = OrderStatus.Rejected;
            hubContext.Clients.Client(order.AppUser.ConnectionId)
              .SendAsync("OrderRejected");
            pustokDbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}
