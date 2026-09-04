using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MurendeniConstructionArchitects.Data;
using MurendeniConstructionArchitects.Models;

namespace MurendeniConstructionArchitects.Pages.Portal;

public class MyProjectsModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public MyProjectsModel(ApplicationDbContext db)
    {
        _db = db;
    }

    public List<ProjectProfile> Projects { get; set; } = new();

    public async Task OnGetAsync()
    {
        int clientId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        Projects = await _db.Profiles
            .Where(p => p.ClientId == clientId)
            .OrderByDescending(p => p.UpdatedAt)
            .ToListAsync();
    }
}