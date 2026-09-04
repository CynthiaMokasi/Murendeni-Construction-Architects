using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MurendeniConstructionArchitects.Data;
using MurendeniConstructionArchitects.Models;

namespace MurendeniConstructionArchitects.Pages.Portfolio;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public IndexModel(ApplicationDbContext db)
    {
        _db = db;
    }

    public List<ProjectProfile> Projects { get; set; } = new();

    public async Task OnGetAsync()
    {
        // Public visitors should only ever see published projects.
        Projects = await _db.Profiles
            .Where(p => p.IsPublished)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }
}
