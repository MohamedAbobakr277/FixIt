namespace FixIt.BLL.DTOs;

public class SchedulePageDto
{
    public int ScheduledCount { get; set; }
    public int InProgressCount { get; set; }
    public List<ScheduleItemDto> InProgressItems { get; set; } = new();
    public List<ScheduleItemDto> UpcomingItems { get; set; } = new();
}

public class ScheduleItemDto
{
    public int IssueId { get; set; }
    public int ScheduleId { get; set; }
    public string IssueTitle { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime VisitDate { get; set; }
    public string? WorkerName { get; set; }
    public decimal EstimatedCost { get; set; }
}

public class CreateScheduleDto
{
    public int IssueId { get; set; }
    public DateTime VisitDate { get; set; }
    public string? WorkerName { get; set; }
    public decimal EstimatedCost { get; set; }
}
