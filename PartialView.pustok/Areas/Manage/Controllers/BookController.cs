using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NuGet.Common;
using PartialView.pustok.Controllers;
using PartialView.pustok.DATA;
using PartialView.pustok.Helpers;
using PartialView.pustok.Models;
using PartialView.pustok.Services;
using PartialView.pustok.Settings;

namespace PartialView.pustok.Areas.Manage.Controllers
{
    [Area("Manage")]
    [Authorize(Roles = "Admin")]
    public class BookController : Controller
    {
        private readonly PustokDbContext _pustokDbContext;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly EmailService emailService;
        private readonly IOptions<EmailSetting> options;
        public BookController(PustokDbContext pustokDbContext, IWebHostEnvironment webHostEnvironment, EmailService _emailService, IOptions<EmailSetting> _options)
        {
            _pustokDbContext = pustokDbContext;
            _webHostEnvironment = webHostEnvironment;
            emailService = _emailService;
            options = _options;

        }
        public IActionResult Index(int page = 1, int take = 2)
        {
            try
            {
                var query = _pustokDbContext.Books
                    .Include(b => b.BookImages)
                    .Include(b => b.Author)
                    .Include(b => b.Genre)
                    .AsQueryable();

                PaginatedList<Book> paginatedList = PaginatedList<Book>.PaginationMethod(query, take, page);
                return View(paginatedList);
            }
            catch (Exception ex)
            {
                // Loglama əlavə edilə bilər
                return StatusCode(500, "Daxili server xətası");
            }
        }

        public IActionResult Delete(int? id)
        {
            try
            {
                if (id == null) return NotFound();

                var book = _pustokDbContext.Books.Include(b => b.BookImages).FirstOrDefault(x => x.Id == id);
                if (book == null) return NotFound();

                foreach (var BImage in book.BookImages)
                {
                    var deleteImagePath = Path.Combine(_webHostEnvironment.WebRootPath, "assets/image/products", BImage.ImgName);
                    FileManager.DeleteFile(deleteImagePath);
                }

                _pustokDbContext.Books.Remove(book);
                _pustokDbContext.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Silinmə zamanı xəta baş verdi");
            }
        }

        public IActionResult Create()
        {

            try
            {
                ViewBag.Authors = new SelectList(_pustokDbContext.Authors.ToList(), "Id", "Name");
                ViewBag.Genres = new SelectList(_pustokDbContext.Genres.ToList(), "Id", "Name");
                return View();
            }
            catch (Exception)
            {
                return StatusCode(500, "Form yüklənərkən xəta baş verdi");
            }
        }   
        [HttpPost]
        public IActionResult Create(Book book)
        {
            try
            {
                ViewBag.Authors = new SelectList(_pustokDbContext.Authors.ToList(), "Id", "Name");
                ViewBag.Genres = new SelectList(_pustokDbContext.Genres.ToList(), "Id", "Name");

                if (!ModelState.IsValid) return View();

                if (book.Photos is null)
                {
                    ModelState.AddModelError("Photos", "Photos can not be empty");
                    return View();
                }

                foreach (var photo in book.Photos)
                {
                    BookImage bookImage = new BookImage();
                    bookImage.ImgName = photo.SaveImage(_webHostEnvironment.WebRootPath, "assets/image/products");
                    bookImage.BookId = book.Id;
                    book.BookImages.Add(bookImage);
                }

                if (book.MainPhoto is null)
                {
                    ModelState.AddModelError("MainPhoto", "MainPhoto can not be empty");
                    return View();
                }

                BookImage bookMainImage = new()
                {
                    Status = true,
                    ImgName = book.MainPhoto.SaveImage(_webHostEnvironment.WebRootPath, "assets/image/products")
                };
                book.BookImages.Add(bookMainImage);

                if (book.HoverPhoto is null)
                {
                    ModelState.AddModelError("HoverPhoto", "HoverPhoto can not be empty");
                    return View();
                }

                BookImage bookHoverImage = new()
                {
                    Status = false,
                    ImgName = book.HoverPhoto.SaveImage(_webHostEnvironment.WebRootPath, "assets/image/products")
                };
                book.BookImages.Add(bookHoverImage);

                _pustokDbContext.Books.Add(book);
                _pustokDbContext.SaveChanges();

                var url = Url.Action("Detail", "Book", new { area = "", id = book.Id }, Request.Scheme);
                var body = $"<a href='{url}' > New book </a>";
                var emails = _pustokDbContext.Subscribers.Select(s => s.Email).ToList();
                foreach (var email in emails)
                {
                    emailService.SendEmail(email, "New Book", body, "fatalizadaanar@gmail.com", "vrea jmhh khwd wmmk", options.Value);
                }

                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Kitab yaradılarkən xəta baş verdi");
                return View();
            }
        }

