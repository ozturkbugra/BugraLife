using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BugraLife.Models;
using BugraLife.DBContext;
using Microsoft.AspNetCore.Authorization;

namespace BugraLife.Controllers
{
    [Authorize]
    public class MovementController : Controller
    {
        private readonly BugraLifeDBContext _context;

        public MovementController(BugraLifeDBContext context)
        {
            _context = context;
        }

        // 1. LİSTELEME
        public async Task<IActionResult> Index()
        {
            var list = await _context.Movements
                .Include(x => x.Debtor)
                .Include(x => x.Ingredient)
                .Include(x => x.Person)
                .OrderByDescending(x => x.movement_date) // Tarihe göre yeniden eskiye
                .ToListAsync();

            // --- DROPDOWN VERİLERİ ---

            // 1. Cariler (Debtors) - Alfabetik
            ViewBag.Debtors = await _context.Debtors
                .OrderBy(x => x.debtor_name)
                .ToListAsync();

            // 2. Varlık Türleri (Ingredients) - Alfabetik
            ViewBag.Ingredients = await _context.Ingredients
                .OrderBy(x => x.ingredient_name)
                .ToListAsync();

            // 3. Kişiler (Persons) - is_bank olmayanlar, sıraya göre
            ViewBag.Persons = await _context.Persons
                .Where(x => x.is_bank == false)
                .OrderBy(x => x.person_order)
                .ToListAsync();

            return View(list);
        }

        // 2. EKLEME (POST - AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Movement movement)
        {
            if (ModelState.IsValid)
            {
                _context.Movements.Add(movement);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Hareket başarıyla kaydedildi." });
            }
            return Json(new { success = false, message = "Form verileri eksik." });
        }

        // 3. GÜNCELLEME (POST - AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Movement movement)
        {
            if (ModelState.IsValid)
            {
                _context.Movements.Update(movement);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Hareket güncellendi." });
            }
            return Json(new { success = false, message = "Güncelleme başarısız." });
        }

        // 4. SİLME (POST - AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.Movements.FindAsync(id);
            if (item != null)
            {
                _context.Movements.Remove(item);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Kayıt silindi." });
            }
            return Json(new { success = false, message = "Kayıt bulunamadı." });
        }
    }
}