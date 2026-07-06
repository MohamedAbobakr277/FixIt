using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FixIt.BLL.DTOs;
using FixIt.BLL.Interfaces;
using FixIt.Common.Enums;
using FixIt.DAL.Entities;
using FixIt.DAL.UnitOfWork;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Stripe;
using Stripe.Checkout;

namespace FixIt.BLL.Services;

public class StripePaymentService : IPaymentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public StripePaymentService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<string> CreateCheckoutSessionAsync(int issueId, string citizenId)
    {
        // 1. Verify issue exists, is resolved, and belongs to the citizen
        var issue = await _unitOfWork.Issues.GetAll()
            .Include(i => i.MaintenanceSchedule)
            .FirstOrDefaultAsync(i => i.IssueId == issueId && i.CitizenId == citizenId);

        if (issue == null)
        {
            throw new ArgumentException("Issue not found or unauthorized.");
        }

        if (issue.Status != IssueStatus.Resolved)
        {
            throw new InvalidOperationException("Payments can only be made for resolved issues.");
        }

        // 2. Check if a payment already exists for this issue
        var existingPayment = await _unitOfWork.Payments.GetAll()
            .FirstOrDefaultAsync(p => p.IssueId == issueId);

        if (existingPayment != null)
        {
            if (existingPayment.Status == PaymentStatus.Completed)
            {
                throw new InvalidOperationException("Payment has already been completed for this issue.");
            }
            // If pending, we can either reuse or update it. For Stripe, we can just create a new session
            // and update the existing payment with the new Session ID.
        }

        // 3. Define payment amount (use EstimatedCost if available and > 0, otherwise flat service fee of 50.00 EGP)
        decimal amount = 50.00m;
        if (issue.MaintenanceSchedule != null && issue.MaintenanceSchedule.EstimatedCost > 0)
        {
            amount = issue.MaintenanceSchedule.EstimatedCost;
        }
        string currency = "egp";

        // Get application base URL
        var request = _httpContextAccessor.HttpContext?.Request;
        var baseUrl = request != null ? $"{request.Scheme}://{request.Host}" : _configuration["Stripe:Domain"] ?? "https://localhost:7198";

        // 4. Create Stripe Checkout Session
        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(amount * 100), // Amount in cents
                        Currency = currency,
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = $"Service Fee for Issue #{issueId}",
                            Description = $"FixIt resolution fee for: {issue.Title}",
                        },
                    },
                    Quantity = 1,
                },
            },
            Mode = "payment",
            SuccessUrl = $"{baseUrl}/Payment/Success?session_id={{CHECKOUT_SESSION_ID}}",
            CancelUrl = $"{baseUrl}/Payment/Cancel?issueId={issueId}",
            Metadata = new Dictionary<string, string>
            {
                { "IssueId", issueId.ToString() },
                { "CitizenId", citizenId }
            }
        };

        var service = new SessionService();
        Session session = await service.CreateAsync(options);

        // 5. Save/Update Payment Record
        if (existingPayment != null)
        {
            existingPayment.StripeSessionId = session.Id;
            existingPayment.Amount = amount;
            existingPayment.Currency = currency.ToUpper();
            existingPayment.Status = PaymentStatus.Pending;
            existingPayment.CreatedAt = DateTime.UtcNow;
            _unitOfWork.Payments.Update(existingPayment);
        }
        else
        {
            var payment = new Payment
            {
                IssueId = issueId,
                CitizenId = citizenId,
                Amount = amount,
                Currency = currency.ToUpper(),
                StripeSessionId = session.Id,
                Status = PaymentStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.Payments.AddAsync(payment);
        }

        await _unitOfWork.CompleteAsync();

        return session.Url;
    }

    public async Task<bool> HandleWebhookAsync(string json, string stripeSignature)
    {
        var webhookSecret = _configuration["Stripe:WebhookSecret"];
        if (string.IsNullOrEmpty(webhookSecret))
        {
            throw new InvalidOperationException("Stripe Webhook Secret is not configured.");
        }

        try
        {
            var stripeEvent = EventUtility.ConstructEvent(json, stripeSignature, webhookSecret);

            if (stripeEvent.Type == "checkout.session.completed")
            {
                var session = stripeEvent.Data.Object as Session;
                if (session == null) return false;

                // Find the corresponding payment in DB
                var payment = await _unitOfWork.Payments.GetAll()
                    .FirstOrDefaultAsync(p => p.StripeSessionId == session.Id);

                if (payment != null && payment.Status != PaymentStatus.Completed)
                {
                    payment.Status = PaymentStatus.Completed;
                    payment.StripePaymentIntentId = session.PaymentIntentId;
                    payment.CompletedAt = DateTime.UtcNow;

                    _unitOfWork.Payments.Update(payment);
                    await _unitOfWork.CompleteAsync();

                    // Optional: Create a notification for the citizen
                    var notification = new Notification
                    {
                        UserId = payment.CitizenId,
                        Title = "Payment Successful",
                        Message = $"Payment of {payment.Amount} {payment.Currency} for Issue #{payment.IssueId} was successfully processed.",
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow,
                        RelatedEntityUrl = $"/Issue/Details/{payment.IssueId}",
                        Type = NotificationType.Push
                    };
                    await _unitOfWork.Notifications.AddAsync(notification);
                    await _unitOfWork.CompleteAsync();

                    return true;
                }
            }
            else if (stripeEvent.Type == "payment_intent.payment_failed")
            {
                var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                if (paymentIntent == null) return false;

                // Find payment by payment intent or session id if present, or search metadata
                // Actually checkout session is completed event is when checkout completes.
                // If it fails before that, we can search by StripePaymentIntentId or session
                var payment = await _unitOfWork.Payments.GetAll()
                    .FirstOrDefaultAsync(p => p.StripePaymentIntentId == paymentIntent.Id);

                if (payment != null)
                {
                    payment.Status = PaymentStatus.Failed;
                    _unitOfWork.Payments.Update(payment);
                    await _unitOfWork.CompleteAsync();
                    return true;
                }
            }

            return false;
        }
        catch (StripeException ex)
        {
            // Webhook signature verification failed or Stripe API error
            // In a real application, you would log this error
            Console.WriteLine($"Stripe webhook validation failed: {ex.Message}");
            return false;
        }
    }

    public async Task<PaymentDto?> GetPaymentByIssueAsync(int issueId)
    {
        var payment = await _unitOfWork.Payments.GetAll()
            .FirstOrDefaultAsync(p => p.IssueId == issueId);

        return payment == null ? null : _mapper.Map<PaymentDto>(payment);
    }

    public async Task<PaymentDto?> GetPaymentBySessionIdAsync(string sessionId)
    {
        var payment = await _unitOfWork.Payments.GetAll()
            .FirstOrDefaultAsync(p => p.StripeSessionId == sessionId);

        return payment == null ? null : _mapper.Map<PaymentDto>(payment);
    }
}
