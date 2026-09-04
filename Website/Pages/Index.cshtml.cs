using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MurendeniConstructionArchitects.Data;
using MurendeniConstructionArchitects.Models;

namespace MurendeniConstructionArchitects.Pages;

public class IndexModel : PageModel
{
    // EF Core's "gateway" to the database. ASP.NET Core hands us one of
    // these automatically because we registered it in Program.cs.
    private readonly ApplicationDbContext _db;

    public IndexModel(ApplicationDbContext db)
    {
        _db = db;
    }

    // The Razor page (Index.cshtml) reads these to show "Featured Projects".
    public List<ProjectProfile> FeaturedProjects { get; set; } = new();
    public Dictionary<ProjectCategory?, List<ProjectProfile>> ProjectsByCategory { get; set; } = new();

    public async Task OnGetAsync()
    {
        FeaturedProjects = await _db.Profiles
            .Where(p => p.IsPublished && p.IsFeatured)
            .OrderByDescending(p => p.UpdatedAt)
            .Take(6)
            .ToListAsync();
    }
}
