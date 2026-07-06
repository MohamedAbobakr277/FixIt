using FixIt.Common.Enums;

namespace FixIt.BLL.DTOs;

public class PaymentDto
{
    public int PaymentId { get; set; }
    public int IssueId { get; set; }
    public string CitizenId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "EGP";
    public string StripeSessionId { get; set; } = string.Empty;
    public string StripePaymentIntentId { get; set; } = string.Empty;
    public PaymentStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
