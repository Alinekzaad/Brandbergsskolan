using System.ComponentModel.DataAnnotations;
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
public class RedigeraModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IFileService _fileService;
    private readonly ILogger<RedigeraModel> _logger;

    public RedigeraModel(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IFileService fileService,
        ILogger<RedigeraModel> logger)
    {
        _context = context;
        _userManager = userManager;
        _fileService = fileService;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public Guid RequestId { get; set; }
    public string? ExistingAttachment { get; set; }

    public class InputModel
    {
        [Required(ErrorMessage = "Typ av frånvaro krävs.")]
        [Display(Name = "Typ")]
        public AbsenceType Type { get; set; }

        [Required(ErrorMessage = "Startdatum krävs.")]
        [DataType(DataType.Date)]
        [Display(Name = "Startdatum")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Slutdatum krävs.")]
        [DataType(DataType.Date)]
        [Display(Name = "Slutdatum")]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Omfattning krävs.")]
        [Display(Name = "Omfattning")]
        public DayPart DayPart { get; set; }

        [MaxLength(1000, ErrorMessage = "Kommentaren får vara max 1000 tecken.")]
        [Display(Name = "Kommentar")]
        public string? Comment { get; set; }

        [Display(Name = "Bilaga")]
        public IFormFile? Attachment { get; set; }

        [Display(Name = "Ta bort bilaga")]
        public bool RemoveAttachment { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(Guid id)
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
            return RedirectToPage("/Franvaro/MinaArenden");
        }

        if (request.Status != AbsenceStatus.Skickad)
        {
            TempData["ErrorMessage"] = "Du kan endast redigera ärenden som har status \"Skickad\".";
            return RedirectToPage("/Franvaro/Detaljer", new { id });
        }

        RequestId = id;
        ExistingAttachment = request.AttachmentPath;
        Input = new InputModel
        {
            Type = request.Type,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            DayPart = request.DayPart,
            Comment = request.Comment
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id)
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
            return RedirectToPage("/Franvaro/MinaArenden");
        }

        if (request.Status != AbsenceStatus.Skickad)
        {
            TempData["ErrorMessage"] = "Du kan endast redigera ärenden som har status \"Skickad\".";
            return RedirectToPage("/Franvaro/Detaljer", new { id });
        }

        RequestId = id;
        ExistingAttachment = request.AttachmentPath;

        // Validera datum
        if (Input.EndDate < Input.StartDate)
        {
            ModelState.AddModelError("Input.EndDate", "Slutdatum kan inte vara före startdatum.");
        }

        // Validera max 365 dagar framåt
        var maxDate = DateTime.Today.AddDays(365);
        if (Input.StartDate > maxDate || Input.EndDate > maxDate)
        {
            ModelState.AddModelError("Input.StartDate", "Datum kan inte vara mer än 365 dagar framåt.");
        }

        // Validera att kommentar finns om typ är "Annat"
        if (Input.Type == AbsenceType.Annat && string.IsNullOrWhiteSpace(Input.Comment))
        {
            ModelState.AddModelError("Input.Comment", "Kommentar krävs när typ är \"Annat\".");
        }

        // Hantera filuppladdning
        string? newAttachmentPath = null;
        if (Input.Attachment != null && Input.Attachment.Length > 0)
        {
            var (success, filePath, errorMessage) = await _fileService.SaveFileAsync(Input.Attachment);
            if (!success)
            {
                ModelState.AddModelError("Input.Attachment", errorMessage ?? "Fel vid uppladdning av fil.");
            }
            else
            {
                newAttachmentPath = filePath;
            }
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Ta bort gammal bilaga om ny laddas upp eller om användaren vill ta bort
        if ((newAttachmentPath != null || Input.RemoveAttachment) && !string.IsNullOrEmpty(request.AttachmentPath))
        {
            await _fileService.DeleteFileAsync(request.AttachmentPath);
        }

        // Uppdatera request
        request.Type = Input.Type;
        request.StartDate = Input.StartDate;
        request.EndDate = Input.EndDate;
        request.DayPart = Input.DayPart;
        request.Comment = Input.Comment;
        request.UpdatedAtUtc = DateTime.UtcNow;

        if (newAttachmentPath != null)
        {
            request.AttachmentPath = newAttachmentPath;
        }
        else if (Input.RemoveAttachment)
        {
            request.AttachmentPath = null;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Användare {UserId} uppdaterade ärende {RequestId}", user.Id, id);

        TempData["SuccessMessage"] = "Ärendet har uppdaterats.";
        return RedirectToPage("/Franvaro/Detaljer", new { id });
    }
}

