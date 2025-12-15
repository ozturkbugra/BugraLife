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

        // Hesaplara Göre Hareketler Raporu
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

        // Gider Türüne Göre Hareketler Raporu
        public async Task<IActionResult> ExpenseTypeMovements(DateTime? startDate, DateTime? endDate, List<int> typeIds)
        {
            var model = new ExpenseTypeReportViewModel
            {
                Items = new List<ExpenseReportItem>(),
                // Varsayılan Tarihler: Ayın 1'i ve Şu an
                StartDate = startDate ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1),
                EndDate = endDate ?? DateTime.Now,
                SelectedTypeIds = typeIds ?? new List<int>()
            };

            // Dropdown için Gider Türlerini Çek
            ViewBag.ExpenseTypes = await _context.ExpenseTypes
                .OrderBy(x => x.expensetype_order) // Sıralı gelsin
                .ToListAsync();

            // Eğer hiç seçim yapılmadıysa boş dön
            if (typeIds == null || !typeIds.Any())
            {
                return View(model);
            }

            // GİDERLERİ FİLTRELE
            var expenses = await _context.Expenses
                .Include(x => x.ExpenseType) // Tür Adı için
                .Include(x => x.PaymentType) // Hesap Adı için
                .Where(x => typeIds.Contains(x.expensetype_id) &&
                            x.expense_date >= model.StartDate &&
                            x.expense_date <= model.EndDate)
                .OrderByDescending(x => x.expense_date) // Yeniden eskiye
                .Select(x => new ExpenseReportItem
                {
                    Id = x.expense_id,
                    Date = x.expense_date,
                    CategoryName = x.ExpenseType.expensetype_name,
                    AccountName = x.PaymentType.paymenttype_name,
                    Description = x.expense_description,
                    Amount = x.expense_amount
                }).ToListAsync();

            model.Items = expenses;
            model.TotalAmount = expenses.Sum(x => x.Amount); // Toplamı hesapla

            return View(model);
        }


        // Gelir Türüne Göre Hareketler Raporu
        public async Task<IActionResult> IncomeTypeMovements(DateTime? startDate, DateTime? endDate, List<int> typeIds)
        {
            var model = new IncomeTypeReportViewModel
            {
                Items = new List<IncomeReportItem>(),
                // Varsayılan: Ayın 1'i ve Şu an
                StartDate = startDate ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1),
                EndDate = endDate ?? DateTime.Now,
                SelectedTypeIds = typeIds ?? new List<int>()
            };

            // Dropdown için Gelir Türlerini Çek
            ViewBag.IncomeTypes = await _context.IncomeTypes
                .Where(x=> x.is_bank == false)
                .OrderBy(x => x.incometype_order)
                .ToListAsync();

            // Seçim yoksa boş dön
            if (typeIds == null || !typeIds.Any())
            {
                return View(model);
            }

            // GELİRLERİ FİLTRELE
            var incomes = await _context.Incomes
                .Include(x => x.IncomeType)  // Tür Adı
                .Include(x => x.PaymentType) // Hesap Adı
                .Where(x => typeIds.Contains(x.incometype_id) &&
                            x.income_date >= model.StartDate &&
                            x.income_date <= model.EndDate)
                .OrderByDescending(x => x.income_date)
                .Select(x => new IncomeReportItem
                {
                    Id = x.income_id,
                    Date = x.income_date,
                    CategoryName = x.IncomeType.incometype_name,
                    AccountName = x.PaymentType.paymenttype_name,
                    Description = x.income_description,
                    Amount = x.income_amount
                }).ToListAsync();

            model.Items = incomes;
            model.TotalAmount = incomes.Sum(x => x.Amount);

            return View(model);
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