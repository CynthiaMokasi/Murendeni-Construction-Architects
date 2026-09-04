using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MurendeniConstructionArchitects.Data;
using MurendeniConstructionArchitects.Models;

namespace MurendeniConstructionArchitects.Pages.Admin;

public class UpdateProjectStatusModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public UpdateProjectStatusModel(ApplicationDbContext db)
    {
        _db = db;
    }

    [BindProperty]
    public ProjectStatus Status { get; set; }

    public ProjectProfile Project { get; set; } = null!;
    public List<Design> Files { get; set; } = new();
    public string? SuccessMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var project = await LoadOwnedProjectAsync(id);
        if (project == null) return NotFound(); // covers "doesn't exist" AND "not yours"

        Project = project;
        Status = project.Status;
        Files = await _db.Designs.Where(d => d.ProfileId == id).OrderByDescending(d => d.CreatedAt).ToListAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var project = await LoadOwnedProjectAsync(id);
        if (project == null) return NotFound();

        project.Status = Status;
        project.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        SuccessMessage = "Status updated.";
        Project = project;
        Files = await _db.Designs.Where(d => d.ProfileId == id).OrderByDescending(d => d.CreatedAt).ToListAsync();
        return Page();
    }

    // Loads the project ONLY if it's assigned to the logged-in designer
    // (or if the caller is Admin, who can see anything). This is what
    // stops a designer from editing another designer's project just by
    // guessing a different id in the URL.
    private async Task<ProjectProfile?> LoadOwnedProjectAsync(int id)
    {
        string role = User.FindFirst("role")?.Value ?? "";
        var project = await _db.Profiles.Include(p => p.Client).FirstOrDefaultAsync(p => p.ProfileId == id);

        if (project == null) return null;
        if (role == "Admin") return project;

        int employeeId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return project.EmployeeId == employeeId ? project : null;
    }
}