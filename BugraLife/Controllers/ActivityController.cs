using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BugraLife.DBContext;
using BugraLife.Models;
using Microsoft.AspNetCore.Authorization;

namespace BugraLife.Controllers
{
    [Authorize]
    public class ActivityController : Controller
    {
        private readonly BugraLifeDBContext _context;

        public ActivityController(BugraLifeDBContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Definitions = await _context.ActivityDefinitions
                .Where(x => x.is_active)
                .OrderBy(x => x.name)
                .ToListAsync();

            return View();
        }

        public async Task<IActionResult> GetCalendarEvents()
        {
            var logs = await _context.ActivityLogs
                .Include(x => x.ActivityDefinition)
                .ToListAsync();

            var events = logs.Select(x => new
            {
                id = x.activitylog_id,
                title = x.ActivityDefinition.name,
                start = x.log_date.ToString("yyyy-MM-dd"),
                backgroundColor = x.ActivityDefinition.color,
                borderColor = x.ActivityDefinition.color,
                allDay = true
            });

            return Json(events);
        }

        [HttpPost]
        public async Task<IActionResult> LogActivity(int defId, DateTime date)
        {
            var exists = await _context.ActivityLogs.AnyAsync(x =>
                x.activitydefinition_id == defId &&
                x.log_date.Date == date.Date);

            if (exists)
            {
                return Json(new { success = false, message = "Bu aktivite bugün zaten var!" });
            }

            var log = new ActivityLog
            {
                activitydefinition_id = defId,
                log_date = date
            };
            _context.Add(log);
            await _context.SaveChangesAsync();

            // Yeni oluşan Log ID'sini dönüyoruz
            return Json(new { success = true, id = log.activitylog_id });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteLog(int id)
        {
            var log = await _context.ActivityLogs.FindAsync(id);
            if (log != null)
            {
                _context.Remove(log);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        // --- TANIMLAMA İŞLEMLERİ ---

        [HttpPost]
        public async Task<IActionResult> CreateDefinition(ActivityDefinition model)
        {
            if (ModelState.IsValid)
            {
                _context.Add(model);
                await _context.SaveChangesAsync();
                // ÖNEMLİ: Yeni eklenen veriyi geri gönderiyoruz ki sayfayı yenilemeden ekleyebilelim
                return Json(new { success = true, item = model });
            }
            return Json(new { success = false });
        }

        [HttpPost]
        public async Task<IActionResult> EditDefinition(ActivityDefinition model)
        {
            var existing = await _context.ActivityDefinitions.FindAsync(model.activitydefinition_id);
            if (existing != null)
            {
                existing.name = model.name;
                existing.color = model.color;
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteDefinition(int id)
        {
            var item = await _context.ActivityDefinitions.FindAsync(id);
            if (item != null)
            {
                item.is_active = false;
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }
    }
}