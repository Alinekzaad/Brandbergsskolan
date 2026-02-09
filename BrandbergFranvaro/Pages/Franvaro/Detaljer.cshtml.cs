using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BrandbergFranvaro.Data;
using BrandbergFranvaro.Models;

namespace BrandbergFranvaro.Pages.Franvaro;

[Authorize]
public class DetaljerModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public DetaljerModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public new AbsenceRequest? Request { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Challenge();
        }

        Request = await _context.AbsenceRequests
            .FirstOrDefaultAsync(r => r.Id == id && r.UserId == user.Id);

        if (Request == null)
        {
            TempData["ErrorMessage"] = "Ärendet hittades inte eller du har inte behörighet att visa det.";
            return RedirectToPage("/Franvaro/MinaArenden");
        }

        return Page();
    }
}

