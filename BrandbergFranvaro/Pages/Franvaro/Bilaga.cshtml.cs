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
public class BilagaModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IFileService _fileService;

    public BilagaModel(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IFileService fileService)
    {
        _context = context;
        _userManager = userManager;
        _fileService = fileService;
    }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Challenge();
        }

        AbsenceRequest? request;
        
        // Admin kan se alla bilagor
        if (User.IsInRole("Admin"))
        {
            request = await _context.AbsenceRequests.FirstOrDefaultAsync(r => r.Id == id);
        }
        else
        {
            // Personal kan bara se sina egna
            request = await _context.AbsenceRequests
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == user.Id);
        }

        if (request == null || string.IsNullOrEmpty(request.AttachmentPath))
        {
            return NotFound("Bilagan hittades inte.");
        }

        var fullPath = _fileService.GetFullPath(request.AttachmentPath);
        
        if (!System.IO.File.Exists(fullPath))
        {
            return NotFound("Filen finns inte på servern.");
        }

        var contentType = _fileService.GetContentType(request.AttachmentPath);
        var fileName = Path.GetFileName(request.AttachmentPath);

        return PhysicalFile(fullPath, contentType, fileName);
    }
}

