namespace FixIt.Common.DTOs;

public class ScheduleDto
{
    public DateTime VisitDate { get; set; }
    public decimal EstimatedCost { get; set; }
    public string? WorkerName { get; set; }
}
