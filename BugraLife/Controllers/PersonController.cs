using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BugraLife.Models;
using BugraLife.DBContext;
using Microsoft.AspNetCore.Authorization;

namespace BugraLife.Controllers
{
    [Authorize]
    public class PersonController : Controller
    {
        private readonly BugraLifeDBContext _context;

        public PersonController(BugraLifeDBContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var persons = await _context.Persons.OrderBy(x => x.person_order).ToListAsync();

            // Sıralama mantığı (Otomatik artış için)
            int nextOrder = 1;
            if (persons.Any())
            {
                nextOrder = persons.Max(x => x.person_order) + 1;
            }
            ViewBag.NextOrder = nextOrder;

            return View(persons);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Person person)
        {
            if (ModelState.IsValid)
            {
                bool exists = await _context.Persons.AnyAsync(x => x.person_name == person.person_name);
                if (exists)
                {
                    return Json(new { success = false, message = "Bu isimde kayıt zaten var!" });
                }

                _context.Persons.Add(person);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Kayıt başarıyla eklendi!" });
            }
            return Json(new { success = false, message = "Veriler geçersiz." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Person person)
        {
            // Checkbox işaretli değilse form veri göndermez, bu yüzden
            // Controller'a false olarak gelir. Bu ASP.NET'in varsayılan davranışıdır, sorun yok.

            if (ModelState.IsValid)
            {
                bool exists = await _context.Persons.AnyAsync(x => x.person_name == person.person_name && x.person_id != person.person_id);
                if (exists)
                {
                    return Json(new { success = false, message = "Bu isimde başka bir kayıt var!" });
                }

                _context.Persons.Update(person);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Güncelleme başarılı!" });
            }
            return Json(new { success = false, message = "Güncelleme başarısız." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var person = await _context.Persons.FindAsync(id);
            if (person != null)
            {
                _context.Persons.Remove(person);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Kayıt silindi." });
            }
            return Json(new { success = false, message = "Kayıt bulunamadı." });
        }
    }
}