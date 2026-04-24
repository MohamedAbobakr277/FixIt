using System;

namespace FixIt.BLL.DTOs;

public class ReportDto
{
    public string Summary { get; set; } = string.Empty;
    public string? WorkerNotes { get; set; }
    public string? BeforeImageUrl { get; set; }
    public string? AfterImageUrl { get; set; }
    public DateTime SubmittedAt { get; set; }
}
