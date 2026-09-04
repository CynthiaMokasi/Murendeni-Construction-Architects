using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MurendeniConstructionArchitects.Data;
using MurendeniConstructionArchitects.Models;

namespace MurendeniConstructionArchitects.Pages.Portfolio;

public class DetailsModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public DetailsModel(ApplicationDbContext db)
    {
        _db = db;
    }

    public ProjectProfile? Project { get; set; }

    // The {id} in the URL (e.g. /Portfolio/Details/5) is passed in here automatically
    // because the file is named Details.cshtml with an {id:int} route below.
    public async Task<IActionResult> OnGetAsync(int id)
    {
        Project = await _db.Profiles
            .FirstOrDefaultAsync(p => p.ProfileId == id && p.IsPublished);

        if (Project == null)
        {
            // Either the id doesn't exist, or the project isn't published -
            // either way, a public visitor shouldn't see it.
            return NotFound();
        }

        return Page();
    }
}
