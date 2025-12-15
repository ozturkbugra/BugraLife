using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BugraLife.Models;
using BugraLife.DBContext;

namespace BugraLife.Controllers
{
    public class IncomeController : Controller
    {
        private readonly BugraLifeDBContext _context;

        public IncomeController(BugraLifeDBContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var list = await _context.Incomes
                .Include(x => x.IncomeType)
                .Include(x => x.PaymentType)
                .Include(x => x.Person)
                .Where(x => x.is_bankmovement == false)
                .OrderByDescending(x => x.income_date)
                .ToListAsync();

            ViewBag.IncomeTypes = await _context.IncomeTypes
                .Where(x => x.is_bank == false).OrderBy(x => x.incometype_order).ToListAsync();

            ViewBag.PaymentTypes = await _context.PaymentTypes
                .Where(x => x.is_bank == false).OrderBy(x => x.paymenttype_order).ToListAsync();

            ViewBag.Persons = await _context.Persons
                .Where(x => x.is_bank == false).OrderBy(x => x.person_order).ToListAsync();

            return View(list);
        }

        // 2. EKLEME (BAKİYE ARTIR)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Income income)
        {
            if (ModelState.IsValid)
            {
                // 1. Hesap Bakiyesini Güncelle (+ Ekle)
                var account = await _context.PaymentTypes.FindAsync(income.paymenttype_id);
                if (account != null)
                {
                    account.paymenttype_balance += income.income_amount;
                }

                // 2. Geliri Kaydet
                income.is_bankmovement = false;
                _context.Incomes.Add(income);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Gelir eklendi ve bakiye güncellendi." });
            }
            return Json(new { success = false, message = "Eksik bilgi." });
        }

        // 3. GÜNCELLEME (ESKİYİ ÇIKAR, YENİYİ EKLE)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Income income)
        {
            if (ModelState.IsValid)
            {
                // 1. Eski kaydı veritabanından çek (Değişiklik öncesi hali)
                // AsNoTracking kullanıyoruz ki EF Core çakışma yaşamasın
                var oldIncome = await _context.Incomes.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.income_id == income.income_id);

                if (oldIncome != null)
                {
                    // A. Eski tutarı, eski hesaptan düş
                    var oldAccount = await _context.PaymentTypes.FindAsync(oldIncome.paymenttype_id);
                    if (oldAccount != null)
                    {
                        oldAccount.paymenttype_balance -= oldIncome.income_amount;
                    }

                    // B. Yeni tutarı, yeni hesaba ekle
                    // (Hesap değişmediyse aynı hesaba işlem yapar, sorun yok)
                    var newAccount = await _context.PaymentTypes.FindAsync(income.paymenttype_id);
                    if (newAccount != null)
                    {
                        newAccount.paymenttype_balance += income.income_amount;
                    }
                }

                // 2. Kaydı Güncelle
                income.is_bankmovement = false;
                _context.Incomes.Update(income);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Gelir ve bakiyeler güncellendi." });
            }
            return Json(new { success = false, message = "Güncelleme başarısız." });
        }

        // 4. SİLME (BAKİYE AZALT)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var income = await _context.Incomes.FindAsync(id);
            if (income != null)
            {
                // 1. Silinen geliri hesaptan düş (-)
                var account = await _context.PaymentTypes.FindAsync(income.paymenttype_id);
                if (account != null)
                {
                    account.paymenttype_balance -= income.income_amount;
                }

                // 2. Kaydı Sil
                _context.Incomes.Remove(income);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Kayıt silindi, bakiye düşüldü." });
            }
            return Json(new { success = false, message = "Kayıt bulunamadı." });
        }
    }
}