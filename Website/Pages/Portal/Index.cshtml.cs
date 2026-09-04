using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MurendeniConstructionArchitects.Data;
using MurendeniConstructionArchitects.Models;

namespace MurendeniConstructionArchitects.Pages.Portal;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public IndexModel(ApplicationDbContext db)
    {
        _db = db;
    }

    public string ClientName { get; set; } = string.Empty;
    public int TotalProjects { get; set; }
    public int InProgressCount { get; set; }
    public int CompletedCount { get; set; }
    public List<ProjectProfile> RecentProjects { get; set; } = new();

    public async Task OnGetAsync()
    {
        // Because AuthorizeFolder("/Portal") is set in Program.cs, we only
        // ever reach this line if the visitor is already logged in - so
        // this claim is guaranteed to exist.
        int clientId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        ClientName = User.FindFirstValue(ClaimTypes.Name) ?? "there";

        var projects = await _db.Profiles
            .Where(p => p.ClientId == clientId)
            .OrderByDescending(p => p.UpdatedAt)
            .ToListAsync();

        TotalProjects = projects.Count;
        InProgressCount = projects.Count(p => p.Status == ProjectStatus.InProgress);
        CompletedCount = projects.Count(p => p.Status == ProjectStatus.Completed);
        RecentProjects = projects.Take(5).ToList();
    }
}