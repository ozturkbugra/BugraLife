using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BugraLife.Models;
using BugraLife.DBContext;
using Microsoft.AspNetCore.Authorization;

namespace BugraLife.Controllers
{
    [Authorize]
    public class DebtorController : Controller
    {
        private readonly BugraLifeDBContext _context;

        public DebtorController(BugraLifeDBContext context)
        {
            _context = context;
        }

        // 1. LİSTELEME
        public async Task<IActionResult> Index()
        {
            var debtors = await _context.Debtors.OrderBy(x => x.debtor_name).ToListAsync();
            return View(debtors);
        }

        // 2. EKLEME (POST - AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Debtor debtor)
        {
            if (ModelState.IsValid)
            {
                // Kontrol: Aynı isimde borçlu var mı?
                bool exists = await _context.Debtors.AnyAsync(x => x.debtor_name == debtor.debtor_name);
                if (exists)
                {
                    return Json(new { success = false, message = "Bu isimde bir borçlu zaten kayıtlı!" });
                }

                _context.Debtors.Add(debtor);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Borçlu/Alacaklı başarıyla eklendi!" });
            }
            return Json(new { success = false, message = "Form verileri geçersiz." });
        }

        // 3. GÜNCELLEME (POST - AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Debtor debtor)
        {
            if (ModelState.IsValid)
            {
                // Kontrol: Kendisi hariç aynı isimde kayıt var mı?
                bool exists = await _context.Debtors.AnyAsync(x => x.debtor_name == debtor.debtor_name && x.debtor_id != debtor.debtor_id);
                if (exists)
                {
                    return Json(new { success = false, message = "Bu isimde başka bir kayıt zaten mevcut!" });
                }

                _context.Debtors.Update(debtor);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Borçlu/Alacaklı bilgisi güncellendi!" });
            }
            return Json(new { success = false, message = "Güncelleme başarısız." });
        }

        // 4. SİLME (POST - AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var debtor = await _context.Debtors.FindAsync(id);
            if (debtor != null)
            {
                _context.Debtors.Remove(debtor);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Kayıt silindi." });
            }
            return Json(new { success = false, message = "Kayıt bulunamadı." });
        }
    }
}