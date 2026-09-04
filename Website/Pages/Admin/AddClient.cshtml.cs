using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MurendeniConstructionArchitects.Data;
using MurendeniConstructionArchitects.Helpers;
using MurendeniConstructionArchitects.Models;

namespace MurendeniConstructionArchitects.Pages.Admin;

public class AddClientInput
{
    [Required, MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Phone { get; set; }

    // If the admin leaves this blank, we generate a temporary one below -
    // saves them having to think one up on the spot.
    public string? TemporaryPassword { get; set; }
}

public class AddClientModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public AddClientModel(ApplicationDbContext db)
    {
        _db = db;
    }

    [BindProperty]
    public AddClientInput Input { get; set; } = new();

    // Shown once, after saving, so the admin can pass it on to the client.
    public string? GeneratedPassword { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        bool emailTaken = await _db.Clients.AnyAsync(c => c.Email == Input.Email);
        if (emailTaken)
        {
            ModelState.AddModelError(nameof(Input.Email), "A client with this email already exists.");
            return Page();
        }

        string password = string.IsNullOrWhiteSpace(Input.TemporaryPassword)
            ? GenerateTemporaryPassword()
            : Input.TemporaryPassword;

        var client = new Client
        {
            FullName = Input.FullName,
            Email = Input.Email,
            Phone = Input.Phone,
            PasswordHash = PasswordHasher.HashPassword(password)
        };

        _db.Clients.Add(client);
        await _db.SaveChangesAsync();

        GeneratedPassword = password;
        return Page(); // stay on the page so the admin can see and copy the password
    }

    private static string GenerateTemporaryPassword()
    {
        // Simple, readable temporary password - not meant to be permanent,
        // just enough for the client to log in and change it themselves.
        return "Welcome" + Random.Shared.Next(1000, 9999) + "!";
    }
}