using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BugraLife.Models;
using BugraLife.DBContext;
using Microsoft.AspNetCore.Authorization;

namespace BugraLife.Controllers
{
    [Authorize]
    public class DailyController : Controller
    {
        private readonly BugraLifeDBContext _context;

        public DailyController(BugraLifeDBContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        // --- 1. TAKVİM VERİLERİNİ GETİR ---
        public async Task<IActionResult> GetEvents()
        {
            var events = await _context.Dailies.Select(x => new
            {
                id = x.daily_id,
                // Başlık olarak Mod'u gösteriyoruz (Örn: "Süper")
                title = x.daily_status == DailyStatus.Kotu ? "Kötü" :
                        x.daily_status == DailyStatus.Orta ? "Orta" :
                        x.daily_status == DailyStatus.Iyi ? "İyi" : "Süper",

                start = x.daily_date.ToString("yyyy-MM-dd"),

                // Renk Ayarı
                backgroundColor = x.daily_status == DailyStatus.Kotu ? "#dc3545" : // Kırmızı
                                  x.daily_status == DailyStatus.Orta ? "#fd7e14" : // Turuncu
                                  x.daily_status == DailyStatus.Iyi ? "#0d6efd" :  // Mavi
                                  "#198754",                                       // Yeşil (Süper)

                borderColor = "transparent",

                // Detaylar
                extendedProps = new
                {
                    description = x.daily_description,
                    statusId = (int)x.daily_status
                }
            }).ToListAsync();

            return Json(events);
        }

        // --- 2. YENİ EKLE ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Daily daily)
        {
            if (!string.IsNullOrEmpty(daily.daily_description))
            {
                // KONTROL: O gün için zaten kayıt var mı?
                bool exists = await _context.Dailies.AnyAsync(x => x.daily_date.Date == daily.daily_date.Date);
                if (exists)
                {
                    return Json(new { success = false, message = "Bugün için zaten günlük girdin. Var olanı düzenle." });
                }

                _context.Dailies.Add(daily);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Günlük kaydedildi!" });
            }
            return Json(new { success = false, message = "Bir şeyler yazmalısın." });
        }

        // --- 3. GÜNCELLE ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Daily daily)
        {
            var item = await _context.Dailies.FindAsync(daily.daily_id);
            if (item != null)
            {
                item.daily_description = daily.daily_description;
                item.daily_status = daily.daily_status;

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Güncellendi." });
            }
            return Json(new { success = false, message = "Hata." });
        }

        // --- 4. SİL ---
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.Dailies.FindAsync(id);
            if (item != null)
            {
                _context.Dailies.Remove(item);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Silindi." });
            }
            return Json(new { success = false, message = "Hata." });
        }
    }
}