using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FixIt.BLL.DTOs;
using FixIt.Common.DTOs;
using FixIt.BLL.Interfaces;
using FixIt.Common.Constants;
using FixIt.Common.Pagination;
using FixIt.DAL.Entities;
using FixIt.DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using FixIt.Common.Enums;

namespace FixIt.BLL.Services;

public class IssueService : IIssueService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IWebHostEnvironment _environment;

    public IssueService(IUnitOfWork unitOfWork, IMapper mapper, IWebHostEnvironment environment)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _environment = environment;
    }

    public async Task<PaginatedList<IssueListDto>> GetCitizenIssuesAsync(string citizenId, IssueFilterDto filter)
    {
        var query = _unitOfWork.Issues.GetAll()
            .Where(i => i.CitizenId == citizenId);

        if (filter.Statuses != null && filter.Statuses.Any())
            query = query.Where(i => filter.Statuses.Contains(i.Status));
        
        if (filter.Categories != null && filter.Categories.Any())
            query = query.Where(i => filter.Categories.Contains(i.Category));

        query = filter.SortBy?.ToLower() switch
        {
            "date" => filter.SortOrder?.ToLower() == "desc"
                ? query.OrderByDescending(i => i.SubmittedAt)
                : query.OrderBy(i => i.SubmittedAt),
            "status" => filter.SortOrder?.ToLower() == "desc"
                ? query.OrderByDescending(i => i.Status)
                : query.OrderBy(i => i.Status),
            _ => query.OrderByDescending(i => i.SubmittedAt)
        };

        var mappedQuery = query.ProjectTo<IssueListDto>(_mapper.ConfigurationProvider);

        return await PaginatedList<IssueListDto>.CreateAsync(mappedQuery, filter.PageIndex, filter.PageSize);
    }

    public async Task<IssueDetailsDto?> GetIssueByIdAsync(int issueId, string citizenId)
    {
        var issue = await _unitOfWork.Issues.GetAll()
            .Where(i => i.IssueId == issueId && i.CitizenId == citizenId)
            .Include(i => i.Citizen)
            .Include(i => i.MaintenanceSchedule)
            .Include(i => i.MaintenanceReport)
            .Include(i => i.Rating)
            .FirstOrDefaultAsync();

        return issue == null ? null : _mapper.Map<IssueDetailsDto>(issue);
    }

    public async Task<int> CreateAsync(FixIt.BLL.DTOs.CreateIssueDto dto, string citizenId)
    {
        var issue = _mapper.Map<Issue>(dto);
        issue.CitizenId = citizenId;
        issue.SubmittedAt = DateTime.UtcNow;
        issue.UpdatedAt = DateTime.UtcNow;

        if (dto.Image != null)
        {
            issue.ImageUrl = await SaveImageAsync(dto.Image);
        }

        await _unitOfWork.Issues.AddAsync(issue);
        
        // Add status history entry
        issue.StatusHistory.Add(new IssueStatusHistory
        {
            Status = IssueStatus.New,
            ChangedAt = DateTime.UtcNow,
            Note = "Issue created by citizen."
        });

        await _unitOfWork.CompleteAsync();

        return issue.IssueId;
    }

    private async Task<string> SaveImageAsync(IFormFile image)
    {
        var uploadsFolder = Path.Combine(_environment.WebRootPath, AppConstants.UploadsIssuesPath);
        
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var fileExtension = Path.GetExtension(image.FileName).ToLowerInvariant();
        if (!AppConstants.AllowedImageExtensions.Contains(fileExtension))
        {
            throw new InvalidOperationException("Invalid image format. Allowed formats: jpg, jpeg, png, webp");
        }

        var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await image.CopyToAsync(stream);
        }

        return $"/{AppConstants.UploadsIssuesPath}/{uniqueFileName}";
    }
}
