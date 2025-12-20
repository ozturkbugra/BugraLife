using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BugraLife.DBContext;
using BugraLife.Models;

namespace BugraLife.Controllers
{
    public class PracticalNoteController : Controller
    {
        private readonly BugraLifeDBContext _context;

        public PracticalNoteController(BugraLifeDBContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // En son eklenen en üstte olsun
            var notes = await _context.PracticalNotes
                                      .OrderByDescending(x => x.created_at)
                                      .ToListAsync();
            return View(notes);
        }

        [HttpPost]
        public async Task<IActionResult> Create(PracticalNote model)
        {
            if (ModelState.IsValid)
            {
                _context.Add(model);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Not başarıyla eklendi." });
            }
            return Json(new { success = false, message = "Eksik alanlar var." });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(PracticalNote model)
        {
            var note = await _context.PracticalNotes.FindAsync(model.practicalnote_id);
            if (note != null)
            {
                note.practicalnote_title = model.practicalnote_title;
                note.practicalnote_description = model.practicalnote_description;

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Not güncellendi." });
            }
            return Json(new { success = false, message = "Not bulunamadı." });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var note = await _context.PracticalNotes.FindAsync(id);
            if (note != null)
            {
                _context.Remove(note);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Not silindi." });
            }
            return Json(new { success = false, message = "Hata oluştu." });
        }
    }
}