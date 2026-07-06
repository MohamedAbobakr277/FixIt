using FixIt.BLL.DTOs;

namespace FixIt.BLL.Interfaces;

public interface IPaymentService
{
    /// <summary>
    /// Creates a Stripe Checkout Session for a resolved issue and returns the session URL.
    /// </summary>
    Task<string> CreateCheckoutSessionAsync(int issueId, string citizenId);

    /// <summary>
    /// Processes an incoming Stripe webhook event (validates signature and updates payment status).
    /// </summary>
    Task<bool> HandleWebhookAsync(string json, string stripeSignature);

    /// <summary>
    /// Returns payment details for a specific issue, or null if no payment exists.
    /// </summary>
    Task<PaymentDto?> GetPaymentByIssueAsync(int issueId);

    /// <summary>
    /// Returns the payment associated with a Stripe Session ID.
    /// </summary>
    Task<PaymentDto?> GetPaymentBySessionIdAsync(string sessionId);
}
