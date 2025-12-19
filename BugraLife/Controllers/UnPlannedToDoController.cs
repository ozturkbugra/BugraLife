using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BugraLife.Models;
using BugraLife.DBContext;
using Microsoft.AspNetCore.Authorization;

namespace BugraLife.Controllers
{
    [Authorize]
    public class UnPlannedToDoController : Controller
    {
        private readonly BugraLifeDBContext _context;

        public UnPlannedToDoController(BugraLifeDBContext context)
        {
            _context = context;
        }

        // 1. LİSTELEME
        public async Task<IActionResult> Index()
        {
            // Önce YAPILMAYANLAR (false), sonra Tarihe göre YENİDEN ESKİYE
            var list = await _context.UnPlannedToDos
                .OrderBy(x => x.unplannedtodo_done) // False (0) önce gelir
                .ThenByDescending(x => x.unplannedtodo_createdat)
                .ToListAsync();

            return View(list);
        }

        // 2. EKLEME (POST - AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UnPlannedToDo todo)
        {
            if (!string.IsNullOrEmpty(todo.unplannedtodo_description))
            {
                // Otomatik değerler
                todo.unplannedtodo_createdat = DateTime.Now;
                todo.unplannedtodo_done = false;

                _context.UnPlannedToDos.Add(todo);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Not alındı!" });
            }
            return Json(new { success = false, message = "Açıklama boş olamaz." });
        }

        // 3. GÜNCELLEME (Sadece Açıklama)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UnPlannedToDo todo)
        {
            var item = await _context.UnPlannedToDos.FindAsync(todo.unplannedtodo_id);
            if (item != null)
            {
                item.unplannedtodo_description = todo.unplannedtodo_description;
                // Tarihi ve durumu değiştirmiyoruz, onlar ayrı yönetiliyor

                _context.UnPlannedToDos.Update(item);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Not güncellendi." });
            }
            return Json(new { success = false, message = "Kayıt bulunamadı." });
        }

        // 4. DURUM DEĞİŞTİR (Tik atma işlemi)
        [HttpPost]
        public async Task<IActionResult> ChangeStatus(int id)
        {
            var item = await _context.UnPlannedToDos.FindAsync(id);
            if (item != null)
            {
                // True ise False, False ise True yap (Tersi)
                item.unplannedtodo_done = !item.unplannedtodo_done;

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Durum güncellendi." });
            }
            return Json(new { success = false, message = "Hata oluştu." });
        }

        // 5. SİLME
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.UnPlannedToDos.FindAsync(id);
            if (item != null)
            {
                _context.UnPlannedToDos.Remove(item);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Not silindi." });
            }
            return Json(new { success = false, message = "Kayıt bulunamadı." });
        }
    }
}