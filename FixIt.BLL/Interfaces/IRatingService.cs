using FixIt.Common.DTOs;
using FixIt.BLL.Mapping;

namespace FixIt.BLL.Interfaces;

public interface IRatingService
{
    Task CreateRatingAsync(CreateRatingDto dto, string citizenId);
    Task<RatingDto?> GetRatingByIssueIdAsync(int issueId);
    Task<(bool CanRate, string? ErrorMessage)> CanRateIssueAsync(int issueId, string citizenId);
}
