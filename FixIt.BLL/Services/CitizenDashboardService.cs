using AutoMapper;
using FixIt.BLL.DTOs;
using FixIt.BLL.Interfaces;
using FixIt.Common.DTOs;
using FixIt.Common.Enums;
using FixIt.Common.Constants;
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
    private readonly Microsoft.AspNetCore.Hosting.IWebHostEnvironment _environment;

    public CitizenDashboardService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        UserManager<ApplicationUser> userManager,
        Microsoft.AspNetCore.Hosting.IWebHostEnvironment environment)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _userManager = userManager;
        _environment = environment;
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
            IsTwoFactorEnabled = citizen.TwoFactorEnabled,
            PhoneNumber = citizen.PhoneNumber,
            Address = citizen.Address,
            MemberSince = citizen.CreatedAt,
            ProfilePicture = citizen.ProfileImageUrl,
            IssuesReported = citizen.Issues.Count,
            ResolvedIssues = citizen.Issues.Count(i => i.Status == IssueStatus.Resolved || i.Status == IssueStatus.Closed),
            RatingsGiven = citizen.Ratings.Count,
            IssueHistory = _mapper.Map<List<IssueListDto>>(citizen.Issues.OrderByDescending(i => i.SubmittedAt).Take(10).ToList()),
            EmailIssueUpdates = citizen.EmailIssueUpdates,
            EmailMaintenanceAlerts = citizen.EmailMaintenanceAlerts,
            EmailWeeklyReports = citizen.EmailWeeklyReports,
            AppRealTimePush = citizen.AppRealTimePush,
            AppDirectMessages = citizen.AppDirectMessages
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

    public async Task<bool> UpdateProfileAsync(string citizenId, FixIt.BLL.DTOs.UpdateProfileDto dto)
    {
        var user = await _userManager.FindByIdAsync(citizenId) as Citizen;
        if (user == null) return false;

        user.FullName = dto.FullName.Trim();
        user.Email = dto.Email.Trim().ToLower();
        user.UserName = dto.Email.Trim().ToLower(); // Important for login
        user.PhoneNumber = dto.PhoneNumber?.Trim();
        user.Address = dto.Address?.Trim();

        if (dto.ProfilePicture != null)
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, AppConstants.UploadsProfilesPath);
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            var fileExtension = Path.GetExtension(dto.ProfilePicture.FileName).ToLowerInvariant();
            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await dto.ProfilePicture.CopyToAsync(fileStream);
            }

            user.ProfileImageUrl = $"/{AppConstants.UploadsProfilesPath}/{uniqueFileName}";
        }

        var result = await _userManager.UpdateAsync(user);
        
        if (result.Succeeded)
        {
            return true;
        }

        return false;
    }

    public async Task<bool> UpdateNotificationsAsync(string citizenId, UpdateNotificationsDto dto)
    {
        var user = await _userManager.FindByIdAsync(citizenId) as Citizen;
        if (user == null) return false;

        user.EmailIssueUpdates = dto.EmailIssueUpdates;
        user.EmailMaintenanceAlerts = dto.EmailMaintenanceAlerts;
        user.EmailWeeklyReports = dto.EmailWeeklyReports;
        user.AppRealTimePush = dto.AppRealTimePush;
        user.AppDirectMessages = dto.AppDirectMessages;

        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }
}
