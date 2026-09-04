using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MurendeniConstructionArchitects.Data;
using MurendeniConstructionArchitects.Models;

namespace MurendeniConstructionArchitects.Pages.Portal;

public class ProjectDetailsModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public ProjectDetailsModel(ApplicationDbContext db)
    {
        _db = db;
    }

    public ProjectProfile Project { get; set; } = null!;
    public List<Design> Files { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        int clientId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // Matching on BOTH id and clientId in one query means a client
        // can never load someone else's project just by guessing a URL.
        var project = await _db.Profiles
            .FirstOrDefaultAsync(p => p.ProfileId == id && p.ClientId == clientId);

        if (project == null)
        {
            return NotFound();
        }

        Project = project;

        Files = await _db.Designs
            .Where(d => d.ProfileId == id)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();

        return Page();
    }
}