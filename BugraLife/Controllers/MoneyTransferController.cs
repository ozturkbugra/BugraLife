using BugraLife.DBContext;
using BugraLife.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace BugraLife.Controllers
{
    [Authorize]
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
        public async Task<IActionResult> MakeTransfer(int sourceAccountId, int targetAccountId, string amount, string commission, DateTime date, string description)
        {
            // 1. FORMAT DÜZELTME (Virgül/Nokta Sorunu İçin)
            decimal decimalAmount = 0;
            decimal decimalCommission = 0;

            try
            {
                string cleanAmount = (amount ?? "0").Replace(",", ".");
                string cleanCommission = (commission ?? "0").Replace(",", ".");
                decimalAmount = decimal.Parse(cleanAmount, CultureInfo.InvariantCulture);
                decimalCommission = decimal.Parse(cleanCommission, CultureInfo.InvariantCulture);
            }
            catch
            {
                return Json(new { success = false, message = "Tutar formatı hatalı!" });
            }

            if (sourceAccountId == targetAccountId)
            {
                return Json(new { success = false, message = "Kaynak ve hedef hesap aynı olamaz." });
            }

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var sourceAccount = await _context.PaymentTypes.FindAsync(sourceAccountId);
                    var targetAccount = await _context.PaymentTypes.FindAsync(targetAccountId);

                    var personid = await _context.Persons.Where(x => x.is_bank == true).Select(x => x.person_id).FirstOrDefaultAsync();
                    var incometypeid = await _context.IncomeTypes.Where(x => x.is_bank == true).Select(x => x.incometype_id).FirstOrDefaultAsync();

                    var transferTypeId = await _context.ExpenseTypes
                        .Where(x => x.is_bank == true && x.is_commission == false)
                        .Select(x => x.expensetype_id)
                        .FirstOrDefaultAsync();

                    var commissionTypeId = await _context.ExpenseTypes
                        .Where(x => x.is_commission == true)
                        .Select(x => x.expensetype_id)
                        .FirstOrDefaultAsync();

                    if (commissionTypeId == 0) commissionTypeId = transferTypeId;

                    if (sourceAccount == null || targetAccount == null)
                    {
                        return Json(new { success = false, message = "Hesap bilgileri bulunamadı." });
                    }

                    // --- AÇIKLAMA OLUŞTURMA (DİNAMİK) ---
                    // Örnek: "Garanti Bankası » Ziraat Bankası Transferi"
                    string autoDescription = $"{sourceAccount.paymenttype_name} » {targetAccount.paymenttype_name} Transferi";

                    // Eğer kullanıcı açıklama yazdıysa onu kullan, yazmadıysa otomatiği kullan
                    string finalDescription = string.IsNullOrEmpty(description) ? autoDescription : description;

                    // Masraf açıklaması her zaman detaylı olsun
                    string commissionDesc = $"{sourceAccount.paymenttype_name} » {targetAccount.paymenttype_name} İşlem Masrafı";


                    // --- ADIM 1: ANA TRANSFER (GİDER) ---
                    var expense = new Expense
                    {
                        paymenttype_id = sourceAccountId,
                        expense_amount = decimalAmount,
                        expense_date = date,
                        expense_description = finalDescription, // "Garanti » Ziraat Transferi"
                        is_bankmovement = true,
                        expensetype_id = transferTypeId,
                        person_id = personid
                    };

                    sourceAccount.paymenttype_balance -= decimalAmount;
                    _context.Expenses.Add(expense);

                    // --- ADIM 2: HEDEF HESABA GİRİŞ (GELİR) ---
                    var income = new Income
                    {
                        paymenttype_id = targetAccountId,
                        income_amount = decimalAmount,
                        income_date = date,
                        income_description = finalDescription, // "Garanti » Ziraat Transferi"
                        is_bankmovement = true,
                        incometype_id = incometypeid,
                        person_id = personid
                    };

                    targetAccount.paymenttype_balance += decimalAmount;
                    _context.Incomes.Add(income);

                    // --- ADIM 3: KOMİSYON / MASRAF İŞLEMİ ---
                    if (decimalCommission > 0)
                    {
                        var commExpense = new Expense
                        {
                            paymenttype_id = sourceAccountId,
                            expense_amount = decimalCommission,
                            expense_date = date,
                            expense_description = commissionDesc, // "Garanti » Ziraat İşlem Masrafı"
                            is_bankmovement = false,
                            expensetype_id = commissionTypeId,
                            person_id = personid
                        };

                        sourceAccount.paymenttype_balance -= decimalCommission;
                        _context.Expenses.Add(commExpense);
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Json(new { success = true, message = "Transfer işlemi başarıyla tamamlandı." });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return Json(new { success = false, message = "Hata: " + ex.Message });
                }
            }
        }
    }
}