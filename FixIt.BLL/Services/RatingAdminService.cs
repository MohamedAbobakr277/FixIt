using FixIt.BLL.Interfaces;
using FixIt.Common.DTOs;
using FixIt.DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FixIt.BLL.Services;

public class RatingAdminService : IRatingAdminService
{
    private readonly IUnitOfWork _unitOfWork;

    public RatingAdminService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<AdminRatingsPageDto> GetRatingsPageDataAsync()
    {
        var allRatings = await _unitOfWork.Ratings.GetAll()
            .Include(r => r.Issue)
            .Include(r => r.Citizen)
            .OrderByDescending(r => r.SubmittedAt)
            .ToListAsync();

        var dto = new AdminRatingsPageDto
        {
            TotalRatings = allRatings.Count,
            AverageRating = allRatings.Any() ? Math.Round(allRatings.Average(r => r.Stars), 1) : 0,
            PositiveRatings = allRatings.Count(r => r.Stars >= 4),
            Distribution = new Dictionary<int, int> { { 5, 0 }, { 4, 0 }, { 3, 0 }, { 2, 0 }, { 1, 0 } },
            RecentRatings = allRatings.Take(10).Select(r => new AdminRatingItemDto
            {
                Stars = r.Stars,
                Comment = r.Comment,
                SubmittedAt = r.SubmittedAt,
                CitizenName = r.Citizen?.FullName ?? "Unknown",
                IssueTitle = r.Issue?.Title ?? "Unknown"
            }).ToList()
        };

        foreach (var r in allRatings)
        {
            if (dto.Distribution.ContainsKey(r.Stars))
                dto.Distribution[r.Stars]++;
        }

        return dto;
    }
}
