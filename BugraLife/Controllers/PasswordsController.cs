using BugraLife.DBContext;
using BugraLife.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace BugraLife.Controllers
{
    [Authorize]
    public class PasswordsController : Controller
    {
        private readonly BugraLifeDBContext _context;

        public PasswordsController(BugraLifeDBContext context)
        {
            _context = context;
        }

        // 1. LİSTELEME
        public async Task<IActionResult> Index()
        {
            var passwords = await _context.WebSitePasswords
                                          .Include(w => w.WebSite)
                                          .ToListAsync();

            ViewBag.WebSites = await _context.WebSites.ToListAsync();

            return View(passwords);
        }

        // 2. EKLEME (POST - AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WebSitePassword password)
        {
            if (ModelState.IsValid)
            {
                password.created_at = DateTime.Now;
                _context.WebSitePasswords.Add(password);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Şifre başarıyla eklendi!" });
            }
            return Json(new { success = false, message = "Lütfen tüm alanları doldurun." });
        }

        // 3. GÜNCELLEME (POST - AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(WebSitePassword password)
        {
            if (ModelState.IsValid)
            {
                password.updated_at = DateTime.Now;

                _context.Update(password);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Şifre güncellendi!" });
            }
            return Json(new { success = false, message = "Güncelleme başarısız!" });
        }

        // 4. SİLME (POST - AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var password = await _context.WebSitePasswords.FindAsync(id);
            if (password != null)
            {
                _context.WebSitePasswords.Remove(password);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Şifre silindi." });
            }
            return Json(new { success = false, message = "Kayıt bulunamadı." });
        }
    }

}