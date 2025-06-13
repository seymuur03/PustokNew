using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PartialView.pustok.DATA;
using PartialView.pustok.Helpers;
using PartialView.pustok.Models;

namespace PartialView.pustok.Areas.Manage.Controllers
{
    [Area("Manage")]
    [Authorize(Roles = "Admin")]
    public class BlogController : Controller
    {
        private readonly PustokDbContext pustokDbContext;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public BlogController(PustokDbContext _pustokAppDbContext, IWebHostEnvironment webHostEnvironment)
        {
            pustokDbContext = _pustokAppDbContext;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index(int page = 1, int take = 2)
        {
            var query = pustokDbContext.Blogs.AsQueryable();
            PaginatedList<Blog> paginatedList = PaginatedList<Blog>.PaginationMethod(query, take, page);
            return View(paginatedList);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Blog blog)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View();
                }

                if (blog.Photo == null)
                {
                    ModelState.AddModelError(nameof(blog.Photo), "You must add a photo for blog");
                    return View();
                }

                if (!blog.Photo.ContentType.StartsWith("image/"))
                {
                    ModelState.AddModelError(nameof(blog.Photo), "File must be an image");
                    return View();
                }

                string fileExtension = Path.GetExtension(blog.Photo.FileName);
                string uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                string uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "assets", "image", "others", uniqueFileName);

                using (FileStream stream = new FileStream(uploadPath, FileMode.Create))
                {
                    blog.Photo.CopyTo(stream);
                }

                blog.ImageName = uniqueFileName;

                pustokDbContext.Blogs.Add(blog);
                pustokDbContext.SaveChanges();

                return RedirectToAction("Index", "blog");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred while creating the blog: {ex.Message}");
                return View(blog);
            }
        }

        public IActionResult Edit(int? id)
        {
            if (!id.HasValue)
            {
                return NotFound();
            }

            var blogToEdit = pustokDbContext.Blogs.FirstOrDefault(t => t.Id == id);
            if (blogToEdit == null)
            {
                return NotFound();
            }

            return View(blogToEdit);
        }

        [HttpPost]
        public IActionResult Edit(Blog blog)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(blog);
                }

                var existingblog = pustokDbContext.Blogs.FirstOrDefault(t => t.Id == blog.Id);
                if (existingblog == null)
                {
                    return NotFound();
                }

                string previousImage = existingblog.ImageName;

                if (blog.Photo != null)
                {
                    string extension = Path.GetExtension(blog.Photo.FileName);
                    string imageName = $"{Guid.NewGuid()}{extension}";
                    string savePath = Path.Combine(_webHostEnvironment.WebRootPath, "assets", "image", "others", imageName);

                    using (var stream = new FileStream(savePath, FileMode.Create))
                    {
                        blog.Photo.CopyTo(stream);
                    }

                    existingblog.ImageName = imageName;

                    string oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, "assets", "image", "others", previousImage);

                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                existingblog.Name = blog.Name;
                existingblog.Descripton = blog.Descripton;
                existingblog.AuthorName = blog.AuthorName;

                pustokDbContext.SaveChanges();

                return RedirectToAction("Index", "blog");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred while editing the blog: {ex.Message}");
                return View(blog);
            }
        }

        public IActionResult Delete(int id)
        {
            try
            {
                var blog = pustokDbContext.Blogs.FirstOrDefault(t => t.Id == id);
                if (blog == null)
                {
                    return NotFound();
                }

                string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, "assets", "image", "others", blog.ImageName);

                if (!string.IsNullOrEmpty(imagePath) && System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }

                pustokDbContext.Blogs.Remove(blog);
                pustokDbContext.SaveChanges();

                return RedirectToAction("Index", "blog");
            }
            catch (Exception ex)
            {
                // Opsiyonel olaraq TempData və ya ViewBag ilə mesaj göndərə bilərsən
                TempData["Error"] = $"Error while deleting blog: {ex.Message}";
                return RedirectToAction("Index", "blog");
            }
        }

        public IActionResult Detail(int? id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound();
                }
                var eblog = pustokDbContext.Blogs.FirstOrDefault(t => t.Id == id);
                if (eblog is null)
                    return NotFound();

                return View(eblog);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error while loading blog detail: {ex.Message}";
                return RedirectToAction("Index");
            }
        }
    }
}
