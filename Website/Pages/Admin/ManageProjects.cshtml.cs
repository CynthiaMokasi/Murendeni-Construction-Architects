using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MurendeniConstructionArchitects.Data;
using MurendeniConstructionArchitects.Models;

namespace MurendeniConstructionArchitects.Pages.Admin;

public class ManageProjectsModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public ManageProjectsModel(ApplicationDbContext db)
    {
        _db = db;
    }

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    // "All" plus the four real statuses - shown as filter buttons on the page.
    [BindProperty(SupportsGet = true)]
    public string StatusFilter { get; set; } = "All";

    public List<ProjectProfile> Projects { get; set; } = new();

    public async Task OnGetAsync()
    {
        // Include Client so we can show the client's name without a separate lookup.
        var query = _db.Profiles.Include(p => p.Client).AsQueryable();

        if (!string.IsNullOrWhiteSpace(Search))
        {
            query = query.Where(p => p.ProfileName.Contains(Search));
        }

        if (StatusFilter != "All" && Enum.TryParse<ProjectStatus>(StatusFilter, out var status))
        {
            query = query.Where(p => p.Status == status);
        }

        Projects = await query
            .OrderByDescending(p => p.UpdatedAt)
            .ToListAsync();
    }
}