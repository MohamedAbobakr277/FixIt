using FixIt.Common.Enums;
using Microsoft.AspNetCore.Http;

namespace FixIt.BLL.DTOs;

public class CreateIssueDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public IssueCategory Category { get; set; }
    public IFormFile? Image { get; set; }
}
