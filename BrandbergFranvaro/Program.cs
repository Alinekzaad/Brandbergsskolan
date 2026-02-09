using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BrandbergFranvaro.Data;
using BrandbergFranvaro.Models;
using BrandbergFranvaro.Services;

var builder = WebApplication.CreateBuilder(args);

// Lägg till tjänster
builder.Services.AddRazorPages();

// Konfigurera Entity Framework - SQLite som standard för lokal utveckling
var useSqlite = builder.Configuration.GetValue<bool>("UseSqlite", true);
if (useSqlite)
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite("Data Source=brandberg.db"));
}
else
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
}

// Konfigurera Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Lösenordskrav
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;

    // Lockout-inställningar
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // Användarinställningar
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false; // För enklare testning
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Konfigurera cookies
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(24);
    options.SlidingExpiration = true;
});

// Lägg till filuppladdningstjänst
builder.Services.AddScoped<IFileService, FileService>();

// Lägg till antiforgery
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
});

var app = builder.Build();

// Konfigurera HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

// Initiera databasen och seed data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var configuration = services.GetRequiredService<IConfiguration>();

    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        
        if (configuration.GetValue<bool>("UseSqlite", true))
        {
            // För SQLite: Skapa schema från modellen
            await context.Database.EnsureCreatedAsync();
        }
        else
        {
            // För PostgreSQL: Använd migrationer med retry-logik
            var retries = 10;
            while (retries > 0)
            {
                try
                {
                    await context.Database.MigrateAsync();
                    break;
                }
                catch (Exception)
                {
                    retries--;
                    if (retries == 0) throw;
                    logger.LogWarning("Väntar på databas... {Retries} försök kvar.", retries);
                    await Task.Delay(2000);
                }
            }
        }

        await SeedData.InitializeAsync(app.Services);
        logger.LogInformation("Databasen har initierats framgångsrikt.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Ett fel uppstod vid initiering av databasen.");
    }
}

app.Run();

