using System.ComponentModel.DataAnnotations;

namespace MurendeniConstructionArchitects.Models;

public enum InquiryStatus
{
    Open,
    InProgress,
    Resolved,
    Closed
}

public class Inquiry
{
    public int InquiryId { get; set; }

    // Null until the visitor is (or becomes) a registered client.
    public int? ClientId { get; set; }
    public Client? Client { get; set; }

    // Null until an admin assigns someone to follow up.
    public int? EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    [Required, MaxLength(200)]
    public string Subject { get; set; } = string.Empty;

    public InquiryStatus Status { get; set; } = InquiryStatus.Open;

    // Guest contact details, used when ClientId is null (public contact form).
    [MaxLength(100)]
    public string? GuestName { get; set; }

    [MaxLength(255)]
    public string? GuestEmail { get; set; }

    [MaxLength(20)]
    public string? GuestPhone { get; set; }

    [MaxLength(100)]
    public string? ServiceRequested { get; set; }

    public string? Message { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
