using System.Threading.Tasks;
using FixIt.BLL.DTOs;
using FixIt.Common.DTOs;
using FixIt.Common.Pagination;

namespace FixIt.BLL.Interfaces;

public interface IIssueService
{
    Task<PaginatedList<IssueListDto>> GetCitizenIssuesAsync(string citizenId, IssueFilterDto filter);
    Task<IssueDetailsDto?> GetIssueByIdAsync(int issueId, string citizenId);
    Task<int> CreateAsync(CreateIssueDto dto, string citizenId);
}
