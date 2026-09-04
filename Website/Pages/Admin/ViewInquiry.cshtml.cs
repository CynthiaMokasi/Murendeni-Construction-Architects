using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MurendeniConstructionArchitects.Data;
using MurendeniConstructionArchitects.Helpers;
using MurendeniConstructionArchitects.Models;

namespace MurendeniConstructionArchitects.Pages.Admin;

public class ViewInquiryInput
{
    public int? EmployeeId { get; set; }
    public InquiryStatus Status { get; set; }
}

public class ViewInquiryModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public ViewInquiryModel(ApplicationDbContext db)
    {
        _db = db;
    }

    [BindProperty]
    public ViewInquiryInput Input { get; set; } = new();

    public Inquiry Inquiry { get; set; } = null!;
    public List<Employee> Employees { get; set; } = new();
    public string? SuccessMessage { get; set; }
    public string? GeneratedPassword { get; set; } // shown once, right after creating an account

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var inquiry = await LoadInquiryAsync(id);
        if (inquiry == null) return NotFound();

        Inquiry = inquiry; 

        Input.EmployeeId = inquiry.EmployeeId;
        Input.Status = inquiry.Status;

        Employees = await _db.Employees.OrderBy(e => e.FullName).ToListAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var inquiry = await _db.Inquiries.FirstOrDefaultAsync(i => i.InquiryId == id);
        if (inquiry == null) return NotFound();

        inquiry.EmployeeId = Input.EmployeeId;
        inquiry.Status = Input.Status;
        await _db.SaveChangesAsync();

        SuccessMessage = "Inquiry updated.";
        Inquiry = (await LoadInquiryAsync(id))!;
        Employees = await _db.Employees.OrderBy(e => e.FullName).ToListAsync();
        return Page();
    }

    // Turns a guest inquiry into a real Client account, using whatever
    // contact details the visitor originally typed into the Contact form.
    public async Task<IActionResult> OnPostCreateClientAsync(int id)
    {
        var inquiry = await _db.Inquiries.FirstOrDefaultAsync(i => i.InquiryId == id);
        if (inquiry == null) return NotFound();

        if (inquiry.ClientId != null)
        {
            // Already linked to a client - nothing to do, just reload.
            Inquiry = (await LoadInquiryAsync(id))!;
            Employees = await _db.Employees.OrderBy(e => e.FullName).ToListAsync();
            return Page();
        }

        if (string.IsNullOrEmpty(inquiry.GuestEmail))
        {
            SuccessMessage = "Can't create an account - this inquiry has no email address on file.";
            Inquiry = (await LoadInquiryAsync(id))!;
            Employees = await _db.Employees.OrderBy(e => e.FullName).ToListAsync();
            return Page();
        }

        // If someone with this email already registered separately, link to
        // that existing account instead of creating a duplicate.
        var existingClient = await _db.Clients.FirstOrDefaultAsync(c => c.Email == inquiry.GuestEmail);

        if (existingClient != null)
        {
            inquiry.ClientId = existingClient.ClientId;
            await _db.SaveChangesAsync();
            SuccessMessage = $"Linked to the existing account for {existingClient.Email}.";
        }
        else
        {
            string password = "Welcome" + Random.Shared.Next(1000, 9999) + "!";

            var newClient = new Client
            {
                FullName = inquiry.GuestName ?? "New Client",
                Email = inquiry.GuestEmail,
                Phone = inquiry.GuestPhone,
                PasswordHash = PasswordHasher.HashPassword(password)
            };

            _db.Clients.Add(newClient);
            await _db.SaveChangesAsync(); // saves newClient first, so it gets a ClientId

            inquiry.ClientId = newClient.ClientId;
            await _db.SaveChangesAsync();

            GeneratedPassword = password;
            SuccessMessage = $"Client account created for {newClient.Email}.";
        }

        Inquiry = (await LoadInquiryAsync(id))!;
        Employees = await _db.Employees.OrderBy(e => e.FullName).ToListAsync();
        return Page();
    }

    private async Task<Inquiry?> LoadInquiryAsync(int id)
    {
        return await _db.Inquiries
            .Include(i => i.Client)
            .FirstOrDefaultAsync(i => i.InquiryId == id);
    }
}