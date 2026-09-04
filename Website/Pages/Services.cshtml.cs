using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MurendeniConstructionArchitects.Pages;

// A tiny class just to hold the text for one service card.
// We're not saving services in the database (yet), so this is
// simple C# data instead of an EF Core entity.
public class ServiceItem
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class ServicesModel : PageModel
{
    public List<ServiceItem> Services { get; } = new()
    {
        new ServiceItem
        {
            Title = "Architectural Drafting",
            Description = "Professional architectural drafting services for new builds, additions and renovations, prepared to municipal submission standard."
        },
        new ServiceItem
        {
            Title = "2D Floor Plans",
            Description = "Accurate, easy-to-read 2D floor plans for your project, from concept layouts through to construction drawings."
        },
        new ServiceItem
        {
            Title = "3D Building Models",
            Description = "Realistic 3D models and walkthroughs that help you and your builder see the finished space before construction starts."
        },
        new ServiceItem
        {
            Title = "Design Services",
            Description = "End-to-end design support - from the first concept sketch to a full set of construction-ready documents."
        }
    };

    public void OnGet()
    {
    }
}
