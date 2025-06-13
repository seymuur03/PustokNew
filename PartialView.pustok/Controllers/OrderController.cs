using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PartialView.pustok.DATA;
using PartialView.pustok.Models;
using PartialView.pustok.ViewModels;

namespace PartialView.pustok.Controllers
{
    public class OrderController : Controller
    {
        private readonly PustokDbContext _pustokDbContext;
        private readonly UserManager<AppUser> _userManager;

        public OrderController(PustokDbContext pustokDbContext, UserManager<AppUser> userManager)
        {
            _pustokDbContext = pustokDbContext;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Member")]
        public IActionResult Checkout()
        {
            try
            {
                var user = _userManager.Users
                    .Include(u => u.DbBaskets)
                    .ThenInclude(b => b.Book)
                    .FirstOrDefault(u => u.UserName == User.Identity.Name);

                if (user == null || user.DbBaskets == null || !user.DbBaskets.Any())
                {
                    return RedirectToAction("Index", "Home");
                }

                var checkoutVm = new CheckoutVm
                {
                    CheckoutItems = user.DbBaskets.Select(db => new CheckoutItemVm
                    {
                        Name = db.Book.Title,
                        Count = db.Count,
                        Price = db.Book.Price - ((db.Book.Discount * db.Book.Price) / 100)
                    }).ToList(),
                    TotalPrice = user.DbBaskets.Sum(db =>
                        (db.Book.Price - ((db.Book.Discount * db.Book.Price) / 100)) * db.Count)
                };

                return View(checkoutVm);
            }
            catch (Exception)
            {
                return StatusCode(500, "Yoxlama səhifəsi yüklənərkən xəta baş verdi.");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Member")]
        public IActionResult Checkout(OrderVm orderVm)
        {
            try
            {
                var user = _userManager.Users
                    .Include(u => u.DbBaskets)
                    .ThenInclude(b => b.Book)
                    .FirstOrDefault(u => u.UserName == User.Identity.Name);

                if (user == null || user.DbBaskets == null || !user.DbBaskets.Any())
                {
                    return RedirectToAction("Index", "Home");
                }

                var checkoutVm = new CheckoutVm
                {
                    CheckoutItems = user.DbBaskets.Select(db => new CheckoutItemVm
                    {
                        Name = db.Book.Title,
                        Count = db.Count,
                        Price = db.Book.Price - ((db.Book.Discount * db.Book.Price) / 100)
                    }).ToList(),
                    TotalPrice = user.DbBaskets.Sum(db =>
                        (db.Book.Price - ((db.Book.Discount * db.Book.Price) / 100)) * db.Count),
                    OrderVm = orderVm
                };

                if (!ModelState.IsValid)
                {
                    return View(checkoutVm);
                }

                var order = new Order
                {
                    TotalPrice = checkoutVm.TotalPrice,
                    Address = orderVm.Address,
                    ZipCode = orderVm.ZipCode,
                    Town = orderVm.Town,
                    City = orderVm.City,
                    CreatedDate = DateTime.Now,
                    AppUserId = user.Id,
                    OrderItems = user.DbBaskets.Select(db => new OrderItem
                    {
                        BookId = db.Book.Id,
                        Count = db.Count
                    }).ToList()
                };

                _pustokDbContext.Orders.Add(order);
                _pustokDbContext.DbBaskets.RemoveRange(user.DbBaskets);
                _pustokDbContext.SaveChanges();

                return RedirectToAction("Profile", "Account", new { tab = "orders" });
            }
            catch (Exception)
            {
                return StatusCode(500, "Sifariş göndərilərkən xəta baş verdi.");
            }
        }
    }
}
