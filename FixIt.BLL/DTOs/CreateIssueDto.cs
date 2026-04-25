using FixIt.Common.DTOs;
using Microsoft.AspNetCore.Http;

namespace FixIt.BLL.DTOs;

public class CreateIssueDto : FixIt.Common.DTOs.CreateIssueDto
{
    public IFormFile? Image { get; set; }
}
