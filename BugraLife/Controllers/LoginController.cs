using BugraLife.DBContext;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

public class LoginController : Controller
{
    private readonly BugraLifeDBContext _context;

    public LoginController(BugraLifeDBContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Index()
    {
        // Eğer kullanıcı zaten giriş yapmışsa direkt ana sayfaya at
        if (User.Identity!.IsAuthenticated)
        {
            return RedirectToAction("Index", "Home");
        }
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Index(string username, string password, bool rememberMe)
    {
        string hashedPassword = Sifrele(password);

        var user = _context.LoginUser.FirstOrDefault(x =>
            x.loginuser_username == username &&
            x.login_password == hashedPassword);

        if (user != null)
        {
            // 1. Kullanıcının kimlik kartını (Claims) oluşturuyoruz
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.loginuser_username),
                new Claim(ClaimTypes.GivenName, user.loginuser_namesurname),
                new Claim("UserId", user.loginuser_id.ToString()) // Özel veri de saklayabilirsin
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = rememberMe // Beni hatırla seçildiyse tarayıcı kapansa da oturum kalır
            };

            // 2. Sisteme giriş yap (Çerezi oluştur)
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            return RedirectToAction("Index", "Home");
        }
        else
        {
            ViewBag.Error = "Kullanıcı adı veya şifre hatalı!";
            return View();
        }
    }

    // Çıkış Yapma Metodu
    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Login");
    }

    private string Sifrele(string sifre)
    {
        using (var sha256 = System.Security.Cryptography.SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(sifre));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}