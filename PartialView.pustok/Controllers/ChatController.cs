using Microsoft.AspNetCore.Mvc;

namespace PartialView.pustok.Controllers
{
    public class ChatController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
