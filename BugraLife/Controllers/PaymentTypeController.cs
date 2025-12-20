using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BugraLife.Models;
using BugraLife.DBContext;
using Microsoft.AspNetCore.Authorization;

namespace BugraLife.Controllers
{
    [Authorize]
    public class PaymentTypeController : Controller
    {
        private readonly BugraLifeDBContext _context;

        public PaymentTypeController(BugraLifeDBContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var list = await _context.PaymentTypes.OrderBy(x => x.paymenttype_order).ToListAsync();

            int nextOrder = 1;
            if (list.Any())
            {
                nextOrder = list.Max(x => x.paymenttype_order) + 1;
            }
            ViewBag.NextOrder = nextOrder;

            return View(list);
        }

        // 2. EKLEME (Balance input'u yok, otomatik 0 atanıyor)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PaymentType paymentType)
        {
            if (ModelState.IsValid)
            {
                // 1. İsim Kontrolü
                bool exists = await _context.PaymentTypes.AnyAsync(x => x.paymenttype_name == paymentType.paymenttype_name);
                if (exists) return Json(new { success = false, message = "Bu ödeme türü zaten kayıtlı!" });

                // ------------------------------------------------------------
                // ADIM A: HESABI OLUŞTUR
                // ------------------------------------------------------------
                _context.PaymentTypes.Add(paymentType);
                await _context.SaveChangesAsync(); // Kaydet ki ID oluşsun (paymentType.paymenttype_id)

                // ------------------------------------------------------------
                // ADIM B: AÇILIŞ BAKİYESİ VARSA HAREKET OLUŞTUR
                // ------------------------------------------------------------
                if (paymentType.paymenttype_balance != 0)
                {
                    // Varsayılan Kişi ve Tür ID'lerini bul (Hata almamak için)
                    // Sistemdeki ilk kaydı veya "Diğer" kategorisini alıyoruz.
                    var defaultPersonId = await _context.Persons.Where(x=> x.is_bank == true).Select(x => x.person_id).FirstOrDefaultAsync();
                    var defaultIncomeTypeId = await _context.IncomeTypes.Where(x => x.is_bank == true).Select(x => x.incometype_id).FirstOrDefaultAsync();
                    var defaultExpenseTypeId = await _context.ExpenseTypes.Where(x => x.is_bank == true).Select(x => x.expensetype_id).FirstOrDefaultAsync();

                    // Eğer veritabanı boşsa ve ID bulamazsa işlem yapma (Patlamasın)
                    if (defaultPersonId != 0)
                    {
                        // DURUM 1: BAKİYE POZİTİF (+) İSE -> GELİR EKLE
                        if (paymentType.paymenttype_balance > 0)
                        {
                            var income = new Income
                            {
                                paymenttype_id = paymentType.paymenttype_id,
                                income_amount = paymentType.paymenttype_balance,
                                income_date = DateTime.Now,
                                income_description = "Hesap Açılış Bakiyesi",
                                is_bankmovement = true, // Listede görünsün
                                person_id = defaultPersonId,
                                incometype_id = defaultIncomeTypeId != 0 ? defaultIncomeTypeId : 1
                            };
                            _context.Incomes.Add(income);
                        }
                        // DURUM 2: BAKİYE NEGATİF (-) İSE -> GİDER EKLE (Örn: Kredi Kartı Borcu)
                        else
                        {
                            var expense = new Expense
                            {
                                paymenttype_id = paymentType.paymenttype_id,
                                // Gider tablosuna tutar pozitif girilir (Math.Abs ile eksiği artı yap)
                                expense_amount = Math.Abs(paymentType.paymenttype_balance),
                                expense_date = DateTime.Now,
                                expense_description = "Hesap Açılış Bakiyesi (Borç/Devir)",
                                is_bankmovement = true, // Listede görünsün
                                person_id = defaultPersonId,
                                expensetype_id = defaultExpenseTypeId != 0 ? defaultExpenseTypeId : 1
                            };
                            _context.Expenses.Add(expense);
                        }

                        // Hareketi kaydet
                        await _context.SaveChangesAsync();
                    }
                }

                return Json(new { success = true, message = "Hesap ve açılış fişi oluşturuldu!" });
            }
            return Json(new { success = false, message = "Form verileri geçersiz." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PaymentType paymentType)
        {
            if (ModelState.IsValid)
            {
                // 1. İsim Çakışma Kontrolü
                bool exists = await _context.PaymentTypes.AnyAsync(x => x.paymenttype_name == paymentType.paymenttype_name && x.paymenttype_id != paymentType.paymenttype_id);
                if (exists)
                {
                    return Json(new { success = false, message = "Bu isimde başka bir kayıt zaten mevcut!" });
                }

                // 2. Mevcut Kaydı Veritabanından Çek (Değişiklikleri üzerine yazacağız)
                var existingRecord = await _context.PaymentTypes.FindAsync(paymentType.paymenttype_id);
                if (existingRecord == null) return Json(new { success = false, message = "Kayıt bulunamadı." });

                // --- BAKİYE DÜZELTME MANTIĞI BAŞLANGIÇ ---

                // A. Şu anki gerçek bakiyeyi hesapla (Gelirler - Giderler)
                var totalIncome = await _context.Incomes
                    .Where(x => x.paymenttype_id == paymentType.paymenttype_id)
                    .SumAsync(x => x.income_amount);

                var totalExpense = await _context.Expenses
                    .Where(x => x.paymenttype_id == paymentType.paymenttype_id)
                    .SumAsync(x => x.expense_amount);

                decimal currentRealBalance = totalIncome - totalExpense;

                // B. Kullanıcının girdiği hedef bakiye
                decimal targetBalance = paymentType.paymenttype_balance;

                // C. Aradaki Fark (Hedef - Mevcut)
                decimal difference = targetBalance - currentRealBalance;

                // D. Fark varsa Düzeltme Kaydı Oluştur
                if (difference != 0)
                {
                    // Varsayılan Kişi ve Kategori ID'lerini bul (Create metodundaki gibi)
                    var defaultPersonId = await _context.Persons.Where(x => x.is_bank == true).Select(x => x.person_id).FirstOrDefaultAsync();
                    var defaultIncomeTypeId = await _context.IncomeTypes.Where(x => x.is_bank == true).Select(x => x.incometype_id).FirstOrDefaultAsync();
                    var defaultExpenseTypeId = await _context.ExpenseTypes.Where(x => x.is_bank == true).Select(x => x.expensetype_id).FirstOrDefaultAsync();

                    if (defaultPersonId != 0)
                    {
                        if (difference > 0)
                        {
                            // Bakiye artmalı -> GELİR EKLE
                            var income = new Income
                            {
                                paymenttype_id = paymentType.paymenttype_id,
                                income_amount = difference, // Pozitif fark
                                income_date = DateTime.Now,
                                income_description = "Bakiye Düzeltme Fişi (Manuel)",
                                is_bankmovement = true,
                                person_id = defaultPersonId,
                                incometype_id = defaultIncomeTypeId != 0 ? defaultIncomeTypeId : 1
                            };
                            _context.Incomes.Add(income);
                        }
                        else
                        {
                            // Bakiye azalmalı -> GİDER EKLE
                            var expense = new Expense
                            {
                                paymenttype_id = paymentType.paymenttype_id,
                                expense_amount = Math.Abs(difference), // Gider pozitif girilir
                                expense_date = DateTime.Now,
                                expense_description = "Bakiye Düzeltme Fişi (Manuel)",
                                is_bankmovement = true,
                                person_id = defaultPersonId,
                                expensetype_id = defaultExpenseTypeId != 0 ? defaultExpenseTypeId : 1
                            };
                            _context.Expenses.Add(expense);
                        }
                    }
                }
                // --- BAKİYE DÜZELTME MANTIĞI BİTİŞ ---

                // 3. Diğer Alanları Güncelle
                existingRecord.paymenttype_name = paymentType.paymenttype_name;
                existingRecord.paymenttype_order = paymentType.paymenttype_order;
                existingRecord.is_creditcard = paymentType.is_creditcard;

                // paymenttype_balance sütununu da güncelleyelim ama asıl işi Income/Expense tabloları yapıyor
                existingRecord.paymenttype_balance = targetBalance;

                _context.PaymentTypes.Update(existingRecord);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Güncelleme ve bakiye eşitleme başarılı!" });
            }
            return Json(new { success = false, message = "Güncelleme başarısız." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.PaymentTypes.FindAsync(id);
            if (item != null)
            {
                _context.PaymentTypes.Remove(item);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Kayıt silindi." });
            }
            return Json(new { success = false, message = "Kayıt bulunamadı." });
        }
    }
}