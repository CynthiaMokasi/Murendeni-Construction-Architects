using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MurendeniConstructionArchitects.Data;
using MurendeniConstructionArchitects.Models;

namespace MurendeniConstructionArchitects.Pages.Admin;

public class EditProjectInput
{
    public string ProfileName { get; set; } = string.Empty;
    public int EmployeeId { get; set; }
    public PropertyType PropertyType { get; set; }
    public ProjectStatus Status { get; set; }
    public bool IsPublished { get; set; }
    public bool IsFeatured { get; set; }
    public string? Description { get; set; }
    public string? Location { get; set; }
    public int? YearCompleted { get; set; }
    public IFormFile? CoverImageFile { get; set; }
}

public class EditProjectModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _environment;

    public EditProjectModel(ApplicationDbContext db, IWebHostEnvironment environment)
    {
        _db = db;
        _environment = environment;
    }

    [BindProperty]
    public EditProjectInput Input { get; set; } = new();

    public List<Employee> Employees { get; set; } = new();
    public List<Design> ExistingFiles { get; set; } = new();
    public string CurrentRole { get; set; } = string.Empty;
    public string? CurrentCoverImageUrl { get; set; }
    public string? SuccessMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var project = await _db.Profiles.FindAsync(id);
        if (project == null) return NotFound();

        Input.ProfileName = project.ProfileName;
        Input.EmployeeId = project.EmployeeId;
        Input.PropertyType = project.PropertyType;
        Input.Status = project.Status;
        Input.IsPublished = project.IsPublished;
        Input.IsFeatured = project.IsFeatured;
        Input.Description = project.Description;
        Input.Location = project.Location;
        Input.YearCompleted = project.YearCompleted;

        CurrentCoverImageUrl = project.CoverImageUrl;
        CurrentRole = User.FindFirst("role")?.Value ?? "";
        ExistingFiles = await _db.Designs
            .Where(d => d.ProfileId == id)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();

        Employees = await _db.Employees.OrderBy(e => e.FullName).ToListAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var project = await _db.Profiles.FindAsync(id);
        if (project == null) return NotFound();

        CurrentRole = User.FindFirst("role")?.Value ?? "";

        // These fields apply regardless of role - Sales and Admin both
        // manage the day-to-day details of a project.
        project.ProfileName = Input.ProfileName;
        project.EmployeeId = Input.EmployeeId;
        project.PropertyType = Input.PropertyType;
        project.Status = Input.Status;
        project.Description = Input.Description;
        project.Location = Input.Location;
        project.YearCompleted = Input.YearCompleted;
        project.UpdatedAt = DateTime.UtcNow;

        // These are Admin-only, and MUST stay inside this check - the
        // checkboxes are hidden from Sales in the view, so an unchecked
        // (false) value would otherwise silently unpublish/unfeature
        // the project every time Sales saves an edit.
        if (CurrentRole == "Admin")
        {
            project.IsPublished = Input.IsPublished;
            project.IsFeatured = Input.IsFeatured;

            if (Input.CoverImageFile != null && Input.CoverImageFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = $"{Guid.NewGuid()}_{Input.CoverImageFile.FileName}";
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await Input.CoverImageFile.CopyToAsync(stream);
                }

                project.CoverImageUrl = $"/uploads/{uniqueFileName}";
            }
        }

        await _db.SaveChangesAsync();

        SuccessMessage = "Project updated.";
        CurrentCoverImageUrl = project.CoverImageUrl;
        ExistingFiles = await _db.Designs
            .Where(d => d.ProfileId == id)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
        Employees = await _db.Employees.OrderBy(e => e.FullName).ToListAsync();
        return Page();
    }
}