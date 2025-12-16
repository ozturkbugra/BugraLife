using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BugraLife.DBContext;
using BugraLife.Models;

namespace BugraLife.Controllers
{
    public class HomeController : Controller
    {
        private readonly BugraLifeDBContext _context;

        public HomeController(BugraLifeDBContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // 1. Aktif Sabit Giderleri Çek
            var fixedExpenses = await _context.FixedExpenses
                .Include(x => x.ExpenseType)
                .Where(x => x.is_active)
                .ToListAsync();

            var statusList = new List<FixedExpenseStatus>();
            var today = DateTime.Today;

            // 2. Her Bir Sabit Gider İçin Durum Kontrolü
            foreach (var item in fixedExpenses)
            {
                // Bu ayın son ödeme tarihini oluştur (Şubat ayı 28/29 çektiği için gün kontrolü yapıyoruz)
                int daysInMonth = DateTime.DaysInMonth(today.Year, today.Month);
                int dueDay = item.payment_day > daysInMonth ? daysInMonth : item.payment_day;

                var dueDate = new DateTime(today.Year, today.Month, dueDay);

                // Bu ay bu gider türünde bir ödeme yapılmış mı?
                bool isPaid = await _context.Expenses.AnyAsync(x =>
                    x.expensetype_id == item.expensetype_id &&
                    x.expense_date.Month == today.Month &&
                    x.expense_date.Year == today.Year);

                // Gün Farkını Hesapla (Bugün - Son Ödeme Tarihi)
                // Sonuç Negatifse (-5): 5 Gün Gecikti
                // Sonuç Pozitifse (+5): 5 Gün Var (Ama biz Today - DueDate yaparsak mantık ters olur, o yüzden TimeSpan kullanıyoruz)

                TimeSpan diff = dueDate - today;
                int daysRemaining = (int)diff.TotalDays;
                // daysRemaining > 0 : Gün var
                // daysRemaining < 0 : Gün geçti

                statusList.Add(new FixedExpenseStatus
                {
                    ExpenseName = item.ExpenseType?.expensetype_name ?? "Bilinmiyor",
                    PaymentDay = item.payment_day,
                    IsPaid = isPaid,
                    DueDate = dueDate,
                    DaysDiff = daysRemaining
                });
            }

          

            var paymentTypes = await _context.PaymentTypes.Where(x=> x.is_bank == false).OrderBy(x => x.paymenttype_order).ToListAsync();

            // Bugüne kadar olan Gelirleri çekip grupla
            var incomes = await _context.Incomes
                .Where(x => x.income_date.Date <= today) // GELECEK DAHİL DEĞİL
                .GroupBy(x => x.paymenttype_id)
                .Select(g => new { Id = g.Key, Total = g.Sum(x => x.income_amount) })
                .ToListAsync();

            // Bugüne kadar olan Giderleri çekip grupla
            var expenses = await _context.Expenses
                .Where(x => x.expense_date.Date <= today) // GELECEK DAHİL DEĞİL
                .GroupBy(x => x.paymenttype_id)
                .Select(g => new { Id = g.Key, Total = g.Sum(x => x.expense_amount) })
                .ToListAsync();

            var accountStatuses = new List<AccountStatus>();

            foreach (var pt in paymentTypes)
            {
                // Hesaplama: (Toplam Gelir) - (Toplam Gider)
                // paymenttype_balance (başlangıç bakiyesi) sütununu kullanmıyoruz, sen istemiştin.

                decimal totalInc = incomes.FirstOrDefault(x => x.Id == pt.paymenttype_id)?.Total ?? 0;
                decimal totalExp = expenses.FirstOrDefault(x => x.Id == pt.paymenttype_id)?.Total ?? 0;
                decimal currentBalance = totalInc - totalExp;

                string typeName = "Kasa (Nakit)";
                if (pt.is_creditcard) typeName = "Kredi Kartı";
                else if (pt.is_bank) typeName = "Banka Hesabı";

                accountStatuses.Add(new AccountStatus
                {
                    AccountName = pt.paymenttype_name,
                    Balance = currentBalance,
                    Type = typeName,
                    IsCreditCard = pt.is_creditcard
                });
            }

            var toDos = await _context.PlannedToDos
            .Where(x => x.plannedtodo_done == false && x.plannedtodo_date.Date <= today)
            .OrderBy(x => x.plannedtodo_date) // En eski tarih en üstte (Aciliyet sırası)
            .ToListAsync();

            // ViewModel'i Doldur
            var model = new DashboardViewModel
            {
                FixedExpenseStatuses = statusList.OrderBy(x => x.IsPaid).ThenBy(x => x.DaysDiff).ToList(),
                Accounts = accountStatuses,
                PendingToDos = toDos // YENİ
            };

            return View(model);

            
        }
    }
}