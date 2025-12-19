using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BugraLife.Models;
using BugraLife.DBContext;
using Microsoft.AspNetCore.Authorization;

namespace BugraLife.Controllers
{
    [Authorize]
    public class LocationController : Controller
    {
        private readonly BugraLifeDBContext _context;

        public LocationController(BugraLifeDBContext context)
        {
            _context = context;
        }

        // 1. LİSTELEME
        public async Task<IActionResult> Index()
        {
            // İsim sırasına göre getirelim
            var list = await _context.Locations
                .OrderBy(x => x.location_name)
                .ToListAsync();

            return View(list);
        }

        // 2. EKLEME (POST - AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Location location)
        {
            if (ModelState.IsValid)
            {
                // Aynı isimde konum var mı?
                bool exists = await _context.Locations.AnyAsync(x => x.location_name == location.location_name);
                if (exists)
                {
                    return Json(new { success = false, message = "Bu konum adı zaten kayıtlı!" });
                }

                _context.Locations.Add(location);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Konum başarıyla eklendi." });
            }
            return Json(new { success = false, message = "Form verileri eksik." });
        }

        // 3. GÜNCELLEME (POST - AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Location location)
        {
            if (ModelState.IsValid)
            {
                // Kendisi hariç aynı isimde var mı?
                bool exists = await _context.Locations.AnyAsync(x => x.location_name == location.location_name && x.location_id != location.location_id);
                if (exists)
                {
                    return Json(new { success = false, message = "Bu isimde başka bir konum zaten var!" });
                }

                _context.Locations.Update(location);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Konum güncellendi." });
            }
            return Json(new { success = false, message = "Güncelleme başarısız." });
        }

        // 4. SİLME (POST - AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.Locations.FindAsync(id);
            if (item != null)
            {
                _context.Locations.Remove(item);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Konum silindi." });
            }
            return Json(new { success = false, message = "Kayıt bulunamadı." });
        }

        // HARİTA GÖSTERİM SAYFASI
        // HARİTA GÖSTERİM SAYFASI (Parametreli)
        public async Task<IActionResult> Maps(int? id)
        {
            // 1. Dropdown'ı doldurmak için tüm listeyi çekiyoruz
            ViewBag.LocationList = await _context.Locations
                .OrderBy(x => x.location_name)
                .ToListAsync();

            // 2. Eğer ID geldiyse o kaydı bulup Model olarak sayfaya gönderiyoruz
            if (id.HasValue)
            {
                var selectedLocation = await _context.Locations.FindAsync(id);
                return View(selectedLocation);
            }

            // ID yoksa boş bir model gönderiyoruz (Sayfa ilk açıldığında)
            return View(new Location());
        }
    }
}