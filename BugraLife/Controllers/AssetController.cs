using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BugraLife.Models;
using BugraLife.DBContext;
using Microsoft.AspNetCore.Authorization;

namespace BugraLife.Controllers
{
    [Authorize]
    public class AssetController : Controller
    {
        private readonly BugraLifeDBContext _context;

        public AssetController(BugraLifeDBContext context)
        {
            _context = context;
        }

        // 1. LİSTELEME VE DROPDOWN DOLDURMA
        public async Task<IActionResult> Index()
        {
            // Varlıkları listelerken ilişkili tabloları (Ingredient, Person) dahil ediyoruz (Include)
            var assets = await _context.Assets
                .Include(x => x.Ingredient)
                .Include(x => x.Person)
                .OrderByDescending(x => x.asset_date) // Tarihe göre yeniden eskiye
                .ToListAsync();

            // --- DROPDOWN VERİLERİ ---

            // 1. Ingredients: Alfabetik sıralı
            ViewBag.Ingredients = await _context.Ingredients
                .OrderBy(x => x.ingredient_name)
                .ToListAsync();

            // 2. Persons: Sadece is_bank == false olanlar + Order'a göre sıralı
            ViewBag.Persons = await _context.Persons
                .Where(x => x.is_bank == false)
                .OrderBy(x => x.person_order)
                .ToListAsync();

            return View(assets);
        }

        // 2. EKLEME (POST - AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Asset asset)
        {
            if (ModelState.IsValid)
            {
                _context.Assets.Add(asset);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Varlık başarıyla eklendi!" });
            }
            return Json(new { success = false, message = "Form verileri eksik veya hatalı." });
        }

        // 3. GÜNCELLEME (POST - AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Asset asset)
        {
            if (ModelState.IsValid)
            {
                _context.Assets.Update(asset);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Varlık güncellendi!" });
            }
            return Json(new { success = false, message = "Güncelleme başarısız." });
        }

        // 4. SİLME (POST - AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var asset = await _context.Assets.FindAsync(id);
            if (asset != null)
            {
                _context.Assets.Remove(asset);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Varlık silindi." });
            }
            return Json(new { success = false, message = "Kayıt bulunamadı." });
        }
    }
}