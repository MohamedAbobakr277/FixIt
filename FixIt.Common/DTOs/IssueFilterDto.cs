using System.Collections.Generic;
using FixIt.Common.Enums;

namespace FixIt.Common.DTOs;

public class IssueFilterDto
{
    public List<IssueStatus>? Statuses { get; set; } = new();
    public List<IssueCategory>? Categories { get; set; } = new();
    
    public string? SortBy { get; set; } = "date";
    public string? SortOrder { get; set; } = "desc";
    
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
