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

            // Listeyi Sırala: Ödenmeyenler ve Gecikenler en üstte olsun
            var model = new DashboardViewModel
            {
                FixedExpenseStatuses = statusList
                    .OrderBy(x => x.IsPaid) // Önce Ödenmeyenler (False)
                    .ThenBy(x => x.DaysDiff) // Sonra günü geçenler (Negatifler)
                    .ToList()
            };

            return View(model);
        }
    }
}