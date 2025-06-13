using Microsoft.AspNetCore.Mvc;

namespace Pustok.MvcProject.Controllers
{
    public class ContactController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
