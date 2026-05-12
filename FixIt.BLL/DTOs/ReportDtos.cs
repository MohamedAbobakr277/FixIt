using Microsoft.AspNetCore.Http;

namespace FixIt.BLL.DTOs;

public class ReportsPageDto
{
    public List<InProgressIssueDto> InProgressIssues { get; set; } = new();
}

public class InProgressIssueDto
{
    public int IssueId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string? WorkerName { get; set; }
    public string Category { get; set; } = string.Empty;
}

public class CreateReportDto
{
    public int IssueId { get; set; }
    public string Summary { get; set; } = string.Empty;
    public string? WorkerNotes { get; set; }
    public IFormFile? BeforeImage { get; set; }
    public IFormFile? AfterImage { get; set; }
}
