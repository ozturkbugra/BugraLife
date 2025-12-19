using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BugraLife.Models;
using BugraLife.DBContext;
using Microsoft.AspNetCore.Authorization; // Model namespace'ini kontrol et

namespace BugraLife.Controllers
{
    [Authorize]
    public class IngredientsController : Controller
    {

        private readonly BugraLifeDBContext _context;

        public IngredientsController(BugraLifeDBContext context)
        {
            _context = context;
        }

        // 1. LİSTELEME
        public async Task<IActionResult> Index()
        {
            var ingredients = await _context.Ingredients.ToListAsync();
            return View(ingredients);
        }

        // 2. EKLEME (POST - AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Ingredient ingredient)
        {
            if (ModelState.IsValid)
            {
                // --- KONTROL BAŞLANGICI ---
                // Veritabanında aynı isimde (büyük/küçük harf duyarsız olabilir DB ayarına göre) kayıt var mı?
                bool exists = await _context.Ingredients.AnyAsync(x => x.ingredient_name == ingredient.ingredient_name);

                if (exists)
                {
                    return Json(new { success = false, message = "Bu malzeme zaten kayıtlı! Lütfen farklı bir isim giriniz." });
                }
                // --- KONTROL BİTİŞİ ---

                _context.Ingredients.Add(ingredient);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Malzeme başarıyla eklendi!" });
            }
            return Json(new { success = false, message = "Form verileri geçersiz." });
        }

        // 3. GÜNCELLEME (POST - AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Ingredient ingredient)
        {
            if (ModelState.IsValid)
            {
                // --- KONTROL BAŞLANGICI ---
                // Kendisi HARİÇ (id != ingredient.ingredient_id) aynı ismi kullanan başka kayıt var mı?
                bool exists = await _context.Ingredients.AnyAsync(x => x.ingredient_name == ingredient.ingredient_name && x.ingredient_id != ingredient.ingredient_id);

                if (exists)
                {
                    return Json(new { success = false, message = "Bu isimde başka bir malzeme zaten var!" });
                }
                // --- KONTROL BİTİŞİ ---

                _context.Ingredients.Update(ingredient);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Malzeme güncellendi!" });
            }
            return Json(new { success = false, message = "Güncelleme başarısız!" });
        }

        // 4. SİLME (POST - AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var ingredient = await _context.Ingredients.FindAsync(id);
            if (ingredient != null)
            {
                _context.Ingredients.Remove(ingredient);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Malzeme silindi." });
            }
            return Json(new { success = false, message = "Kayıt bulunamadı." });
        }
    }
}