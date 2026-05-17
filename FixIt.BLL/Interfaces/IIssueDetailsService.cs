using FixIt.Common.DTOs;

namespace FixIt.BLL.Interfaces;

public interface IIssueDetailsService
{
    Task<IssueDetailsDto?> GetIssueDetailsAsync(int issueId);
}
