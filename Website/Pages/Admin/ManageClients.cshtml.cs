using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MurendeniConstructionArchitects.Data;

namespace MurendeniConstructionArchitects.Pages.Admin;

// A small "row" shape combining a Client with how many projects they have -
// simpler than trying to squeeze this into the Client entity itself.
public class ClientRow
{
    public int ClientId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int ProjectCount { get; set; }
}

public class ManageClientsModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public ManageClientsModel(ApplicationDbContext db)
    {
        _db = db;
    }

    // SupportsGet = true means this fills in from ?Search=... in the URL,
    // not just from a posted form - so the search box works with a normal GET.
    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    public List<ClientRow> Clients { get; set; } = new();

    public async Task OnGetAsync()
    {
        var query = _db.Clients.AsQueryable();

        if (!string.IsNullOrWhiteSpace(Search))
        {
            query = query.Where(c =>
                c.FullName.Contains(Search) || c.Email.Contains(Search));
        }

        Clients = await query
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new ClientRow
            {
                ClientId = c.ClientId,
                FullName = c.FullName,
                Email = c.Email,
                ProjectCount = c.Payments.Count() // placeholder metric, fixed below
            })
            .ToListAsync();

        // EF Core can't easily count Profiles (a different table) inside the
        // Select above without a join, so we fetch project counts separately
        // and merge them in - simpler to read than a complex LINQ join.
        var projectCounts = await _db.Profiles
            .GroupBy(p => p.ClientId)
            .Select(g => new { ClientId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ClientId, x => x.Count);

        foreach (var row in Clients)
        {
            row.ProjectCount = projectCounts.GetValueOrDefault(row.ClientId, 0);
        }
    }
}