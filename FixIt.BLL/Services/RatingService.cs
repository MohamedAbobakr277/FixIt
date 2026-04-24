using FixIt.BLL.DTOs;
using FixIt.BLL.Interfaces;
using FixIt.BLL.Mapping;
using FixIt.Common.Enums;
using FixIt.DAL.Entities;
using FixIt.DAL.UnitOfWork;
using AutoMapper;

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
        var issue = await _unitOfWork.Issues.GetByIdAsync(dto.IssueId);

        if (issue == null)
        {
            throw new InvalidOperationException("Issue not found.");
        }

        if (issue.CitizenId != citizenId)
        {
            throw new UnauthorizedAccessException("You can only rate your own issues.");
        }

        if (issue.Status != IssueStatus.Resolved)
        {
            throw new InvalidOperationException("Only resolved issues can be rated.");
        }

        // Check if rating already exists
        var existingRating =await _unitOfWork.Ratings.GetAll()
                                      .FirstOrDefaultAsync(r => r.IssueId == dto.IssueId);
                                      
        if (existingRating != null)
        {
            throw new InvalidOperationException("This issue has already been rated.");
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
        issue.Status = IssueStatus.Closed;
        _unitOfWork.Issues.Update(issue);

        await _unitOfWork.CompleteAsync();
    }

    public async Task<RatingDto?> GetRatingByIssueIdAsync(int issueId)
    {
        var rating = _unitOfWork.Ratings.GetAll().FirstOrDefault(r => r.IssueId == issueId);
        
        if (rating == null) return null;

        return _mapper.Map<RatingDto>(rating);
    }
}
