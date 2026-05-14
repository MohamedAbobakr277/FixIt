using AutoMapper;
using FixIt.BLL.DTOs;
using FixIt.BLL.Interfaces;
using FixIt.Common.DTOs;
using FixIt.Common.Enums;
using FixIt.DAL.Entities;
using FixIt.DAL.UnitOfWork;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FixIt.BLL.Services;

public class CitizenDashboardService : ICitizenDashboardService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly UserManager<ApplicationUser> _userManager;

    public CitizenDashboardService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        UserManager<ApplicationUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _userManager = userManager;
    }

    public async Task<CitizenDashboardDto> GetDashboardDataAsync(string citizenId)
    {
        var citizen = await _userManager.FindByIdAsync(citizenId) as Citizen;
        if (citizen == null) throw new Exception("Citizen not found");

        var issues = await _unitOfWork.Issues
            .GetAll()
            .Where(i => i.CitizenId == citizenId)
            .OrderByDescending(i => i.SubmittedAt)
            .ToListAsync();

        var dashboard = new CitizenDashboardDto
        {
            FullName = citizen.FullName,
            TotalIssues = issues.Count,
            InProgressIssues = issues.Count(i => i.Status == IssueStatus.InProgress || i.Status == IssueStatus.Scheduled || i.Status == IssueStatus.Approved),
            ResolvedIssues = issues.Count(i => i.Status == IssueStatus.Resolved || i.Status == IssueStatus.Closed),
            NeedsRatingIssues = issues.Count(i => i.Status == IssueStatus.Resolved && i.Rating == null),
            RecentIssues = _mapper.Map<List<IssueListDto>>(issues.Take(5))
        };

        return dashboard;
    }

    public async Task<CitizenProfileDto> GetProfileDataAsync(string citizenId)
    {
        var citizen = await _userManager.Users
            .OfType<Citizen>()
            .Include(c => c.Issues)
            .Include(c => c.Ratings)
                .ThenInclude(r => r.Issue)
            .FirstOrDefaultAsync(c => c.Id == citizenId);

        if (citizen == null) throw new Exception("Citizen not found");

        var profile = new CitizenProfileDto
        {
            FullName = citizen.FullName,
            Email = citizen.Email ?? "",
            MemberSince = citizen.CreatedAt,
            ProfilePicture = citizen.ProfilePicture,
            IssuesReported = citizen.Issues.Count,
            ResolvedIssues = citizen.Issues.Count(i => i.Status == IssueStatus.Resolved || i.Status == IssueStatus.Closed),
            RatingsGiven = citizen.Ratings.Count,
            IssueHistory = _mapper.Map<List<IssueListDto>>(citizen.Issues.OrderByDescending(i => i.SubmittedAt).Take(10).ToList())
        };
        
        profile.RecentRatings = citizen.Ratings.OrderByDescending(r => r.SubmittedAt).Take(5).Select(r => new RatingDetailsDto
        {
            IssueId = r.IssueId,
            IssueTitle = r.Issue?.Title ?? "Unknown Issue",
            Stars = r.Stars,
            Comment = r.Comment,
            RatedAt = r.SubmittedAt
        }).ToList();

        return profile;
    }
}
