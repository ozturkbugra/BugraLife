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

        // ---------------------------------------------------------
        // 1. HESAP HAREKETLERİ (GÜNCEL)
        // ---------------------------------------------------------
        public async Task<IActionResult> AccountMovements(DateTime? startDate, DateTime? endDate, List<int> accountIds, List<int> personIds)
        {
            var model = new AccountMovementViewModel
            {
                Movements = new List<MovementItem>(),
                StartDate = startDate ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1),
                EndDate = endDate ?? DateTime.Now,
                SelectedAccountIds = accountIds ?? new List<int>(),
                SelectedPersonIds = personIds ?? new List<int>()
            };

            ViewBag.Accounts = await _context.PaymentTypes.Where(x=> x.is_bank == false).OrderBy(x => x.paymenttype_order).ToListAsync();
            ViewBag.Persons = await _context.Persons.Where(x => x.is_bank == false).OrderBy(x => x.person_id).ToListAsync();

            // Eğer hiç seçim yoksa işlem yapma (Sayfa ilk açıldığında boş gelsin)
            // NOT: Kullanıcı "Tümü" seçerse listede -1 olacak, o yüzden count > 0 olacak.
            if ((accountIds == null || !accountIds.Any()) && (personIds == null || !personIds.Any()))
            {
                return View(model);
            }

            // --- FİLTRE MANTIĞI ---
            // Eğer listede -1 varsa "Tümü" seçilmiş demektir, filtreleme YAPMA (false).
            // Yoksa ve liste doluysa filtrele (true).
            bool filterByAccount = accountIds != null && accountIds.Any() && !accountIds.Contains(-1);
            bool filterByPerson = personIds != null && personIds.Any() && !personIds.Contains(-1);

            // 1. GELİRLER
            var incomesQuery = _context.Incomes
                .Include(x => x.PaymentType)
                .Include(x => x.Person)
                .Where(x => x.income_date >= model.StartDate && x.income_date <= model.EndDate);

            if (filterByAccount) incomesQuery = incomesQuery.Where(x => accountIds.Contains(x.paymenttype_id));
            if (filterByPerson) incomesQuery = incomesQuery.Where(x => personIds.Contains(x.person_id));

            var incomes = await incomesQuery.Select(x => new MovementItem
            {
                Id = x.income_id,
                Date = x.income_date,
                AccountName = x.PaymentType.paymenttype_name,
                Description = x.income_description,
                Amount = x.income_amount,
                Type = "Gelir",
                IsExpense = false
            }).ToListAsync();

            // 2. GİDERLER
            var expensesQuery = _context.Expenses
                .Include(x => x.PaymentType)
                .Include(x => x.Person)
                .Where(x => x.expense_date >= model.StartDate && x.expense_date <= model.EndDate);

            if (filterByAccount) expensesQuery = expensesQuery.Where(x => accountIds.Contains(x.paymenttype_id));
            if (filterByPerson) expensesQuery = expensesQuery.Where(x => personIds.Contains(x.person_id));

            var expenses = await expensesQuery.Select(x => new MovementItem
            {
                Id = x.expense_id,
                Date = x.expense_date,
                AccountName = x.PaymentType.paymenttype_name,
                Description = x.expense_description,
                Amount = x.expense_amount,
                Type = "Gider",
                IsExpense = true
            }).ToListAsync();

            model.Movements.AddRange(incomes);
            model.Movements.AddRange(expenses);
            model.Movements = model.Movements.OrderByDescending(x => x.Date).ToList();

            model.TotalIncome = incomes.Sum(x => x.Amount);
            model.TotalExpense = expenses.Sum(x => x.Amount);
            model.NetBalance = model.TotalIncome - model.TotalExpense;

            return View(model);
        }

        // ---------------------------------------------------------
        // 2. GİDER TÜRÜ HAREKETLERİ (GÜNCEL)
        // ---------------------------------------------------------
        public async Task<IActionResult> ExpenseTypeMovements(DateTime? startDate, DateTime? endDate, List<int> typeIds, List<int> personIds)
        {
            var model = new ExpenseTypeReportViewModel
            {
                Items = new List<ExpenseReportItem>(),
                StartDate = startDate ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1),
                EndDate = endDate ?? DateTime.Now,
                SelectedTypeIds = typeIds ?? new List<int>(),
                SelectedPersonIds = personIds ?? new List<int>()
            };

            ViewBag.ExpenseTypes = await _context.ExpenseTypes.Where(x => x.is_bank == false).OrderBy(x => x.expensetype_order).ToListAsync();
            ViewBag.Persons = await _context.Persons.Where(x => x.is_bank == false).OrderBy(x => x.person_id).ToListAsync();

            if ((typeIds == null || !typeIds.Any()) && (personIds == null || !personIds.Any())) return View(model);

            // FİLTRE MANTIĞI (-1 kontrolü)
            bool filterByType = typeIds != null && typeIds.Any() && !typeIds.Contains(-1);
            bool filterByPerson = personIds != null && personIds.Any() && !personIds.Contains(-1);

            var query = _context.Expenses
                .Include(x => x.ExpenseType)
                .Include(x => x.PaymentType)
                .Where(x => x.expense_date >= model.StartDate && x.expense_date <= model.EndDate);

            if (filterByType) query = query.Where(x => typeIds.Contains(x.expensetype_id));
            if (filterByPerson) query = query.Where(x => personIds.Contains(x.person_id));

            var expenses = await query.OrderByDescending(x => x.expense_date)
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
            model.TotalAmount = expenses.Sum(x => x.Amount);

            return View(model);
        }

        // ---------------------------------------------------------
        // 3. GELİR TÜRÜ HAREKETLERİ (GÜNCEL)
        // ---------------------------------------------------------
        public async Task<IActionResult> IncomeTypeMovements(DateTime? startDate, DateTime? endDate, List<int> typeIds, List<int> personIds)
        {
            var model = new IncomeTypeReportViewModel
            {
                Items = new List<IncomeReportItem>(),
                StartDate = startDate ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1),
                EndDate = endDate ?? DateTime.Now,
                SelectedTypeIds = typeIds ?? new List<int>(),
                SelectedPersonIds = personIds ?? new List<int>()
            };

            ViewBag.IncomeTypes = await _context.IncomeTypes.Where(x => x.is_bank == false).OrderBy(x => x.incometype_order).ToListAsync();
            ViewBag.Persons = await _context.Persons.Where(x => x.is_bank == false).OrderBy(x => x.person_id).ToListAsync();

            if ((typeIds == null || !typeIds.Any()) && (personIds == null || !personIds.Any())) return View(model);

            // FİLTRE MANTIĞI (-1 kontrolü)
            bool filterByType = typeIds != null && typeIds.Any() && !typeIds.Contains(-1);
            bool filterByPerson = personIds != null && personIds.Any() && !personIds.Contains(-1);

            var query = _context.Incomes
                .Include(x => x.IncomeType)
                .Include(x => x.PaymentType)
                .Where(x => x.income_date >= model.StartDate && x.income_date <= model.EndDate);

            if (filterByType) query = query.Where(x => typeIds.Contains(x.incometype_id));
            if (filterByPerson) query = query.Where(x => personIds.Contains(x.person_id));

            var incomes = await query.OrderByDescending(x => x.income_date)
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