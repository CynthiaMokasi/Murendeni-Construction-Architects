using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MurendeniConstructionArchitects.Data;
using MurendeniConstructionArchitects.Models;

namespace MurendeniConstructionArchitects.Pages.Admin;

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
        // Admin visiting this page (allowed by the DesignerOrAdmin policy)
        // sees everything; a Designer only sees their own assignments.
        string role = User.FindFirst("role")?.Value ?? "";

        var query = _db.Profiles.Include(p => p.Client).AsQueryable();

        if (role == "Designer")
        {
            int employeeId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            query = query.Where(p => p.EmployeeId == employeeId);
        }

        Projects = await query.OrderByDescending(p => p.UpdatedAt).ToListAsync();
    }
}