using BugraLife.DBContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BugraLife.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly BugraLifeDBContext _context;

        public HomeController(BugraLifeDBContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
