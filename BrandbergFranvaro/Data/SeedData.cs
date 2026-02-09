using Microsoft.AspNetCore.Identity;
using BrandbergFranvaro.Models;

namespace BrandbergFranvaro.Data;

public static class SeedData
{
    public const string AdminRole = "Admin";
    public const string PersonalRole = "Personal";

    public const string AdminEmail = "admin@brandberg.se";
    public const string PersonalEmail = "personal@brandberg.se";
    public const string DefaultPassword = "Password123!";

    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;

        var logger = services.GetRequiredService<ILogger<Program>>();
        
        try
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var context = services.GetRequiredService<ApplicationDbContext>();

            await SeedRolesAsync(roleManager, logger);
            await SeedUsersAsync(userManager, logger);
            await SeedAbsenceRequestsAsync(context, userManager, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ett fel uppstod vid seeding av databasen.");
            throw;
        }
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager, ILogger logger)
    {
        string[] roles = { AdminRole, PersonalRole };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
                logger.LogInformation("Skapade roll: {Role}", role);
            }
        }
    }

    private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager, ILogger logger)
    {
        // Skapa Admin-användare
        var adminUser = await userManager.FindByEmailAsync(AdminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = AdminEmail,
                Email = AdminEmail,
                EmailConfirmed = true,
                FirstName = "Admin",
                LastName = "Administratör"
            };

            var result = await userManager.CreateAsync(adminUser, DefaultPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, AdminRole);
                logger.LogInformation("Skapade admin-användare: {Email}", AdminEmail);
            }
            else
            {
                logger.LogError("Kunde inte skapa admin: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        // Skapa Personal-användare
        var personalUser = await userManager.FindByEmailAsync(PersonalEmail);
        if (personalUser == null)
        {
            personalUser = new ApplicationUser
            {
                UserName = PersonalEmail,
                Email = PersonalEmail,
                EmailConfirmed = true,
                FirstName = "Anna",
                LastName = "Andersson"
            };

            var result = await userManager.CreateAsync(personalUser, DefaultPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(personalUser, PersonalRole);
                logger.LogInformation("Skapade personal-användare: {Email}", PersonalEmail);
            }
            else
            {
                logger.LogError("Kunde inte skapa personal: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        // Skapa ytterligare en personal för mer testdata
        var personal2Email = "erik@brandberg.se";
        var personal2User = await userManager.FindByEmailAsync(personal2Email);
        if (personal2User == null)
        {
            personal2User = new ApplicationUser
            {
                UserName = personal2Email,
                Email = personal2Email,
                EmailConfirmed = true,
                FirstName = "Erik",
                LastName = "Eriksson"
            };

            var result = await userManager.CreateAsync(personal2User, DefaultPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(personal2User, PersonalRole);
                logger.LogInformation("Skapade personal-användare: {Email}", personal2Email);
            }
        }
    }

    private static async Task SeedAbsenceRequestsAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger logger)
    {
        if (context.AbsenceRequests.Any())
        {
            logger.LogInformation("Frånvaroärenden finns redan - hoppar över seeding.");
            return;
        }

        var personalUser = await userManager.FindByEmailAsync(PersonalEmail);
        var personal2User = await userManager.FindByEmailAsync("erik@brandberg.se");

        if (personalUser == null)
        {
            logger.LogWarning("Personal-användare hittades inte - kan inte skapa testärenden.");
            return;
        }

        var today = DateTime.Today;
        var requests = new List<AbsenceRequest>
        {
            // Anna Anderssons ärenden
            new AbsenceRequest
            {
                UserId = personalUser.Id,
                Type = AbsenceType.Sjuk,
                StartDate = today.AddDays(-15),
                EndDate = today.AddDays(-13),
                DayPart = DayPart.Heldag,
                Status = AbsenceStatus.Godkänd,
                Comment = "Förkylning med feber.",
                AdminComment = "Godkänt. Krya på dig!",
                CreatedAtUtc = DateTime.UtcNow.AddDays(-16)
            },
            new AbsenceRequest
            {
                UserId = personalUser.Id,
                Type = AbsenceType.VAB,
                StartDate = today.AddDays(-8),
                EndDate = today.AddDays(-7),
                DayPart = DayPart.Heldag,
                Status = AbsenceStatus.Godkänd,
                Comment = "Barn sjukt i magsjuka.",
                AdminComment = "OK",
                CreatedAtUtc = DateTime.UtcNow.AddDays(-9)
            },
            new AbsenceRequest
            {
                UserId = personalUser.Id,
                Type = AbsenceType.Semester,
                StartDate = today.AddDays(30),
                EndDate = today.AddDays(44),
                DayPart = DayPart.Heldag,
                Status = AbsenceStatus.Skickad,
                Comment = "Sommarledighet.",
                CreatedAtUtc = DateTime.UtcNow.AddDays(-2)
            },
            new AbsenceRequest
            {
                UserId = personalUser.Id,
                Type = AbsenceType.Ledig,
                StartDate = today.AddDays(7),
                EndDate = today.AddDays(7),
                DayPart = DayPart.Förmiddag,
                Status = AbsenceStatus.Skickad,
                Comment = "Tandläkarbesök.",
                CreatedAtUtc = DateTime.UtcNow.AddDays(-1)
            },
            new AbsenceRequest
            {
                UserId = personalUser.Id,
                Type = AbsenceType.Annat,
                StartDate = today.AddDays(-30),
                EndDate = today.AddDays(-30),
                DayPart = DayPart.Eftermiddag,
                Status = AbsenceStatus.Avslagen,
                Comment = "Behöver hämta ut paket på posten.",
                AdminComment = "Avslås - detta kan göras utanför arbetstid. Posten har öppet till 20:00.",
                CreatedAtUtc = DateTime.UtcNow.AddDays(-31)
            }
        };

        // Erik Erikssons ärenden
        if (personal2User != null)
        {
            requests.AddRange(new[]
            {
                new AbsenceRequest
                {
                    UserId = personal2User.Id,
                    Type = AbsenceType.Sjuk,
                    StartDate = today.AddDays(-5),
                    EndDate = today.AddDays(-3),
                    DayPart = DayPart.Heldag,
                    Status = AbsenceStatus.Godkänd,
                    Comment = "Migrän.",
                    AdminComment = "Godkänt.",
                    CreatedAtUtc = DateTime.UtcNow.AddDays(-6)
                },
                new AbsenceRequest
                {
                    UserId = personal2User.Id,
                    Type = AbsenceType.Semester,
                    StartDate = today.AddDays(60),
                    EndDate = today.AddDays(74),
                    DayPart = DayPart.Heldag,
                    Status = AbsenceStatus.Skickad,
                    Comment = "Höstledighet med familjen.",
                    CreatedAtUtc = DateTime.UtcNow.AddDays(-3)
                },
                new AbsenceRequest
                {
                    UserId = personal2User.Id,
                    Type = AbsenceType.VAB,
                    StartDate = today.AddDays(-20),
                    EndDate = today.AddDays(-19),
                    DayPart = DayPart.Heldag,
                    Status = AbsenceStatus.Godkänd,
                    Comment = "Barn har vattkoppor.",
                    AdminComment = "OK, godkänt.",
                    CreatedAtUtc = DateTime.UtcNow.AddDays(-21)
                },
                new AbsenceRequest
                {
                    UserId = personal2User.Id,
                    Type = AbsenceType.Ledig,
                    StartDate = today.AddDays(14),
                    EndDate = today.AddDays(14),
                    DayPart = DayPart.Heldag,
                    Status = AbsenceStatus.Skickad,
                    Comment = "Flyttdag.",
                    CreatedAtUtc = DateTime.UtcNow
                }
            });
        }

        context.AbsenceRequests.AddRange(requests);
        await context.SaveChangesAsync();
        
        logger.LogInformation("Skapade {Count} testärenden för frånvaro.", requests.Count);
    }
}

