using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MurendeniConstructionArchitects.Data;
using MurendeniConstructionArchitects.Models;

namespace MurendeniConstructionArchitects.Pages.Admin;

public class EditClientInput
{
    [Required, MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Phone { get; set; }
}

public class EditClientModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public EditClientModel(ApplicationDbContext db)
    {
        _db = db;
    }

    [BindProperty]
    public EditClientInput Input { get; set; } = new();

    public List<ProjectProfile> Projects { get; set; } = new();
    public string? SuccessMessage { get; set; }

    private int _clientId;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var client = await _db.Clients.FindAsync(id);
        if (client == null) return NotFound();

        _clientId = id;
        Input.FullName = client.FullName;
        Input.Email = client.Email;
        Input.Phone = client.Phone;

        Projects = await _db.Profiles
            .Where(p => p.ClientId == id)
            .OrderByDescending(p => p.UpdatedAt)
            .ToListAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var client = await _db.Clients.FindAsync(id);
        if (client == null) return NotFound();

        if (!ModelState.IsValid)
        {
            Projects = await _db.Profiles.Where(p => p.ClientId == id).ToListAsync();
            return Page();
        }

        client.FullName = Input.FullName;
        client.Email = Input.Email;
        client.Phone = Input.Phone;
        await _db.SaveChangesAsync();

        SuccessMessage = "Client details updated.";
        Projects = await _db.Profiles.Where(p => p.ClientId == id).ToListAsync();
        return Page();
    }
}