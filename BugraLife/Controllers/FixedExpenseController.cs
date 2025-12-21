using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BugraLife.DBContext;
using BugraLife.Models;
using Microsoft.AspNetCore.Authorization;

namespace BugraLife.Controllers
{
    [Authorize]
    public class FixedExpenseController : Controller
    {
        private readonly BugraLifeDBContext _context;

        public FixedExpenseController(BugraLifeDBContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // 1. Listeyi Çek
            var list = await _context.FixedExpenses
                .Include(x => x.ExpenseType)
                .Where(x => x.is_active)
                .OrderBy(x => x.payment_day)
                .ToListAsync();

            // 2. Dropdown Verisi (Sadece Ev Giderleri)
            ViewBag.ExpenseTypes = await _context.ExpenseTypes
                .Where(x => x.is_home == true)
                .OrderBy(x => x.expensetype_name)
                .ToListAsync();

            return View(list);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FixedExpense fixedExpense)
        {
            if (ModelState.IsValid)
            {
                fixedExpense.is_active = true;
                _context.Add(fixedExpense);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Sabit gider başarıyla tanımlandı." });
            }
            return Json(new { success = false, message = "Lütfen tüm alanları doldurunuz." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(FixedExpense fixedExpense)
        {
            // ID kontrolü model binding ile gelir ama biz yine de var olanı çekelim
            var existing = await _context.FixedExpenses.FindAsync(fixedExpense.fixedexpense_id);
            if (existing == null)
            {
                return Json(new { success = false, message = "Kayıt bulunamadı." });
            }

            if (ModelState.IsValid)
            {
                // Sadece değişmesi gereken alanları güncelle
                existing.expensetype_id = fixedExpense.expensetype_id;
                existing.payment_day = fixedExpense.payment_day;
                existing.frequency_count = fixedExpense.frequency_count;

                _context.Update(existing);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Sabit gider güncellendi." });
            }
            return Json(new { success = false, message = "Form verileri geçersiz." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.FixedExpenses.FindAsync(id);
            if (item == null)
            {
                return Json(new { success = false, message = "Kayıt bulunamadı." });
            }

            // Silmek yerine pasife çekiyoruz (Soft Delete)
            item.is_active = false;
            _context.Update(item);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Sabit gider takibi iptal edildi." });
        }


    }
}