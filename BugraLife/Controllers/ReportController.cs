using Microsoft.AspNetCore.Mvc;
using BugraLife.DBContext;
using BugraLife.Models;
using Microsoft.EntityFrameworkCore;

namespace BugraLife.Controllers
{
    public class ReportController : Controller
    {
        private readonly BugraLifeDBContext _context;

        public ReportController(BugraLifeDBContext context)
        {
            _context = context;
        }

        // 1. ANA SAYFA (Butonların olduğu yer)
        public IActionResult Index()
        {
            return View();
        }

        // --- RAPOR SAYFALARI (Şimdilik boş, tıklayınca açılsın diye) ---

        // ---------------------------------------------------------
        // HESAP HAREKETLERİ RAPORU
        // ---------------------------------------------------------
        public async Task<IActionResult> AccountMovements(DateTime? startDate, DateTime? endDate, List<int> accountIds)
        {
            var model = new AccountMovementViewModel
            {
                Movements = new List<MovementItem>(),
                // Varsayılan Tarihler (Ayın başı ve sonu)
                StartDate = startDate ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1),
                EndDate = endDate ?? DateTime.Now,
                SelectedAccountIds = accountIds ?? new List<int>()
            };

            // Dropdown için hesapları doldur
            ViewBag.Accounts = await _context.PaymentTypes.Where(x=> x.is_bank == false).OrderBy(x => x.paymenttype_order).ToListAsync();

            // Eğer hiç hesap seçilmediyse boş dön (veya hepsini getir, tercih senin. Ben boş dönüyorum)
            if (accountIds == null || !accountIds.Any())
            {
                return View(model);
            }

            // 1. GELİRLERİ ÇEK (Income)
            var incomes = await _context.Incomes
                .Include(x => x.PaymentType)
                .Where(x => accountIds.Contains(x.paymenttype_id) &&
                            x.income_date >= model.StartDate &&
                            x.income_date <= model.EndDate)
                .Select(x => new MovementItem
                {
                    Id = x.income_id,
                    Date = x.income_date,
                    AccountName = x.PaymentType.paymenttype_name,
                    Description = x.income_description,
                    Amount = x.income_amount,
                    Type = "Gelir",
                    IsExpense = false
                }).ToListAsync();

            // 2. GİDERLERİ ÇEK (Expense)
            var expenses = await _context.Expenses
                .Include(x => x.PaymentType)
                .Where(x => accountIds.Contains(x.paymenttype_id) &&
                            x.expense_date >= model.StartDate &&
                            x.expense_date <= model.EndDate)
                .Select(x => new MovementItem
                {
                    Id = x.expense_id,
                    Date = x.expense_date,
                    AccountName = x.PaymentType.paymenttype_name,
                    Description = x.expense_description,
                    Amount = x.expense_amount,
                    Type = "Gider",
                    IsExpense = true
                }).ToListAsync();

            // 3. LİSTELERİ BİRLEŞTİR VE SIRALA
            model.Movements.AddRange(incomes);
            model.Movements.AddRange(expenses);

            // Tarihe göre sırala (Yeniden eskiye)
            model.Movements = model.Movements.OrderByDescending(x => x.Date).ToList();

            // 4. TOPLAMLARI HESAPLA
            model.TotalIncome = incomes.Sum(x => x.Amount);
            model.TotalExpense = expenses.Sum(x => x.Amount);
            model.NetBalance = model.TotalIncome - model.TotalExpense;

            return View(model);
        }

        public IActionResult ExpenseTypeMovements() // Gider Türü Hareketleri
        {
            return View();
        }

        public IActionResult IncomeTypeMovements() // Gelir Türü Hareketleri
        {
            return View();
        }

        public IActionResult DebtReceivableReport() // Borç Alacak Raporu
        {
            return View();
        }

        public IActionResult PortfolioReport() // Portföy Raporu
        {
            return View();
        }

        public IActionResult AccountBalances() // Hesap Bakiyeleri
        {
            return View();
        }

        public IActionResult IncomeExpenseReport() // Gelir Gider Raporu
        {
            return View();
        }
    }
}