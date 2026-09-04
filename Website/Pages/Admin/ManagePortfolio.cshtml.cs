using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MurendeniConstructionArchitects.Data;
using MurendeniConstructionArchitects.Models;

namespace MurendeniConstructionArchitects.Pages.Admin;

public class ManagePortfolioModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public ManagePortfolioModel(ApplicationDbContext db)
    {
        _db = db;
    }

    public List<ProjectProfile> Projects { get; set; } = new();

    public async Task OnGetAsync()
    {
        // Everything shows here, published or not - this page is where the
        // admin decides WHICH ones go public, so unpublished ones need to
        // be visible too (just marked clearly).
        Projects = await _db.Profiles
            .OrderByDescending(p => p.IsPublished)
            .ThenByDescending(p => p.UpdatedAt)
            .ToListAsync();
    }

    // Called by the "Publish" / "Unpublish" button on each card - a quick
    // toggle without having to open the full Edit Project form.
    public async Task<IActionResult> OnPostTogglePublishAsync(int id)
    {
        var project = await _db.Profiles.FindAsync(id);
        if (project != null)
        {
            project.IsPublished = !project.IsPublished;
            await _db.SaveChangesAsync();
        }

        return RedirectToPage();
    }
}