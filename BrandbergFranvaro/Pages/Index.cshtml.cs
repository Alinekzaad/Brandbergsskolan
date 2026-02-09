using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BrandbergFranvaro.Data;
using BrandbergFranvaro.Models;

namespace BrandbergFranvaro.Pages;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public IndexModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public ApplicationUser? CurrentUser { get; set; }
    public List<AbsenceRequest> RecentRequests { get; set; } = new();
    public int PendingCount { get; set; }
    public int ApprovedCount { get; set; }
    public int RejectedCount { get; set; }

    public async Task OnGetAsync()
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return;
        }

        CurrentUser = await _userManager.GetUserAsync(User);
        if (CurrentUser == null) return;

        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

        if (User.IsInRole("Admin"))
        {
            // Admin ser alla ärenden
            var query = _context.AbsenceRequests
                .Include(r => r.User)
                .Where(r => r.CreatedAtUtc >= thirtyDaysAgo);

            PendingCount = await query.CountAsync(r => r.Status == AbsenceStatus.Skickad);
            ApprovedCount = await query.CountAsync(r => r.Status == AbsenceStatus.Godkänd);
            RejectedCount = await query.CountAsync(r => r.Status == AbsenceStatus.Avslagen);

            RecentRequests = await _context.AbsenceRequests
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAtUtc)
                .Take(10)
                .ToListAsync();
        }
        else
        {
            // Personal ser bara sina egna ärenden
            var query = _context.AbsenceRequests
                .Where(r => r.UserId == CurrentUser.Id);

            PendingCount = await query.CountAsync(r => r.Status == AbsenceStatus.Skickad);
            ApprovedCount = await query.CountAsync(r => r.Status == AbsenceStatus.Godkänd);
            RejectedCount = await query.CountAsync(r => r.Status == AbsenceStatus.Avslagen);

            RecentRequests = await query
                .OrderByDescending(r => r.CreatedAtUtc)
                .Take(5)
                .ToListAsync();
        }
    }
}

