using FixIt.BLL.DTOs;
using FixIt.BLL.Interfaces;
using FixIt.BLL.Mapping;
using FixIt.Common.Enums;
using FixIt.DAL.Entities;
using FixIt.DAL.UnitOfWork;
using AutoMapper;
using Microsoft.EntityFrameworkCore;


namespace FixIt.BLL.Services;

public class RatingService : IRatingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public RatingService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task CreateRatingAsync(CreateRatingDto dto, string citizenId)
    {
        var (canRate, errorMessage) = await CanRateIssueAsync(dto.IssueId, citizenId);
        if (!canRate)
        {
            if (errorMessage == "You can only rate your own issues.")
                throw new UnauthorizedAccessException(errorMessage);
            
            throw new InvalidOperationException(errorMessage);
        }

        var rating = new Rating
        {
            IssueId = dto.IssueId,
            CitizenId = citizenId,
            Stars = dto.Stars,
            Comment = dto.Comment,
            SubmittedAt = DateTime.UtcNow
        };

        await _unitOfWork.Ratings.AddAsync(rating);

        // Update issue status to Closed
        var issue = await _unitOfWork.Issues.GetByIdAsync(dto.IssueId);
        if (issue != null)
        {
            issue.Status = IssueStatus.Closed;
            _unitOfWork.Issues.Update(issue);
        }

        await _unitOfWork.CompleteAsync();
    }

    public async Task<RatingDto?> GetRatingByIssueIdAsync(int issueId)
    {
        var rating = await _unitOfWork.Ratings.GetAll().FirstOrDefaultAsync(r => r.IssueId == issueId);
        
        if (rating == null) return null;

        return _mapper.Map<RatingDto>(rating);
    }

    public async Task<(bool CanRate, string? ErrorMessage)> CanRateIssueAsync(int issueId, string citizenId)
    {
        var issue = await _unitOfWork.Issues.GetByIdAsync(issueId);

        if (issue == null)
        {
            return (false, "Issue not found.");
        }

        if (issue.CitizenId != citizenId)
        {
            return (false, "You can only rate your own issues.");
        }

        if (issue.Status != IssueStatus.Resolved)
        {
            if (issue.Status == IssueStatus.Closed)
                return (false, "This issue has already been rated and closed.");
                
            return (false, "Only resolved issues can be rated.");
        }

        // Check if rating already exists (redundant if status is Closed, but safe)
        var existingRating = await _unitOfWork.Ratings.GetAll()
                                      .FirstOrDefaultAsync(r => r.IssueId == issueId);
                                      
        if (existingRating != null)
        {
            return (false, "This issue has already been rated.");
        }

        return (true, null);
    }
}