        public IActionResult Edit(int?id)
        {
            try
            {
                ViewBag.Authors = new SelectList(_pustokDbContext.Authors.ToList(), "Id", "Name");
                ViewBag.Genres = new SelectList(_pustokDbContext.Genres.ToList(), "Id", "Name");

                if (id is null) return NotFound();

                var book = _pustokDbContext.Books
                    .Include(b => b.BookImages)
                    .Include(b => b.Author)
                    .Include(b => b.Genre)
                    .FirstOrDefault(x => x.Id == id);

                if (book == null) return NotFound();
                return View(book);
            }
            catch (Exception)
            {
                return StatusCode(500, "Form yüklənərkən xəta baş verdi");
            }
        }
        [HttpPost]
        public IActionResult Edit(Book book)
        {
            try
            {
                ViewBag.Authors = new SelectList(_pustokDbContext.Authors.ToList(), "Id", "Name");
                ViewBag.Genres = new SelectList(_pustokDbContext.Genres.ToList(), "Id", "Name");

                var Ebook = _pustokDbContext.Books
                    .Include(b => b.BookImages)
                    .Include(b => b.Author)
                    .Include(b => b.Genre)
                    .FirstOrDefault(x => x.Id == book.Id);

                if (Ebook == null) return NotFound();
                if (!ModelState.IsValid) return View();

                if (book.MainPhoto is not null)
                {
                    var dMain = Ebook.BookImages.FirstOrDefault(x => x.Status == true);
                    if (dMain != null)
                    {
                        _pustokDbContext.BookImages.Remove(dMain);
                        FileManager.DeleteFile(Path.Combine(_webHostEnvironment.WebRootPath, "assets/image/products", dMain.ImgName));
                    }
                    Ebook.BookImages.Add(new BookImage
                    {
                        Status = true,
                        ImgName = book.MainPhoto.SaveImage(_webHostEnvironment.WebRootPath, "assets/image/products")
                    });
                }

                if (book.HoverPhoto is not null)
                {
                    var dHover = Ebook.BookImages.FirstOrDefault(x => x.Status == false);
                    if (dHover != null)
                    {
                        _pustokDbContext.BookImages.Remove(dHover);
                        FileManager.DeleteFile(Path.Combine(_webHostEnvironment.WebRootPath, "assets/image/products", dHover.ImgName));
                    }
                    Ebook.BookImages.Add(new BookImage
                    {
                        Status = false,
                        ImgName = book.HoverPhoto.SaveImage(_webHostEnvironment.WebRootPath, "assets/image/products")
                    });
                }

                var existingIds = Ebook.BookImages.Where(b => b.Status == null).Select(b => b.Id).ToList();
                var toDelete = existingIds.Except(book.ImgIds ?? new List<int>()).ToList();
                foreach (var idToDelete in toDelete)
                {
                    var image = Ebook.BookImages.FirstOrDefault(x => x.Id == idToDelete);
                    if (image != null)
                    {
                        FileManager.DeleteFile(Path.Combine(_webHostEnvironment.WebRootPath, "assets/image/products", image.ImgName));
                        _pustokDbContext.BookImages.Remove(image);
                    }
                }

                if (book.Photos != null)
                {
                    foreach (var photo in book.Photos)
                    {
                        Ebook.BookImages.Add(new BookImage
                        {
                            ImgName = photo.SaveImage(_webHostEnvironment.WebRootPath, "assets/image/products"),
                            BookId = book.Id
                        });
                    }
                }

                Ebook.Title = book.Title;
                Ebook.Price = book.Price;
                Ebook.Description = book.Description;
                Ebook.PCode = book.PCode;
                Ebook.Discount = book.Discount;
                Ebook.Rate = book.Rate;
                Ebook.AuthorId = book.AuthorId;
                Ebook.GenreId = book.GenreId;
                Ebook.IsFeatured = book.IsFeatured;
                Ebook.IsNew = book.IsNew;
                Ebook.InStock = book.InStock;

                _pustokDbContext.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Redaktə zamanı xəta baş verdi");
                return View(book);
            }
        }

        public IActionResult Detail(int?id)
        {
            try
            {
                if (id == null) return NotFound();
                var book = _pustokDbContext.Books
                    .Include(b => b.BookImages)
                    .Include(b => b.Author)
                    .Include(b => b.Genre)
                    .FirstOrDefault(g => g.Id == id);
                if (book == null) return NotFound();

                return View(book);
            }
            catch (Exception)
            {
                return StatusCode(500, "Detallar yüklənərkən xəta baş verdi");
            }
        }
    }
}
