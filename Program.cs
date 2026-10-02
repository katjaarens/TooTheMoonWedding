using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddRazorPages();

// Session-Dienst hinzufügen
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// --- PFAD FÜR SQLITE ANPASSEN ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=wedding_rsvp.db";

if (!connectionString.Contains("Data Source=", StringComparison.OrdinalIgnoreCase))
{
    connectionString = $"Data Source={connectionString}";
}

var dbPath = Path.Combine(Directory.GetCurrentDirectory(), "wedding_rsvp.db");
var finalConnectionString = $"Data Source={dbPath}";

// Datenbank-Kontext für SQLite mit dem absoluten Pfad registrieren
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(finalConnectionString));

    builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Guestbook/Index"; // Wo der Login stattfindet
    });

var app = builder.Build();

// Session-Middleware (nach Build, vor dem Routing)
app.UseSession();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

// Sorgt dafür, dass die Datenbank und Tabellen beim Start automatisch erstellt werden
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    context.Database.EnsureCreated();
}
app.Run();