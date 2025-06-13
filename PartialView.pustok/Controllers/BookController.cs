using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PartialView.pustok.DATA;
using PartialView.pustok.Models;
using PartialView.pustok.ViewModels.BookFolder;

namespace PartialView.pustok.Controllers
{
    public class BookController : Controller
    {
        private readonly PustokDbContext _pustokDbContext;
        private readonly UserManager<AppUser> _userManager;

        public BookController(PustokDbContext pustokDbContext, UserManager<AppUser> userManager)
        {
            _pustokDbContext = pustokDbContext;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Detail(int? id)
        {
            try
            {
                if (id is null)
                    return NotFound();

                var user = _userManager.GetUserAsync(User).Result;
                var vm = user != null ? GetBookDetailVm(id, user.Id) : GetBookDetailVm(id);

                if (vm.Book is null)
                    return NotFound();

                return View(vm);
            }
            catch (Exception)
            {
                return StatusCode(500, "Kitab detalları yüklənərkən xəta baş verdi.");
            }
        }

        public IActionResult BookModal(int? id)
        {
            try
            {
                if (id == null)
                    return Content("Id null ola bilməz");

                var eBook = _pustokDbContext.Books
                    .Include(b => b.Author)
                    .Include(b => b.Genre)
                    .Include(b => b.BookImages)
                    .Include(b => b.BookTags).ThenInclude(bt => bt.Tag)
                    .FirstOrDefault(b => b.Id == id);

                if (eBook == null)
                    return NotFound();

                return PartialView("_ModalBookPartialView", eBook);
            }
            catch (Exception)
            {
                return StatusCode(500, "Modal məlumatları yüklənərkən xəta baş verdi.");
            }
        }

        [Authorize(Roles = "Member")]
        [HttpPost]
        public IActionResult AddComment(BookComment bookComment)
        {
            try
            {
                if (!_pustokDbContext.Books.Any(b => b.Id == bookComment.BookId))
                    return NotFound();

                var user = _userManager.GetUserAsync(User).Result;
                if (user == null)
                {
                    return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Detail", "Book", new { id = bookComment.BookId }) });
                }

                if (!ModelState.IsValid)
                {
                    var generalVm = GetBookDetailVm(bookComment.BookId, user.Id);
                    generalVm.BookComment = bookComment;
                    return View("Detail", generalVm);
                }

                bookComment.AppUserId = user.Id;
                _pustokDbContext.BookComments.Add(bookComment);
                _pustokDbContext.SaveChanges();

                return RedirectToAction("Detail", new { id = bookComment.BookId });
            }
            catch (Exception)
            {
                return StatusCode(500, "Şərh əlavə edilərkən xəta baş verdi.");
            }
        }

        public IActionResult DeleteComment(int? id)
        {
            try
            {
                if (id is null)
                    return NotFound();

                var user = _userManager.GetUserAsync(User).Result;
                var dComment = _pustokDbContext.BookComments.Include(bc => bc.AppUser).FirstOrDefault(bc => bc.Id == id);

                if (dComment == null)
                    return NotFound();

                if (dComment.AppUserId != user.Id)
                    return Content("Bu şərhi silmək icazəniz yoxdur");

                _pustokDbContext.BookComments.Remove(dComment);
                _pustokDbContext.SaveChanges();

                return RedirectToAction("Detail", new { id = dComment.BookId });
            }
            catch (Exception)
            {
                return StatusCode(500, "Şərh silinərkən xəta baş verdi.");
            }
        }

        private BookDetailVm GetBookDetailVm(int? bookId, string userId = null)
        {
            var eBook = _pustokDbContext.Books
                .Include(b => b.Author)
                .Include(b => b.Genre)
                .Include(b => b.BookImages)
                .Include(b => b.BookTags).ThenInclude(bt => bt.Tag)
                .Include(b => b.BookComments).ThenInclude(bc => bc.AppUser)
                .FirstOrDefault(b => b.Id == bookId);

            var relatedBooks = _pustokDbContext.Books
                .Include(b => b.Author)
                .Include(b => b.Genre)
                .Include(b => b.BookImages)
                .Include(b => b.BookTags).ThenInclude(bt => bt.Tag)
                .Include(b => b.BookComments).ThenInclude(bc => bc.AppUser)
                .Where(b => b.GenreId == eBook.GenreId || b.AuthorId == eBook.AuthorId)
                .ToList();

            var vm = new BookDetailVm
            {
                Book = eBook,
                RelatedBooks = relatedBooks,    
                TotalComments = _pustokDbContext.BookComments.Count(bc => bc.BookId == bookId),
                AverageRate = _pustokDbContext.BookComments
                    .Where(bc => bc.BookId == bookId)
                    .Select(bc => (decimal?)bc.Rate)
                    .Average() ?? 0
            };

            if (userId != null)
            {
                vm.HasComment = _pustokDbContext.BookComments
                    .Any(bc => bc.AppUserId == userId && bc.BookId == bookId && bc.CommentStatus != CommentStatus.Rejected);
            }

            return vm;
        }

    }
}
