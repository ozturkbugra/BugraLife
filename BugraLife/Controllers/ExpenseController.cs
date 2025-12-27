using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BugraLife.Models;
using BugraLife.DBContext;
using Microsoft.AspNetCore.Authorization;
using System.Globalization;

namespace BugraLife.Controllers
{
    [Authorize]
    public class ExpenseController : Controller
    {
        private readonly BugraLifeDBContext _context;

        public ExpenseController(BugraLifeDBContext context)
        {
            _context = context;
        }

        // 1. LİSTELEME (Filtreli)
        public async Task<IActionResult> Index(bool showAll = false)
        {
            // Temel sorgu
            var query = _context.Expenses
                .Include(x => x.ExpenseType)
                .Include(x => x.PaymentType)
                .Include(x => x.Person)
                .Where(x => x.is_bankmovement == false);

            // Eğer "Tümünü Göster" denmediyse, sadece bu ayın verilerini getir
            if (!showAll)
            {
                var now = DateTime.Now;
                query = query.Where(x => x.expense_date.Month == now.Month && x.expense_date.Year == now.Year);
            }

            var list = await query.OrderByDescending(x => x.expense_date).ToListAsync();

            // View tarafında buton durumunu kontrol etmek için
            ViewBag.ShowAll = showAll;

            // Dropdown Verileri
            ViewBag.ExpenseTypes = await _context.ExpenseTypes
                .Where(x => x.is_bank == false)
                .OrderBy(x => x.expensetype_name)
                .ToListAsync();

            ViewBag.PaymentTypes = await _context.PaymentTypes
                .Where(x => x.is_bank == false).OrderBy(x => x.paymenttype_order).ToListAsync();

            ViewBag.Persons = await _context.Persons
                .Where(x => x.is_bank == false).OrderBy(x => x.person_order).ToListAsync();

            return View(list);
        }

        // 2. EKLEME (Sayfa yenilemeden satır eklemek için veriyi geri dönüyoruz)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Expense expense)
        {
            if (ModelState.IsValid)
            {
                // Bakiye Düş
                var account = await _context.PaymentTypes.FindAsync(expense.paymenttype_id);
                if (account != null) account.paymenttype_balance -= expense.expense_amount;

                // Kaydet
                expense.is_bankmovement = false;
                _context.Expenses.Add(expense);
                await _context.SaveChangesAsync();

                // EKLENEN VERİYİ DETAYLI ÇEK (Tabloya basmak için isimler lazım)
                var newExpense = await GetExpenseDetails(expense.expense_id);

                return Json(new { success = true, message = "Gider eklendi.", data = newExpense });
            }
            return Json(new { success = false, message = "Eksik bilgi." });
        }

        // 3. GÜNCELLEME (Sayfa yenilemeden satırı güncellemek için veriyi geri dönüyoruz)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Expense expense)
        {
            if (ModelState.IsValid)
            {
                var oldExpense = await _context.Expenses.AsNoTracking().FirstOrDefaultAsync(x => x.expense_id == expense.expense_id);

                if (oldExpense != null)
                {
                    // Eski tutarı iade et
                    var oldAccount = await _context.PaymentTypes.FindAsync(oldExpense.paymenttype_id);
                    if (oldAccount != null) oldAccount.paymenttype_balance += oldExpense.expense_amount;

                    // Yeni tutarı düş
                    var newAccount = await _context.PaymentTypes.FindAsync(expense.paymenttype_id);
                    if (newAccount != null) newAccount.paymenttype_balance -= expense.expense_amount;
                }

                // Güncelle
                expense.is_bankmovement = false;
                _context.Expenses.Update(expense);
                await _context.SaveChangesAsync();

                // GÜNCELLENEN VERİYİ DETAYLI ÇEK
                var updatedExpense = await GetExpenseDetails(expense.expense_id);

                return Json(new { success = true, message = "Gider güncellendi.", data = updatedExpense });
            }
            return Json(new { success = false, message = "Güncelleme başarısız." });
        }

        // 4. SİLME
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var expense = await _context.Expenses.FindAsync(id);
            if (expense != null)
            {
                var account = await _context.PaymentTypes.FindAsync(expense.paymenttype_id);
                if (account != null) account.paymenttype_balance += expense.expense_amount;

                _context.Expenses.Remove(expense);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Gider silindi." });
            }
            return Json(new { success = false, message = "Kayıt bulunamadı." });
        }

        // YARDIMCI METOD: ID'si verilen giderin tüm detaylarını JSON formatına uygun hazırlar
        private async Task<object> GetExpenseDetails(int id)
        {
            var item = await _context.Expenses
                .Include(x => x.Person)
                .Include(x => x.ExpenseType)
                .Include(x => x.PaymentType)
                .FirstOrDefaultAsync(x => x.expense_id == id);

            var trCulture = new CultureInfo("tr-TR");

            return new
            {
                id = item.expense_id,
                dateStr = item.expense_date.ToString("dd.MM.yyyy"),
                dateRaw = item.expense_date.ToString("yyyy-MM-dd"),
                person = item.Person != null ? item.Person.person_name : "-",
                personId = item.person_id,
                type = item.ExpenseType != null ? item.ExpenseType.expensetype_name : "-",
                typeId = item.expensetype_id,
                payment = item.PaymentType != null ? item.PaymentType.paymenttype_name : "-",
                paymentId = item.paymenttype_id,
                desc = item.expense_description,
                amountRaw = item.expense_amount.ToString("N2", trCulture) // 1.500,00 formatı
            };
        }
    }
}