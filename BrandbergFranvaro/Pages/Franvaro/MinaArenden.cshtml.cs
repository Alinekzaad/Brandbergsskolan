using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BrandbergFranvaro.Data;
using BrandbergFranvaro.Models;
using BrandbergFranvaro.Services;

namespace BrandbergFranvaro.Pages.Franvaro;

[Authorize]
public class MinaArendenModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IFileService _fileService;
    private readonly ILogger<MinaArendenModel> _logger;

    public MinaArendenModel(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IFileService fileService,
        ILogger<MinaArendenModel> logger)
    {
        _context = context;
        _userManager = userManager;
        _fileService = fileService;
        _logger = logger;
    }

    public List<AbsenceRequest> Requests { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? FilterMonth { get; set; }

    [BindProperty(SupportsGet = true)]
    public AbsenceType? FilterType { get; set; }

    [BindProperty(SupportsGet = true)]
    public AbsenceStatus? FilterStatus { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Challenge();
        }

        var query = _context.AbsenceRequests
            .Where(r => r.UserId == user.Id)
            .AsQueryable();

        // Filtrera på månad
        if (!string.IsNullOrEmpty(FilterMonth) && DateTime.TryParse(FilterMonth + "-01", out var month))
        {
            var startOfMonth = new DateTime(month.Year, month.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
            query = query.Where(r => r.StartDate <= endOfMonth && r.EndDate >= startOfMonth);
        }

        // Filtrera på typ
        if (FilterType.HasValue)
        {
            query = query.Where(r => r.Type == FilterType.Value);
        }

        // Filtrera på status
        if (FilterStatus.HasValue)
        {
            query = query.Where(r => r.Status == FilterStatus.Value);
        }

        Requests = await query
            .OrderByDescending(r => r.CreatedAtUtc)
            .ToListAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Challenge();
        }

        var request = await _context.AbsenceRequests
            .FirstOrDefaultAsync(r => r.Id == id && r.UserId == user.Id);

        if (request == null)
        {
            TempData["ErrorMessage"] = "Ärendet hittades inte.";
            return RedirectToPage();
        }

        // Kontrollera att ärendet kan tas bort (endast status Skickad)
        if (request.Status != AbsenceStatus.Skickad)
        {
            TempData["ErrorMessage"] = "Du kan endast ta bort ärenden som har status \"Skickad\".";
            return RedirectToPage();
        }

        // Ta bort eventuell bilaga
        if (!string.IsNullOrEmpty(request.AttachmentPath))
        {
            await _fileService.DeleteFileAsync(request.AttachmentPath);
        }

        _context.AbsenceRequests.Remove(request);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Användare {UserId} tog bort ärende {RequestId}", user.Id, id);

        TempData["SuccessMessage"] = "Ärendet har tagits bort.";
        return RedirectToPage();
    }
}

