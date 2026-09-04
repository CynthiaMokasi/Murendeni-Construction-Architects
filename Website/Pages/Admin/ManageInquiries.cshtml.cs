using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MurendeniConstructionArchitects.Data;
using MurendeniConstructionArchitects.Models;

namespace MurendeniConstructionArchitects.Pages.Admin;

public class ManageInquiriesModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public ManageInquiriesModel(ApplicationDbContext db)
    {
        _db = db;
    }

    [BindProperty(SupportsGet = true)]
    public string StatusFilter { get; set; } = "All";

    public List<Inquiry> Inquiries { get; set; } = new();

    public async Task OnGetAsync()
    {
        // Include Client so registered clients show their name; guest
        // inquiries fall back to GuestName in the page itself.
        var query = _db.Inquiries.Include(i => i.Client).AsQueryable();

        if (StatusFilter != "All" && Enum.TryParse<InquiryStatus>(StatusFilter, out var status))
        {
            query = query.Where(i => i.Status == status);
        }

        Inquiries = await query
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();
    }
}