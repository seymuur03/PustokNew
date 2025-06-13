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
    public class GenreController(PustokDbContext _pustokDbContext) : Controller
    {
        public IActionResult Index(int page = 1, int take = 2)
        {
            try
            {
                var query = _pustokDbContext.Genres.AsQueryable();
                PaginatedList<Genre> paginatedList = PaginatedList<Genre>.PaginationMethod(query, take, page);
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

                var genre = _pustokDbContext.Genres.FirstOrDefault(x => x.Id == id);
                if (genre == null)
                    return NotFound();

                _pustokDbContext.Genres.Remove(genre);
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
        public IActionResult Create(Genre genre)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View();

                var existg = _pustokDbContext.Genres.FirstOrDefault(eg => eg.Name.ToUpper() == genre.Name.ToUpper());
                if (existg is not null)
                {
                    ModelState.AddModelError("Name", "Bu adda janr artıq mövcuddur");
                    return View();
                }

                _pustokDbContext.Genres.Add(genre);
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

                var genre = _pustokDbContext.Genres.FirstOrDefault(g => g.Id == id);
                if (genre == null)
                    return NotFound();

                return View(genre);
            }
            catch (Exception)
            {
                return StatusCode(500, "Form yüklənərkən xəta baş verdi");
            }
        }

        [HttpPost]
        public IActionResult Edit(Genre genre)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View();

                var existG = _pustokDbContext.Genres.FirstOrDefault(eg => eg.Id == genre.Id);
                if (existG is null)
                    return NotFound();

                var existg = _pustokDbContext.Genres.FirstOrDefault(eg =>
                    eg.Name.ToUpper() == genre.Name.ToUpper() && eg.Id != genre.Id);

                if (existg is not null)
                {
                    ModelState.AddModelError("Name", "Bu adda janr artıq mövcuddur");
                    return View();
                }

                existG.Name = genre.Name;
                _pustokDbContext.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Redaktə zamanı xəta baş verdi");
                return View(genre);
            }
        }

        public IActionResult Detail(int? id)
        {
            try
            {
                if (id == null)
                    return NotFound();

                var genre = _pustokDbContext.Genres
                    .Include(g => g.Books)
                    .FirstOrDefault(g => g.Id == id);

                if (genre == null)
                    return NotFound();

                return View(genre);
            }
            catch (Exception)
            {
                return StatusCode(500, "Detallar yüklənərkən xəta baş verdi");
            }
        }
    }
}
