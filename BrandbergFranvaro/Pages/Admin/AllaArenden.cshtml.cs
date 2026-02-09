using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BrandbergFranvaro.Data;
using BrandbergFranvaro.Models;
using BrandbergFranvaro.Services;

namespace BrandbergFranvaro.Pages.Admin;

[Authorize(Roles = "Admin")]
public class AllaArendenModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly IFileService _fileService;
    private readonly ILogger<AllaArendenModel> _logger;

    public AllaArendenModel(
        ApplicationDbContext context,
        IFileService fileService,
        ILogger<AllaArendenModel> logger)
    {
        _context = context;
        _fileService = fileService;
        _logger = logger;
    }

    public List<AbsenceRequest> Requests { get; set; } = new();
    public int PendingCount { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? FilterPerson { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? FilterFromDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? FilterToDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public AbsenceType? FilterType { get; set; }

    [BindProperty(SupportsGet = true)]
    public AbsenceStatus? FilterStatus { get; set; }

    public async Task OnGetAsync()
    {
        var query = _context.AbsenceRequests
            .Include(r => r.User)
            .AsQueryable();

        // Filtrera på person (namn eller e-post)
        if (!string.IsNullOrWhiteSpace(FilterPerson))
        {
            var searchTerm = FilterPerson.ToLower();
            query = query.Where(r => 
                (r.User != null && r.User.Email != null && r.User.Email.ToLower().Contains(searchTerm)) ||
                (r.User != null && r.User.FirstName != null && r.User.FirstName.ToLower().Contains(searchTerm)) ||
                (r.User != null && r.User.LastName != null && r.User.LastName.ToLower().Contains(searchTerm)));
        }

        // Filtrera på datumintervall
        if (FilterFromDate.HasValue)
        {
            query = query.Where(r => r.StartDate >= FilterFromDate.Value || r.EndDate >= FilterFromDate.Value);
        }

        if (FilterToDate.HasValue)
        {
            query = query.Where(r => r.StartDate <= FilterToDate.Value || r.EndDate <= FilterToDate.Value);
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

        PendingCount = Requests.Count(r => r.Status == AbsenceStatus.Skickad);
    }

    public async Task<IActionResult> OnPostApproveAsync(Guid id)
    {
        var request = await _context.AbsenceRequests.FindAsync(id);
        
        if (request == null)
        {
            TempData["ErrorMessage"] = "Ärendet hittades inte.";
            return RedirectToPage();
        }

        if (request.Status != AbsenceStatus.Skickad)
        {
            TempData["ErrorMessage"] = "Endast ärenden med status \"Skickad\" kan godkännas.";
            return RedirectToPage();
        }

        request.Status = AbsenceStatus.Godkänd;
        request.AdminComment = "Godkänt.";
        request.UpdatedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Admin godkände ärende {RequestId}", id);

        TempData["SuccessMessage"] = "Ärendet har godkänts.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        var request = await _context.AbsenceRequests.FindAsync(id);
        
        if (request == null)
        {
            TempData["ErrorMessage"] = "Ärendet hittades inte.";
            return RedirectToPage();
        }

        // Ta bort eventuell bilaga
        if (!string.IsNullOrEmpty(request.AttachmentPath))
        {
            await _fileService.DeleteFileAsync(request.AttachmentPath);
        }

        _context.AbsenceRequests.Remove(request);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Admin tog bort ärende {RequestId}", id);

        TempData["SuccessMessage"] = "Ärendet har tagits bort.";
        return RedirectToPage();
    }
}

