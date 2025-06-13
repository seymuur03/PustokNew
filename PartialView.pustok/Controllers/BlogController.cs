using Microsoft.AspNetCore.Mvc;
using PartialView.pustok.DATA;

namespace Pustok.MvcProject.Controllers
{
    public class BlogController : Controller
    {
        private readonly PustokDbContext _pustokDbContext;

        public BlogController(PustokDbContext pustokDbContext)
        {
            _pustokDbContext = pustokDbContext;
        }

        public IActionResult Index()
        {
            try
            {
                var blogs = _pustokDbContext.Blogs.ToList();
                return View(blogs);
            }
            catch (Exception)
            {
                return StatusCode(500, "Bloqlar yüklənərkən xəta baş verdi");
            }
        }

        public IActionResult Detail(int? id)
        {
            try
            {
                if (id == null)
                    return NotFound();

                var blog = _pustokDbContext.Blogs.FirstOrDefault(b => b.Id == id);
                if (blog == null)
                    return NotFound();

                return View(blog);
            }
            catch (Exception)
            {
                return StatusCode(500, "Bloq detalları göstərilərkən xəta baş verdi");
            }
        }
    }
}
