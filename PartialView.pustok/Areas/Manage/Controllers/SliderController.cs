using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PartialView.pustok.DATA;
using PartialView.pustok.Helpers;
using PartialView.pustok.Models;
using PartialView.pustok.Services;
namespace PartialView.pustok.Areas.Manage.Controllers
{
    [Area("Manage")]
    [Authorize(Roles = "Admin")]
    public class SliderController : Controller
    {
        private readonly PustokDbContext _pustokDbContext;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IOptionPatternService _optionPatternService;

        public SliderController(PustokDbContext pustokDbContext, IWebHostEnvironment webHostEnvironment, IOptionsSnapshot<IOptionPatternService> options)
        {
            _pustokDbContext = pustokDbContext;
            _webHostEnvironment = webHostEnvironment;
            _optionPatternService = options.Value;
        }

        public IActionResult Index(int page = 1, int take = 2)
        {
            try
            {
                var query = _pustokDbContext.Sliders.AsQueryable();
                PaginatedList<Slider> paginatedList = PaginatedList<Slider>.PaginationMethod(query, take, page);
                return View(paginatedList);
            }
            catch (Exception)
            {
                return StatusCode(500, "Slider siyahısı yüklənərkən xəta baş verdi");
            }
        }

        public IActionResult Delete(int? id)
        {
            try
            {
                if (id == null) return NotFound();

                var slider = _pustokDbContext.Sliders.FirstOrDefault(x => x.Id == id);
                if (slider == null) return NotFound();

                var deleteImagePath = Path.Combine(_webHostEnvironment.WebRootPath, "assets/image/bg-images", slider.ImgName);
                FileManager.DeleteFile(deleteImagePath);

                _pustokDbContext.Sliders.Remove(slider);
                _pustokDbContext.SaveChanges();

                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                return StatusCode(500, "Slider silinərkən xəta baş verdi");
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
        public IActionResult Create(Slider slider)
        {
            try
            {
                if (slider == null) return BadRequest("Slider obyekti null-dır");
                if (!ModelState.IsValid) return View();

                if (slider.Photo == null)
                {
                    ModelState.AddModelError("Photo", "Zəhmət olmasa şəkil seçin");
                    return View();
                }

                // Şəkli yadda saxla
                slider.ImgName = slider.Photo.SaveImage(_webHostEnvironment.WebRootPath, "assets/image/bg-images");
                if (slider.ImgName == null)
                {
                    ModelState.AddModelError("Photo", "Şəkil saxlanılmadı");
                    return View();
                }

                _pustokDbContext.Sliders.Add(slider);
                _pustokDbContext.SaveChanges();

                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Slider yaradılarkən xəta baş verdi");
                return View();
            }
        }

        public IActionResult Edit(int? id)
        {
            try
            {
                if (id == null) return NotFound();

                var slider = _pustokDbContext.Sliders.FirstOrDefault(g => g.Id == id);
                if (slider == null) return NotFound();

                return View(slider);
            }
            catch (Exception)
            {
                return StatusCode(500, "Redaktə formu yüklənərkən xəta baş verdi");
            }
        }

        [HttpPost]
        public IActionResult Edit(Slider slider)
        {
            try
            {
                if (!ModelState.IsValid) return View();

                var existSlider = _pustokDbContext.Sliders.FirstOrDefault(g => g.Id == slider.Id);
                if (existSlider == null) return NotFound();

                var file = slider.Photo;
                var oldImageName = existSlider.ImgName;

                if (file != null)
                {
                    // Yeni şəkli yadda saxla
                    var newImageName = file.SaveImage(_webHostEnvironment.WebRootPath, "assets/image/bg-images");
                    if (newImageName == null)
                    {
                        ModelState.AddModelError("Photo", "Yeni şəkil saxlanılmadı");
                        return View(slider);
                    }

                    // Köhnə şəkli sil
                    var deleteImagePath = Path.Combine(_webHostEnvironment.WebRootPath, "assets/image/bg-images", oldImageName);
                    FileManager.DeleteFile(deleteImagePath);

                    existSlider.ImgName = newImageName;
                }

                existSlider.Name = slider.Name;
                existSlider.ButtonLink = slider.ButtonLink;
                existSlider.ButtonText = slider.ButtonText;
                existSlider.Order = slider.Order;
                existSlider.Desc = slider.Desc;

                _pustokDbContext.SaveChanges();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Slider redaktə olunarkən xəta baş verdi");
                return View(slider);
            }
        }

        public IActionResult Detail(int? id)
        {
            try
            {
                if (id == null) return NotFound();

                var slider = _pustokDbContext.Sliders.FirstOrDefault(g => g.Id == id);
                if (slider == null) return NotFound();

                return View(slider);
            }
            catch (Exception)
            {
                return StatusCode(500, "Ətraflı baxış zamanı xəta baş verdi");
            }
        }

        //public IActionResult ReadData()
        //{
        //    try
        //    {
        //        var d1 = _optionPatternService.Key;
        //        var d2 = _optionPatternService.Issuer;

        //        return Json(new
        //        {
        //            Key = d1,
        //            Issuer = d2
        //        });
        //    }
        //    catch (Exception)
        //    {
        //        return StatusCode(500, "Config məlumatları oxunarkən xəta baş verdi");
        //    }
        //}
    }
}

