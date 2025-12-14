using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BugraLife.Models;
using BugraLife.DBContext;

namespace BugraLife.Controllers
{
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
                bool exists = await _context.PaymentTypes.AnyAsync(x => x.paymenttype_name == paymentType.paymenttype_name);
                if (exists)
                {
                    return Json(new { success = false, message = "Bu ödeme türü zaten kayıtlı!" });
                }

                // --- DÜZELTME BURADA ---
                // Kullanıcıdan bakiye almıyoruz, varsayılan 0 yapıyoruz.
                paymentType.paymenttype_balance = 0;

                _context.PaymentTypes.Add(paymentType);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Hesap başarıyla oluşturuldu!" });
            }
            return Json(new { success = false, message = "Form verileri geçersiz." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PaymentType paymentType)
        {
            if (ModelState.IsValid)
            {
                bool exists = await _context.PaymentTypes.AnyAsync(x => x.paymenttype_name == paymentType.paymenttype_name && x.paymenttype_id != paymentType.paymenttype_id);
                if (exists)
                {
                    return Json(new { success = false, message = "Bu isimde başka bir kayıt zaten mevcut!" });
                }

                _context.PaymentTypes.Update(paymentType);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Güncelleme başarılı!" });
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