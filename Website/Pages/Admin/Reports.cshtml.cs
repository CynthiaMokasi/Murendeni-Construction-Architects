using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MurendeniConstructionArchitects.Data;
using MurendeniConstructionArchitects.Models;
using System.Text;

namespace MurendeniConstructionArchitects.Pages.Admin;

// One flexible row shape the report table can display, regardless of
// which report type was picked - keeps the Razor markup simple (one
// table, not three different ones).
public class ReportRow
{
    public string ColumnA { get; set; } = string.Empty;
    public string ColumnB { get; set; } = string.Empty;
    public string ColumnC { get; set; } = string.Empty;
    public string ColumnD { get; set; } = string.Empty;
}

public class ReportsModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public ReportsModel(ApplicationDbContext db)
    {
        _db = db;
    }

    [BindProperty(SupportsGet = true)]
    public string ReportType { get; set; } = "Projects";

    [BindProperty(SupportsGet = true)]
    public DateTime FromDate { get; set; } = DateTime.UtcNow.AddMonths(-1);

    [BindProperty(SupportsGet = true)]
    public DateTime ToDate { get; set; } = DateTime.UtcNow;

    // Summary cards at the top - always show overall totals,
    // not affected by the date range below.
    public int TotalProjects { get; set; }
    public int ActiveProjects { get; set; }
    public int CompletedProjects { get; set; }
    public int TotalClients { get; set; }

    public List<string> ColumnHeaders { get; set; } = new();
    public List<ReportRow> Rows { get; set; } = new();

    public async Task OnGetAsync()
    {
        TotalProjects = await _db.Profiles.CountAsync();
        ActiveProjects = await _db.Profiles.CountAsync(p =>
            p.Status == ProjectStatus.InProgress || p.Status == ProjectStatus.Review);
        CompletedProjects = await _db.Profiles.CountAsync(p => p.Status == ProjectStatus.Completed);
        TotalClients = await _db.Clients.CountAsync();

        (ColumnHeaders, Rows) = await BuildReportAsync();
    }

    // Builds the CSV file for the "Export Excel" button - reuses the exact
    // same data as the on-screen table, so what you see is what you get.
    public async Task<IActionResult> OnGetExportCsvAsync()
    {
        var (headers, rows) = await BuildReportAsync();

        var csv = new StringBuilder();
        csv.AppendLine(string.Join(",", headers.Select(EscapeCsvField)));

        foreach (var row in rows)
        {
            csv.AppendLine(string.Join(",", new[]
            {
                EscapeCsvField(row.ColumnA),
                EscapeCsvField(row.ColumnB),
                EscapeCsvField(row.ColumnC),
                EscapeCsvField(row.ColumnD)
            }));
        }

        byte[] bytes = Encoding.UTF8.GetBytes(csv.ToString());
        string fileName = $"{ReportType}Report_{DateTime.UtcNow:yyyyMMdd}.csv";
        return File(bytes, "text/csv", fileName);
    }

    private static string EscapeCsvField(string field)
    {
        // If the value contains a comma or quote, wrap it in quotes so
        // Excel doesn't misread it as extra columns.
        if (field.Contains(',') || field.Contains('"'))
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }
        return field;
    }

    private async Task<(List<string> headers, List<ReportRow> rows)> BuildReportAsync()
    {
        // ToDate is a date with no time, so add a day to make the range
        // inclusive of everything that happened ON that day too.
        var rangeEnd = ToDate.Date.AddDays(1);

        if (ReportType == "Clients")
        {
            var clients = await _db.Clients
                .Where(c => c.CreatedAt >= FromDate.Date && c.CreatedAt < rangeEnd)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            var rows = clients.Select(c => new ReportRow
            {
                ColumnA = c.FullName,
                ColumnB = c.Email,
                ColumnC = c.Phone ?? "-",
                ColumnD = c.CreatedAt.ToString("dd MMM yyyy")
            }).ToList();

            return (new List<string> { "Name", "Email", "Phone", "Registered" }, rows);
        }

        if (ReportType == "Inquiries")
        {
            var inquiries = await _db.Inquiries
                .Include(i => i.Client)
                .Where(i => i.CreatedAt >= FromDate.Date && i.CreatedAt < rangeEnd)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();

            var rows = inquiries.Select(i => new ReportRow
            {
                ColumnA = i.Client?.FullName ?? i.GuestName ?? "Unknown",
                ColumnB = i.ServiceRequested ?? "-",
                ColumnC = i.Status.ToString(),
                ColumnD = i.CreatedAt.ToString("dd MMM yyyy")
            }).ToList();

            return (new List<string> { "From", "Service", "Status", "Date" }, rows);
        }

        // Default: Projects
        var projects = await _db.Profiles
            .Include(p => p.Client)
            .Where(p => p.CreatedAt >= FromDate.Date && p.CreatedAt < rangeEnd)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        var projectRows = projects.Select(p => new ReportRow
        {
            ColumnA = p.ProfileName,
            ColumnB = p.Client?.FullName ?? "-",
            ColumnC = p.Status.ToString(),
            ColumnD = p.CreatedAt.ToString("dd MMM yyyy")
        }).ToList();

        return (new List<string> { "Project", "Client", "Status", "Date" }, projectRows);
    }
}