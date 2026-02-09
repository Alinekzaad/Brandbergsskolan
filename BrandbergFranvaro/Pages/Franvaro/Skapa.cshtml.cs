using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BrandbergFranvaro.Data;
using BrandbergFranvaro.Models;
using BrandbergFranvaro.Services;

namespace BrandbergFranvaro.Pages.Franvaro;

[Authorize]
public class SkapaModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IFileService _fileService;
    private readonly ILogger<SkapaModel> _logger;

    public SkapaModel(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IFileService fileService,
        ILogger<SkapaModel> logger)
    {
        _context = context;
        _userManager = userManager;
        _fileService = fileService;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required(ErrorMessage = "Typ av frånvaro krävs.")]
        [Display(Name = "Typ")]
        public AbsenceType Type { get; set; }

        [Required(ErrorMessage = "Startdatum krävs.")]
        [DataType(DataType.Date)]
        [Display(Name = "Startdatum")]
        public DateTime StartDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Slutdatum krävs.")]
        [DataType(DataType.Date)]
        [Display(Name = "Slutdatum")]
        public DateTime EndDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Omfattning krävs.")]
        [Display(Name = "Omfattning")]
        public DayPart DayPart { get; set; } = DayPart.Heldag;

        [MaxLength(1000, ErrorMessage = "Kommentaren får vara max 1000 tecken.")]
        [Display(Name = "Kommentar")]
        public string? Comment { get; set; }

        [Display(Name = "Bilaga")]
        public IFormFile? Attachment { get; set; }
    }

    public void OnGet()
    {
        Input.StartDate = DateTime.Today;
        Input.EndDate = DateTime.Today;
    }

    public async Task<IActionResult> OnPostAsync()
    {
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

        // Validera filuppladdning
        string? attachmentPath = null;
        if (Input.Attachment != null && Input.Attachment.Length > 0)
        {
            var (success, filePath, errorMessage) = await _fileService.SaveFileAsync(Input.Attachment);
            if (!success)
            {
                ModelState.AddModelError("Input.Attachment", errorMessage ?? "Fel vid uppladdning av fil.");
            }
            else
            {
                attachmentPath = filePath;
            }
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Challenge();
        }

        var request = new AbsenceRequest
        {
            UserId = user.Id,
            Type = Input.Type,
            StartDate = Input.StartDate,
            EndDate = Input.EndDate,
            DayPart = Input.DayPart,
            Comment = Input.Comment,
            Status = AbsenceStatus.Skickad,
            AttachmentPath = attachmentPath,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        _context.AbsenceRequests.Add(request);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Användare {UserId} skapade frånvaroärende {RequestId}", user.Id, request.Id);

        TempData["SuccessMessage"] = "Din frånvaroanmälan har skickats in!";
        return RedirectToPage("/Franvaro/MinaArenden");
    }
}

