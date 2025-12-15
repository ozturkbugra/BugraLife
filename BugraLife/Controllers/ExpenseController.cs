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
            var list = await _context.Expenses
                .Include(x => x.ExpenseType)
                .Include(x => x.PaymentType)
                .Include(x => x.Person)
                .Where(x => x.is_bankmovement == false)
                .OrderByDescending(x => x.expense_date)
                .ToListAsync();

            // Dropdownlar (is_bank == false olanlar)
            ViewBag.ExpenseTypes = await _context.ExpenseTypes
                .Where(x => x.is_bank == false)
                .OrderBy(x => x.expensetype_name) // String sıralama güvenli olsun diye name kullandım, order string ise int.parse gerekebilir
                .ToListAsync();

            ViewBag.PaymentTypes = await _context.PaymentTypes
                .Where(x => x.is_bank == false).OrderBy(x => x.paymenttype_order).ToListAsync();

            ViewBag.Persons = await _context.Persons
                .Where(x => x.is_bank == false).OrderBy(x => x.person_order).ToListAsync();

            return View(list);
        }

        // 2. EKLEME (BAKİYE AZALT)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Expense expense)
        {
            if (ModelState.IsValid)
            {
                // 1. Hesap Bakiyesinden Düş (-)
                var account = await _context.PaymentTypes.FindAsync(expense.paymenttype_id);
                if (account != null)
                {
                    account.paymenttype_balance -= expense.expense_amount;
                }

                // 2. Gideri Kaydet
                expense.is_bankmovement = false;
                _context.Expenses.Add(expense);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Gider eklendi, bakiye düşüldü." });
            }
            return Json(new { success = false, message = "Eksik bilgi." });
        }

        // 3. GÜNCELLEME (ESKİYİ EKLE, YENİYİ ÇIKAR)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Expense expense)
        {
            if (ModelState.IsValid)
            {
                // 1. Eski kaydı bul
                var oldExpense = await _context.Expenses.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.expense_id == expense.expense_id);

                if (oldExpense != null)
                {
                    // A. Eski tutarı hesaba geri iade et (+)
                    var oldAccount = await _context.PaymentTypes.FindAsync(oldExpense.paymenttype_id);
                    if (oldAccount != null)
                    {
                        oldAccount.paymenttype_balance += oldExpense.expense_amount;
                    }

                    // B. Yeni tutarı yeni hesaptan düş (-)
                    var newAccount = await _context.PaymentTypes.FindAsync(expense.paymenttype_id);
                    if (newAccount != null)
                    {
                        newAccount.paymenttype_balance -= expense.expense_amount;
                    }
                }

                // 2. Güncelle
                expense.is_bankmovement = false;
                _context.Expenses.Update(expense);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Gider güncellendi." });
            }
            return Json(new { success = false, message = "Güncelleme başarısız." });
        }

        // 4. SİLME (BAKİYE ARTIR - İADE)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var expense = await _context.Expenses.FindAsync(id);
            if (expense != null)
            {
                // 1. Silinen gider tutarını hesaba geri ekle (+)
                var account = await _context.PaymentTypes.FindAsync(expense.paymenttype_id);
                if (account != null)
                {
                    account.paymenttype_balance += expense.expense_amount;
                }

                // 2. Sil
                _context.Expenses.Remove(expense);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Gider silindi, tutar iade edildi." });
            }
            return Json(new { success = false, message = "Kayıt bulunamadı." });
        }
    }
}