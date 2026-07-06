using FixIt.Common.Enums;

namespace FixIt.DAL.Entities;

public class Payment
{
    public int PaymentId { get; set; }
    public int IssueId { get; set; }
    public string CitizenId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "EGP";
    public string StripeSessionId { get; set; } = string.Empty;
    public string StripePaymentIntentId { get; set; } = string.Empty;
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    // Navigation
    public Issue? Issue { get; set; }
    public Citizen? Citizen { get; set; }
}
