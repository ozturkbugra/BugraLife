using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BugraLife.Models;
using BugraLife.DBContext;

namespace BugraLife.Controllers
{
    public class CreditCardPaymentController : Controller
    {
        private readonly BugraLifeDBContext _context;

        public CreditCardPaymentController(BugraLifeDBContext context)
        {
            _context = context;
        }

        // SAYFA: Ödeme Formu
        public async Task<IActionResult> Index()
        {
            // 1. ÖDENECEK KARTLAR (Sadece Kredi Kartları)
            ViewBag.CreditCards = await _context.PaymentTypes
                .Where(x => x.is_creditcard == true)
                .OrderBy(x => x.paymenttype_order)
                .ToListAsync();

            // 2. KAYNAK HESAPLAR (Kredi Kartı Olmayanlar: Nakit, Banka Hesabı vb.)
            ViewBag.SourceAccounts = await _context.PaymentTypes
                .Where(x => x.is_creditcard == false && x.is_bank == false)
                .OrderBy(x => x.paymenttype_order)
                .ToListAsync();

            return View();
        }

        // İŞLEM: Ödemeyi Gerçekleştir
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MakePayment(int targetCardId, int sourceAccountId, decimal amount, DateTime date, string description)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    // Hesapları Bul
                    var targetCard = await _context.PaymentTypes.FindAsync(targetCardId); // Borcu ödenecek kart
                    var sourceAccount = await _context.PaymentTypes.FindAsync(sourceAccountId); // Para çıkacak hesap
                    var personid = await _context.Persons.Where(x => x.is_bank == true).Select(x=> x.person_id).FirstOrDefaultAsync();
                    var expensetypeid = await _context.ExpenseTypes.Where(x => x.is_bank == true).Select(x=> x.expensetype_id).FirstOrDefaultAsync();
                    var incometypeid = await _context.IncomeTypes.Where(x => x.is_bank == true).Select(x=> x.incometype_id).FirstOrDefaultAsync();

                    if (targetCard == null || sourceAccount == null)
                    {
                        return Json(new { success = false, message = "Hesap bilgileri hatalı." });
                    }

                    // ----------------------------------------------------
                    // ADIM 1: KAYNAK HESAPTAN PARA ÇIKIŞI (GİDER)
                    // ----------------------------------------------------
                    var expense = new Expense
                    {
                        paymenttype_id = sourceAccountId,
                        expense_amount = amount,
                        expense_date = date,
                        expense_description = string.IsNullOrEmpty(description) ? $"{targetCard.paymenttype_name} Borç Ödemesi" : description,
                        is_bankmovement = true, // Banka hareketi
                        expensetype_id = expensetypeid, // NOT: Veritabanında "Borç Ödeme" veya "Diğer" diye bir ID varsa onu yaz. Şimdilik 1 dedim.
                        person_id = personid // Varsayılan kişi
                    };

                    sourceAccount.paymenttype_balance -= amount; // Bankadan parayı düş
                    _context.Expenses.Add(expense);

                    // ----------------------------------------------------
                    // ADIM 2: KREDİ KARTINA PARA GİRİŞİ (GELİR / BORÇ DÜŞME)
                    // ----------------------------------------------------
                    var income = new Income
                    {
                        paymenttype_id = targetCardId,
                        income_amount = amount,
                        income_date = date,
                        income_description = string.IsNullOrEmpty(description) ? $"{sourceAccount.paymenttype_name} Hesabından Ödeme" : description,
                        is_bankmovement = true, // Banka hareketi
                        incometype_id = incometypeid, // NOT: Veritabanında "Borç Kapama" diye bir ID varsa onu yaz.
                        person_id = personid
                    };

                    targetCard.paymenttype_balance += amount; // Kart bakiyesini artır (Borcu kapat)
                    _context.Incomes.Add(income);

                    // ----------------------------------------------------
                    // KAYDET
                    // ----------------------------------------------------
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync(); // Onayla

                    return Json(new { success = true, message = "Kredi kartı borcu başarıyla ödendi." });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(); // Hata varsa geri al
                    return Json(new { success = false, message = "Hata oluştu: " + ex.Message });
                }
            }
        }
    }
}