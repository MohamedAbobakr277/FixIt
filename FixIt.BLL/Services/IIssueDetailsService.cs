using FixIt.Common.DTOs;

namespace FixIt.BLL.Services;

public interface IIssueDetailsService
{
    Task<IssueDetailsDto?> GetIssueDetailsAsync(int issueId);
}
