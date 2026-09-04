using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MurendeniConstructionArchitects.Data;
using MurendeniConstructionArchitects.Models;

namespace MurendeniConstructionArchitects.Pages.Admin;

public class UploadDesignFileInput
{
    [Required(ErrorMessage = "Please select a project.")]
    public int ProfileId { get; set; }

    [Required(ErrorMessage = "Please give the file a title.")]
    [MaxLength(150)]
    public string DesignTitle { get; set; } = string.Empty;

    // Matches CK_Design_Status in the database - keep these values in sync
    // if the constraint ever changes.
    public string Status { get; set; } = "draft";

    [Required(ErrorMessage = "Please choose a file to upload.")]
    public IFormFile? UploadedFile { get; set; }
}

public class UploadDesignFileModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _environment;

    public UploadDesignFileModel(ApplicationDbContext db, IWebHostEnvironment environment)
    {
        _db = db;
        _environment = environment;
    }

    [BindProperty]
    public UploadDesignFileInput Input { get; set; } = new();

    public List<ProjectProfile> Projects { get; set; } = new();
    public List<Design> RecentUploads { get; set; } = new();
    public string? SuccessMessage { get; set; }

    public async Task OnGetAsync()
    {
        await LoadDataAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadDataAsync();
            return Page();
        }

        int employeeId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        string role = User.FindFirst("role")?.Value ?? "";
        if (role == "Designer")
        {
            bool ownsProject = await _db.Profiles.AnyAsync(p => p.ProfileId == Input.ProfileId && p.EmployeeId == employeeId);
            if (!ownsProject)
            {
                return Forbid();
            }
        }

        // ----------------------------------------------------------------- 
        // TEMPORARY: saving to a local folder so this works without any
        // Azure setup. Before going to production, swap this block for an
        // upload to Azure Blob Storage (per your original design docs) -
        // files saved here won't reliably survive an App Service restart.
        // -----------------------------------------------------------------
        string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
        Directory.CreateDirectory(uploadsFolder);

        string uniqueFileName = $"{Guid.NewGuid()}_{Input.UploadedFile!.FileName}";
        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await Input.UploadedFile.CopyToAsync(stream);
        }

        string publicUrl = $"/uploads/{uniqueFileName}";
        // -----------------------------------------------------------------

        var design = new Design
        {
            ProfileId = Input.ProfileId,
            EmployeeId = employeeId,
            DesignTitle = Input.DesignTitle,
            FileUrl = publicUrl,
            Status = Input.Status,
            IsLocked = false
        };

        _db.Designs.Add(design);

        var project = await _db.Profiles.FindAsync(Input.ProfileId);
        if (project != null)
        {
            project.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();

        SuccessMessage = $"'{Input.DesignTitle}' uploaded successfully.";
        Input = new();
        await LoadDataAsync();
        return Page();
    }

    private async Task LoadDataAsync()
    {
        string role = User.FindFirst("role")?.Value ?? "";
        var projectsQuery = _db.Profiles.AsQueryable();

        if (role == "Designer")
        {
            int employeeId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            projectsQuery = projectsQuery.Where(p => p.EmployeeId == employeeId);
        }

        Projects = await projectsQuery.OrderBy(p => p.ProfileName).ToListAsync();

        var uploadsQuery = _db.Designs.Include(d => d.Profile).AsQueryable();
        if (role == "Designer")
        {
            int employeeId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            uploadsQuery = uploadsQuery.Where(d => d.EmployeeId == employeeId);
        }

        RecentUploads = await uploadsQuery.OrderByDescending(d => d.CreatedAt).Take(10).ToListAsync();
    }
}