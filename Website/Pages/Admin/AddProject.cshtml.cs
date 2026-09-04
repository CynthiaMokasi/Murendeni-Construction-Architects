using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MurendeniConstructionArchitects.Data;
using MurendeniConstructionArchitects.Models;

namespace MurendeniConstructionArchitects.Pages.Admin;

public class AddProjectInput
{
    [Required(ErrorMessage = "Please choose a client.")]
    public int ClientId { get; set; }

    [Required(ErrorMessage = "Please assign an employee.")]
    public int EmployeeId { get; set; }

    [Required, MaxLength(150)]
    public string ProfileName { get; set; } = string.Empty;

    public PropertyType PropertyType { get; set; } = PropertyType.Residential;

    public string? Description { get; set; }

    [MaxLength(150)]
    public string? Location { get; set; }

    public int? YearCompleted { get; set; }
}

public class AddProjectModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public AddProjectModel(ApplicationDbContext db)
    {
        _db = db;
    }

    [BindProperty]
    public AddProjectInput Input { get; set; } = new();

    // These fill the two dropdowns on the page.
    public List<Client> Clients { get; set; } = new();
    public List<Employee> Employees { get; set; } = new();

    public async Task OnGetAsync()
    {
        await LoadDropdownsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadDropdownsAsync();
            return Page();
        }

        var project = new ProjectProfile
        {
            ClientId = Input.ClientId,
            EmployeeId = Input.EmployeeId,
            ProfileName = Input.ProfileName,
            PropertyType = Input.PropertyType,
            Description = Input.Description,
            Location = Input.Location,
            YearCompleted = Input.YearCompleted,
            Status = ProjectStatus.Inquiry,
            IsPublished = false
        };

        _db.Profiles.Add(project);
        await _db.SaveChangesAsync();

        return RedirectToPage("/Admin/ManageProjects");
    }

    private async Task LoadDropdownsAsync()
    {
        Clients = await _db.Clients.OrderBy(c => c.FullName).ToListAsync();
        Employees = await _db.Employees.OrderBy(e => e.FullName).ToListAsync();
    }
}