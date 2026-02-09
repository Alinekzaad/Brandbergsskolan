using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BrandbergFranvaro.Data;
using BrandbergFranvaro.Models;
using System.ComponentModel.DataAnnotations;

namespace BrandbergFranvaro.Pages.Admin;

[Authorize(Roles = "Admin")]
public class SkapaAnvandareModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<SkapaAnvandareModel> _logger;

    public SkapaAnvandareModel(
        UserManager<ApplicationUser> userManager,
        ILogger<SkapaAnvandareModel> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public List<ApplicationUser> Users { get; set; } = new();

    public class InputModel
    {
        [Required(ErrorMessage = "E-post krävs")]
        [EmailAddress(ErrorMessage = "Ogiltig e-postadress")]
        [Display(Name = "E-post")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Förnamn krävs")]
        [StringLength(100, ErrorMessage = "Förnamn får max vara {1} tecken")]
        [Display(Name = "Förnamn")]
        public string FirstName { get; set; } = "";

        [Required(ErrorMessage = "Efternamn krävs")]
        [StringLength(100, ErrorMessage = "Efternamn får max vara {1} tecken")]
        [Display(Name = "Efternamn")]
        public string LastName { get; set; } = "";

        [Required(ErrorMessage = "Roll krävs")]
        [Display(Name = "Roll")]
        public string Role { get; set; } = SeedData.PersonalRole;

        [Required(ErrorMessage = "Lösenord krävs")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Lösenord måste vara minst {2} tecken")]
        [DataType(DataType.Password)]
        [Display(Name = "Lösenord")]
        public string Password { get; set; } = "";

        [DataType(DataType.Password)]
        [Display(Name = "Bekräfta lösenord")]
        [Compare("Password", ErrorMessage = "Lösenorden matchar inte")]
        public string ConfirmPassword { get; set; } = "";
    }

    public async Task OnGetAsync()
    {
        await LoadUsersAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadUsersAsync();
            return Page();
        }

        // Kontrollera om e-post redan finns
        var existingUser = await _userManager.FindByEmailAsync(Input.Email);
        if (existingUser != null)
        {
            ModelState.AddModelError("Input.Email", "E-postadressen används redan av en annan användare.");
            await LoadUsersAsync();
            return Page();
        }

        var user = new ApplicationUser
        {
            UserName = Input.Email,
            Email = Input.Email,
            EmailConfirmed = true,
            FirstName = Input.FirstName,
            LastName = Input.LastName
        };

        var result = await _userManager.CreateAsync(user, Input.Password);

        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, Input.Role);
            
            var roleDisplay = Input.Role == SeedData.AdminRole ? "Admin" : "Personal";
            _logger.LogInformation("Admin skapade ny användare: {Email} med roll {Role}", Input.Email, Input.Role);
            TempData["SuccessMessage"] = $"Användaren {Input.FirstName} {Input.LastName} ({Input.Email}) har skapats som {roleDisplay}.";
            
            return RedirectToPage();
        }

        foreach (var error in result.Errors)
        {
            var errorMessage = error.Code switch
            {
                "PasswordRequiresNonAlphanumeric" => "Lösenordet måste innehålla minst ett specialtecken.",
                "PasswordRequiresDigit" => "Lösenordet måste innehålla minst en siffra.",
                "PasswordRequiresUpper" => "Lösenordet måste innehålla minst en stor bokstav.",
                "PasswordRequiresLower" => "Lösenordet måste innehålla minst en liten bokstav.",
                "DuplicateUserName" => "Användarnamnet används redan.",
                "DuplicateEmail" => "E-postadressen används redan.",
                _ => error.Description
            };
            ModelState.AddModelError(string.Empty, errorMessage);
        }

        await LoadUsersAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        
        if (user == null)
        {
            TempData["ErrorMessage"] = "Användaren hittades inte.";
            return RedirectToPage();
        }

        // Förhindra borttagning av sig själv
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser?.Id == user.Id)
        {
            TempData["ErrorMessage"] = "Du kan inte ta bort ditt eget konto.";
            return RedirectToPage();
        }

        // Förhindra borttagning av admins
        if (await _userManager.IsInRoleAsync(user, "Admin"))
        {
            TempData["ErrorMessage"] = "Du kan inte ta bort en administratör.";
            return RedirectToPage();
        }

        var result = await _userManager.DeleteAsync(user);
        
        if (result.Succeeded)
        {
            _logger.LogInformation("Admin tog bort användare: {Email}", user.Email);
            TempData["SuccessMessage"] = $"Användaren {user.FullName} har tagits bort.";
        }
        else
        {
            TempData["ErrorMessage"] = "Kunde inte ta bort användaren.";
        }

        return RedirectToPage();
    }

    private async Task LoadUsersAsync()
    {
        Users = await _userManager.Users
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .ToListAsync();
    }
}

