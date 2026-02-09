using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BrandbergFranvaro.Data;
using BrandbergFranvaro.Models;
using BrandbergFranvaro.Services;

namespace BrandbergFranvaro.Pages.Admin;

[Authorize(Roles = "Admin")]
public class DetaljerModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly IFileService _fileService;
    private readonly ILogger<DetaljerModel> _logger;

    public DetaljerModel(
        ApplicationDbContext context,
        IFileService fileService,
        ILogger<DetaljerModel> logger)
    {
        _context = context;
        _fileService = fileService;
        _logger = logger;
    }

    public new AbsenceRequest? Request { get; set; }

    [BindProperty]
    [Display(Name = "Admin-kommentar")]
    [MaxLength(1000, ErrorMessage = "Kommentaren får vara max 1000 tecken.")]
    public string? AdminComment { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        Request = await _context.AbsenceRequests
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (Request == null)
        {
            TempData["ErrorMessage"] = "Ärendet hittades inte.";
            return RedirectToPage("/Admin/AllaArenden");
        }

        AdminComment = Request.AdminComment;
        return Page();
    }

    public async Task<IActionResult> OnPostApproveAsync(Guid id)
    {
        var request = await _context.AbsenceRequests.FindAsync(id);
        
        if (request == null)
        {
            TempData["ErrorMessage"] = "Ärendet hittades inte.";
            return RedirectToPage("/Admin/AllaArenden");
        }

        request.Status = AbsenceStatus.Godkänd;
        request.AdminComment = string.IsNullOrWhiteSpace(AdminComment) ? "Godkänt." : AdminComment;
        request.UpdatedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Admin godkände ärende {RequestId}", id);

        TempData["SuccessMessage"] = "Ärendet har godkänts.";
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostRejectAsync(Guid id)
    {
        var request = await _context.AbsenceRequests.FindAsync(id);
        
        if (request == null)
        {
            TempData["ErrorMessage"] = "Ärendet hittades inte.";
            return RedirectToPage("/Admin/AllaArenden");
        }

        // Kommentar är obligatorisk vid avslag
        if (string.IsNullOrWhiteSpace(AdminComment))
        {
            Request = await _context.AbsenceRequests
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);

            ModelState.AddModelError("AdminComment", "Kommentar är obligatorisk vid avslag.");
            return Page();
        }

        request.Status = AbsenceStatus.Avslagen;
        request.AdminComment = AdminComment;
        request.UpdatedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Admin avslog ärende {RequestId}", id);

        TempData["SuccessMessage"] = "Ärendet har avslagits.";
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostResetAsync(Guid id)
    {
        var request = await _context.AbsenceRequests.FindAsync(id);
        
        if (request == null)
        {
            TempData["ErrorMessage"] = "Ärendet hittades inte.";
            return RedirectToPage("/Admin/AllaArenden");
        }

        request.Status = AbsenceStatus.Skickad;
        request.AdminComment = null;
        request.UpdatedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Admin återställde ärende {RequestId} till Skickad", id);

        TempData["SuccessMessage"] = "Ärendet har återställts till \"Skickad\".";
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        var request = await _context.AbsenceRequests.FindAsync(id);
        
        if (request == null)
        {
            TempData["ErrorMessage"] = "Ärendet hittades inte.";
            return RedirectToPage("/Admin/AllaArenden");
        }

        // Ta bort eventuell bilaga
        if (!string.IsNullOrEmpty(request.AttachmentPath))
        {
            await _fileService.DeleteFileAsync(request.AttachmentPath);
        }

        _context.AbsenceRequests.Remove(request);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Admin tog bort ärende {RequestId} permanent", id);

        TempData["SuccessMessage"] = "Ärendet har tagits bort permanent.";
        return RedirectToPage("/Admin/AllaArenden");
    }
}

