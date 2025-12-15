using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BugraLife.Models;
using BugraLife.DBContext;

namespace BugraLife.Controllers
{
    public class MoneyTransferController : Controller
    {
        private readonly BugraLifeDBContext _context;

        public MoneyTransferController(BugraLifeDBContext context)
        {
            _context = context;
        }

        // SAYFA: Transfer Formu
        public async Task<IActionResult> Index()
        {
            // HEM KAYNAK HEM HEDEF OLABİLECEK HESAPLAR (Kredi Kartı OLMAYANLAR)
            var accounts = await _context.PaymentTypes
                .Where(x => x.is_creditcard == false && x.is_bank == false) // Sadece Nakit ve Banka
                .OrderBy(x => x.paymenttype_order)
                .ToListAsync();

            ViewBag.Accounts = accounts;

            return View();
        }

        // İŞLEM: Transferi Gerçekleştir
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MakeTransfer(int sourceAccountId, int targetAccountId, decimal amount, DateTime date, string description)
        {
            // Aynı hesaba transfer engeli
            if (sourceAccountId == targetAccountId)
            {
                return Json(new { success = false, message = "Kaynak ve hedef hesap aynı olamaz." });
            }

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    // Hesapları Bul
                    var sourceAccount = await _context.PaymentTypes.FindAsync(sourceAccountId); // Para Çıkan
                    var targetAccount = await _context.PaymentTypes.FindAsync(targetAccountId); // Para Giren

                    // Otomatik ID'leri çek (Banka/Sistem tanımlı kayıtlar)
                    var personid = await _context.Persons.Where(x => x.is_bank == true).Select(x => x.person_id).FirstOrDefaultAsync();
                    var expensetypeid = await _context.ExpenseTypes.Where(x => x.is_bank == true).Select(x => x.expensetype_id).FirstOrDefaultAsync();
                    var incometypeid = await _context.IncomeTypes.Where(x => x.is_bank == true).Select(x => x.incometype_id).FirstOrDefaultAsync();

                    if (sourceAccount == null || targetAccount == null)
                    {
                        return Json(new { success = false, message = "Hesap bilgileri bulunamadı." });
                    }

                    // Bakiye Kontrolü (İsteğe bağlı - Eksiye düşmesine izin veriyorsan kaldırabilirsin)
                    // if (sourceAccount.paymenttype_balance < amount) 
                    //    return Json(new { success = false, message = "Kaynak hesapta yeterli bakiye yok." });

                    // ----------------------------------------------------
                    // ADIM 1: KAYNAK HESAPTAN ÇIKIŞ (GİDER)
                    // ----------------------------------------------------
                    var expense = new Expense
                    {
                        paymenttype_id = sourceAccountId,
                        expense_amount = amount,
                        expense_date = date,
                        expense_description = string.IsNullOrEmpty(description) ? $"{targetAccount.paymenttype_name} Hesabına Transfer" : description,
                        is_bankmovement = true,
                        expensetype_id = expensetypeid,
                        person_id = personid
                    };

                    sourceAccount.paymenttype_balance -= amount; // Bakiyeyi düş
                    _context.Expenses.Add(expense);

                    // ----------------------------------------------------
                    // ADIM 2: HEDEF HESABA GİRİŞ (GELİR)
                    // ----------------------------------------------------
                    var income = new Income
                    {
                        paymenttype_id = targetAccountId,
                        income_amount = amount,
                        income_date = date,
                        income_description = string.IsNullOrEmpty(description) ? $"{sourceAccount.paymenttype_name} Hesabından Gelen Transfer" : description,
                        is_bankmovement = true,
                        incometype_id = incometypeid,
                        person_id = personid
                    };

                    targetAccount.paymenttype_balance += amount; // Bakiyeyi artır
                    _context.Incomes.Add(income);

                    // ----------------------------------------------------
                    // KAYDET
                    // ----------------------------------------------------
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Json(new { success = true, message = "Para transferi başarıyla tamamlandı." });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return Json(new { success = false, message = "Hata oluştu: " + ex.Message });
                }
            }
        }
    }
}