using System;
using System.Collections.Generic;

namespace FixIt.Common.DTOs;

public class AdminRatingItemDto
{
    public int Stars { get; set; }
    public string? Comment { get; set; }
    public DateTime SubmittedAt { get; set; }
    public string CitizenName { get; set; } = string.Empty;
    public string IssueTitle { get; set; } = string.Empty;
}

public class AdminRatingsPageDto
{
    public double AverageRating { get; set; }
    public int TotalRatings { get; set; }
    public int PositiveRatings { get; set; }
    public Dictionary<int, int> Distribution { get; set; } = new();
    public List<AdminRatingItemDto> RecentRatings { get; set; } = new();
}
