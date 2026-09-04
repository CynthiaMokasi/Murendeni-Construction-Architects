namespace MurendeniConstructionArchitects.Models;

public enum PaymentStatus
{
    Pending,
    Paid,
    Failed,
    Refunded
}

public class Payment
{
    public int PaymentId { get; set; }

    public int ClientId { get; set; }
    public Client? Client { get; set; }

    public int ProfileId { get; set; }
    public ProjectProfile? Profile { get; set; }

    public int DesignId { get; set; }
    public Design? Design { get; set; }

    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; }
    public DateTime? PaidAt { get; set; }
}
