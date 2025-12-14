using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BugraLife.Models;
using BugraLife.DBContext;

namespace BugraLife.Controllers
{
    public class IncomeController : Controller
    {
        private readonly BugraLifeDBContext _context;

        public IncomeController(BugraLifeDBContext context)
        {
            _context = context;
        }

        // 1. LİSTELEME
        public async Task<IActionResult> Index()
        {
            var list = await _context.Incomes
                .Include(x => x.IncomeType)
                .Include(x => x.PaymentType)
                .Include(x => x.Person)
                .Where(x => x.is_bankmovement == false) // Sadece normal gelirler
                .OrderByDescending(x => x.income_date) // Tarihe göre tersten
                .ToListAsync();

            // --- DROPDOWN VERİLERİ (Filtreli ve Sıralı) ---

            // 1. Gelir Türleri (is_bank == false)
            ViewBag.IncomeTypes = await _context.IncomeTypes
                .Where(x => x.is_bank == false)
                .OrderBy(x => x.incometype_order)
                .ToListAsync();

            // 2. Ödeme Türleri / Hesaplar (is_bank == false)
            ViewBag.PaymentTypes = await _context.PaymentTypes
                .Where(x => x.is_bank == false)
                .OrderBy(x => x.paymenttype_order)
                .ToListAsync();

            // 3. Kişiler (is_bank == false)
            ViewBag.Persons = await _context.Persons
                .Where(x => x.is_bank == false)
                .OrderBy(x => x.person_order)
                .ToListAsync();

            return View(list);
        }

        // 2. EKLEME (POST - AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Income income)
        {
            if (ModelState.IsValid)
            {
                income.is_bankmovement = false; // Otomatik false
                _context.Incomes.Add(income);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Gelir başarıyla kaydedildi." });
            }
            return Json(new { success = false, message = "Form verileri eksik." });
        }

        // 3. GÜNCELLEME (POST - AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Income income)
        {
            if (ModelState.IsValid)
            {
                income.is_bankmovement = false;
                _context.Incomes.Update(income);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Gelir güncellendi." });
            }
            return Json(new { success = false, message = "Güncelleme başarısız." });
        }

        // 4. SİLME (POST - AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.Incomes.FindAsync(id);
            if (item != null)
            {
                _context.Incomes.Remove(item);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Kayıt silindi." });
            }
            return Json(new { success = false, message = "Kayıt bulunamadı." });
        }
    }
}