using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MurendeniConstructionArchitects.Data;

namespace MurendeniConstructionArchitects.Pages.Admin;

// A tiny class just to combine different types of recent events into one
// list ("a new client registered", "a design file was uploaded", etc.)
// so the dashboard can show a single, mixed timeline.
public class ActivityItem
{
    public string Description { get; set; } = string.Empty;
    public DateTime When { get; set; }
}

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public IndexModel(ApplicationDbContext db)
    {
        _db = db;
    }

    public int ClientCount { get; set; }
    public int ProjectCount { get; set; }
    public int InquiryCount { get; set; }
    public int FileCount { get; set; }
    public List<ActivityItem> RecentActivity { get; set; } = new();

    public string CurrentRole { get; set; } = string.Empty;

    public async Task OnGetAsync()
    {
        CurrentRole = User.FindFirst("role")?.Value ?? "";

        if (CurrentRole == "Designer")
        {
            // Designers only see numbers about their own work.
            int employeeId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

            ProjectCount = await _db.Profiles.CountAsync(p => p.EmployeeId == employeeId);
            FileCount = await _db.Designs.CountAsync(d => d.EmployeeId == employeeId);
            ClientCount = 0;   // not relevant to a designer's view
            InquiryCount = 0;

            RecentActivity = await _db.Designs
                .Where(d => d.EmployeeId == employeeId)
                .OrderByDescending(d => d.CreatedAt)
                .Take(6)
                .Select(d => new ActivityItem { Description = $"You uploaded: {d.DesignTitle}", When = d.CreatedAt })
                .ToListAsync();

            return;
        }

        // Admin and Sales see the full picture (unchanged from before).
        ClientCount = await _db.Clients.CountAsync();
        ProjectCount = await _db.Profiles.CountAsync();
        InquiryCount = await _db.Inquiries.CountAsync();
        FileCount = await _db.Designs.CountAsync();

        var recentClients = await _db.Clients
            .OrderByDescending(c => c.CreatedAt).Take(3)
            .Select(c => new ActivityItem { Description = $"New client registered: {c.FullName}", When = c.CreatedAt })
            .ToListAsync();

        var recentProjects = await _db.Profiles
            .OrderByDescending(p => p.CreatedAt).Take(3)
            .Select(p => new ActivityItem { Description = $"New project created: {p.ProfileName}", When = p.CreatedAt })
            .ToListAsync();

        var recentFiles = await _db.Designs
            .OrderByDescending(d => d.CreatedAt).Take(3)
            .Select(d => new ActivityItem { Description = $"Design file uploaded: {d.DesignTitle}", When = d.CreatedAt })
            .ToListAsync();

        var recentInquiries = await _db.Inquiries
            .OrderByDescending(i => i.CreatedAt).Take(3)
            .Select(i => new ActivityItem { Description = $"New inquiry received: {i.Subject}", When = i.CreatedAt })
            .ToListAsync();

        RecentActivity = recentClients.Concat(recentProjects).Concat(recentFiles).Concat(recentInquiries)
            .OrderByDescending(a => a.When).Take(6).ToList();
    }
}