using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MurendeniConstructionArchitects.Models;

public enum PropertyType
{
    Residential,
    Commercial
}

public enum ProjectCategory
{
    FloorPlans,
    ThreeDModels,
    Elevations,
    SitePlans
}

/// <summary>
/// Maps to the "Profile" table in the database. In the UI this is what
/// the client and public visitor think of as a "Project" — the thing
/// shown in the Portfolio and tracked in the Client Portal.
/// </summary>
public enum ProjectStatus
{
    Inquiry,
    InProgress,
    Review,
    Completed
}
[Table("Profile")]
public class ProjectProfile
{
    [Column("status")]
    public ProjectStatus Status { get; set; } = ProjectStatus.Inquiry;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    [Column("profile_id")]
    public int ProfileId { get; set; }

    [Column("client_id")]
    public int ClientId { get; set; }
    public Client? Client { get; set; }

    [Column("employee_id")]
    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    [Required, MaxLength(150), Column("profile_name")]
    public string ProfileName { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [Column("is_published")]
    public bool IsPublished { get; set; }

    [Column("is_featured")]
    public bool IsFeatured { get; set; } = false;

    [Column("project_category")]
    public ProjectCategory? Category { get; set; }

    [Column("property_type")]
    public PropertyType PropertyType { get; set; } = PropertyType.Residential;

    [MaxLength(150), Column("location")]
    public string? Location { get; set; }

    [Column("year_completed")]
    public int? YearCompleted { get; set; }

    [MaxLength(500), Column("cover_image_url")]
    public string? CoverImageUrl { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Design> Designs { get; set; } = new List<Design>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
