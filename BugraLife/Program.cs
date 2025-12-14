using BugraLife.DBContext;
using BugraLife.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/Index"; // Giriþ yapmamýþ kiþi buraya atýlsýn
        options.ExpireTimeSpan = TimeSpan.FromDays(365); // Çerez 30 gün kalsýn
        options.SlidingExpiration = true; // Kullanýcý siteye girdikçe süre uzasýn
    });

builder.Services.AddDbContext<BugraLifeDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    static string Sifrele(string sifre)
    {
        using (var sha256 = System.Security.Cryptography.SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(sifre));

            return Convert.ToBase64String(hashedBytes);
        }
    }

    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<BugraLifeDBContext>();

        context.Database.EnsureCreated();

        if (!context.LoginUser.Any())
        {
            string rawPassword = "123456";
            string hashedPassword = Sifrele(rawPassword);

            var adminUser = new LoginUser
            {
                loginuser_username = "bugra",
                loginuser_namesurname = "Buðra Öztürk",
                login_password = hashedPassword
            };

            context.LoginUser.Add(adminUser);
            context.SaveChanges();

        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("Veri eklerken hata oluþtu: " + ex.Message);
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");






app.Run();

