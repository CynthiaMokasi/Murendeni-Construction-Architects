using System.ComponentModel.DataAnnotations;

namespace MurendeniConstructionArchitects.Models;

public class Design
{
    public int DesignId { get; set; }

    public int ProfileId { get; set; }
    public ProjectProfile? Profile { get; set; }

    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    [Required, MaxLength(150)]
    public string DesignTitle { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? FileUrl { get; set; }

    public bool IsLocked { get; set; }

    // Allowed values enforced by CK_Design_Status: draft, review, approved, archived
    [MaxLength(20)]
    public string Status { get; set; } = "draft";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
