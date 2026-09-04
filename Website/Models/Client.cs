using System.ComponentModel.DataAnnotations;

namespace MurendeniConstructionArchitects.Models;

public class Client
{
    public int ClientId { get; set; }

    [Required, MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required, MaxLength(255), EmailAddress]
    public string Email { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Phone { get; set; }

    // Custom auth: store a salted hash, never the raw password.
    public string? PasswordHash { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Inquiry> Inquiries { get; set; } = new List<Inquiry>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
