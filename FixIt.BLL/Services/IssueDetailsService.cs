using AutoMapper;
using FixIt.BLL.Interfaces;
using FixIt.Common.DTOs;
using FixIt.DAL.Entities;
using FixIt.DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace FixIt.BLL.Services;

public class IssueDetailsService : IIssueDetailsService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly INotificationService _notificationService;

    public IssueDetailsService(IUnitOfWork unitOfWork, IMapper mapper, INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _notificationService = notificationService;
    }

    public async Task<IssueDetailsDto?> GetIssueDetailsAsync(int issueId)
    {
        var issue = await _unitOfWork.Issues.GetAll()
            .Include(i => i.Citizen)
            .Include(i => i.MaintenanceSchedule)
            .Include(i => i.MaintenanceReport)
            .Include(i => i.Rating)
            .Include(i => i.Comments).ThenInclude(c => c.User)
            .FirstOrDefaultAsync(i => i.IssueId == issueId);

        if (issue == null)
            return null;

        var dto = _mapper.Map<IssueDetailsDto>(issue);

        var payment = await _unitOfWork.Payments.GetAll()
            .FirstOrDefaultAsync(p => p.IssueId == issueId);

        if (payment != null)
        {
            dto.IsPaid = payment.Status == FixIt.Common.Enums.PaymentStatus.Completed;
            dto.PaymentStatus = payment.Status.ToString();
        }

        return dto;
    }

    public async Task<IssueCommentDto> AddCommentAsync(int issueId, string userId, string text)
    {
        var issue = await _unitOfWork.Issues.GetAll()
            .Include(i => i.Comments)
            .FirstOrDefaultAsync(i => i.IssueId == issueId);
            
        if (issue == null) throw new ArgumentException("Issue not found");

        var comment = new IssueComment
        {
            IssueId = issueId,
            UserId = userId,
            Text = text,
            CreatedAt = DateTime.UtcNow
        };

        issue.Comments.Add(comment);
        _unitOfWork.Issues.Update(issue);
        await _unitOfWork.CompleteAsync();

        var createdComment = await _unitOfWork.Issues.GetAll()
            .Where(i => i.IssueId == issueId)
            .SelectMany(i => i.Comments)
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == comment.Id);

        if (createdComment?.User is Citizen)
        {
            await _notificationService.CreateAdminNotificationAsync(
                AdminNotificationType.CitizenCommunication,
                "💬 New Citizen Comment",
                $"Citizen {createdComment.User.FullName} commented on issue '{issue.Title}'.",
                issueId.ToString(),
                $"/Issue/AdminDetails?id={issueId}"
            );
        }
        else if (userId != issue.CitizenId && !string.IsNullOrEmpty(issue.CitizenId))
        {
            // Notify the Citizen that an Admin/Worker responded
            await _notificationService.SendNotificationAsync(
                issue.CitizenId,
                NotificationType.Push,
                "💬 New Response to your Issue",
                $"An admin or technician has replied to your issue: '{issue.Title}'.",
                $"/Issue/Details?id={issueId}"
            );
        }

        return _mapper.Map<IssueCommentDto>(createdComment);
    }
}
