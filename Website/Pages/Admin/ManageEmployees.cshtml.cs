using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MurendeniConstructionArchitects.Data;
using MurendeniConstructionArchitects.Models;

namespace MurendeniConstructionArchitects.Pages.Admin;

public class ManageEmployeesModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public ManageEmployeesModel(ApplicationDbContext db)
    {
        _db = db;
    }

    public List<Employee> Employees { get; set; } = new();

    public async Task OnGetAsync()
    {
        Employees = await _db.Employees.OrderBy(e => e.FullName).ToListAsync();
    }
}