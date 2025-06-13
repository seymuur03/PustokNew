using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PartialView.pustok.DATA;
using PartialView.pustok.Helpers;
using PartialView.pustok.Models;

namespace PartialView.pustok.Areas.Manage.Controllers
{
    [Area("Manage")]
    [Authorize(Roles = "Admin")]
    public class AuthorController(PustokDbContext _pustokDbContext) : Controller
    {
        public IActionResult Index(int page = 1, int take = 2)
        {
            try
            {
                var query = _pustokDbContext.Authors.AsQueryable();
                PaginatedList<Author> paginatedList = PaginatedList<Author>.PaginationMethod(query, take, page);
                return View(paginatedList);
            }
            catch (Exception)
            {
                return StatusCode(500, "Janr siyahısı yüklənərkən xəta baş verdi");
            }
        }

        public IActionResult Delete(int? id)
        {
            try
            {
                if (id == null)
                    return NotFound();

                var Author = _pustokDbContext.Authors.FirstOrDefault(x => x.Id == id);
                if (Author == null)
                    return NotFound();

                _pustokDbContext.Authors.Remove(Author);
                _pustokDbContext.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                return StatusCode(500, "Janr silinərkən xəta baş verdi");
            }
        }

        public IActionResult Create()
        {
            try
            {
                return View();
            }
            catch (Exception)
            {
                return StatusCode(500, "Form yüklənərkən xəta baş verdi");
            }
        }

        [HttpPost]
        public IActionResult Create(Author Author)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View();

                var existg = _pustokDbContext.Authors.FirstOrDefault(eg => eg.Name.ToUpper() == Author.Name.ToUpper());
                if (existg is not null)
                {
                    ModelState.AddModelError("Name", "Bu adda janr artıq mövcuddur");
                    return View();
                }

                _pustokDbContext.Authors.Add(Author);
                _pustokDbContext.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Janr əlavə olunarkən xəta baş verdi");
                return View();
            }
        }

        public IActionResult Edit(int? id)
        {
            try
            {
                if (id == null)
                    return NotFound();

                var Author = _pustokDbContext.Authors.FirstOrDefault(g => g.Id == id);
                if (Author == null)
                    return NotFound();

                return View(Author);
            }
            catch (Exception)
            {
                return StatusCode(500, "Form yüklənərkən xəta baş verdi");
            }
        }

        [HttpPost]
        public IActionResult Edit(Author Author)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View();

                var existG = _pustokDbContext.Authors.FirstOrDefault(eg => eg.Id == Author.Id);
                if (existG is null)
                    return NotFound();

                var existg = _pustokDbContext.Authors.FirstOrDefault(eg =>
                    eg.Name.ToUpper() == Author.Name.ToUpper() && eg.Id != Author.Id);

                if (existg is not null)
                {
                    ModelState.AddModelError("Name", "Bu adda janr artıq mövcuddur");
                    return View();
                }

                existG.Name = Author.Name;
                _pustokDbContext.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Redaktə zamanı xəta baş verdi");
                return View(Author);
            }
        }

        public IActionResult Detail(int? id)
        {
            try
            {
                if (id == null)
                    return NotFound();

                var Author = _pustokDbContext.Authors
                    .Include(g => g.Books)
                    .FirstOrDefault(g => g.Id == id);

                if (Author == null)
                    return NotFound();

                return View(Author);
            }
            catch (Exception)
            {
                return StatusCode(500, "Detallar yüklənərkən xəta baş verdi");
            }
        }
    }
}
