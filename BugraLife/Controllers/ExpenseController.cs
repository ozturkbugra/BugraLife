using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BugraLife.Models;
using BugraLife.DBContext;

namespace BugraLife.Controllers
{
    public class ExpenseController : Controller
    {
        private readonly BugraLifeDBContext _context;

        public ExpenseController(BugraLifeDBContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // 1. LİSTELEME
            var list = await _context.Expenses
                .Include(x => x.ExpenseType)
                .Include(x => x.PaymentType)
                .Include(x => x.Person)
                .Where(x => x.is_bankmovement == false) // Banka hareketleri hariç
                .OrderByDescending(x => x.expense_date) // Tarihe göre tersten
                .ToListAsync();

            // --- DROPDOWN VERİLERİ ---

            // 2. Gider Türleri: 
            // Şart: is_bank == false
            // Sıra: Order'a göre (String olma ihtimaline karşı int.TryParse ile güvenli sıralama)
            var expenseTypes = await _context.ExpenseTypes
                .Where(x => x.is_bank == false)
                .ToListAsync();

            ViewBag.ExpenseTypes = expenseTypes
                .OrderBy(x => int.TryParse(x.expensetype_order, out int v) ? v : 999)
                .ToList();

            // 3. Ödeme Türleri / Hesaplar:
            // Şart: is_bank == false (Sadece nakit cüzdanlar vs.)
            // Sıra: paymenttype_order
            ViewBag.PaymentTypes = await _context.PaymentTypes
                .Where(x => x.is_bank == false)
                .OrderBy(x => x.paymenttype_order)
                .ToListAsync();

            // 4. Kişiler:
            // Şart: is_bank == false
            ViewBag.Persons = await _context.Persons
                .Where(x => x.is_bank == false)
                .OrderBy(x => x.person_order)
                .ToListAsync();

            return View(list);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Expense expense)
        {
            if (ModelState.IsValid)
            {
                expense.is_bankmovement = false;
                _context.Expenses.Add(expense);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Gider başarıyla kaydedildi." });
            }
            return Json(new { success = false, message = "Form verileri eksik." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Expense expense)
        {
            if (ModelState.IsValid)
            {
                expense.is_bankmovement = false;
                _context.Expenses.Update(expense);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Gider güncellendi." });
            }
            return Json(new { success = false, message = "Güncelleme başarısız." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.Expenses.FindAsync(id);
            if (item != null)
            {
                _context.Expenses.Remove(item);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Kayıt silindi." });
            }
            return Json(new { success = false, message = "Kayıt bulunamadı." });
        }
    }
}