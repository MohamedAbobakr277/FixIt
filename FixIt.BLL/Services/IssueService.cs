using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FixIt.BLL.DTOs;
using FixIt.BLL.Interfaces;
using FixIt.Common.Pagination;
using FixIt.DAL.Entities;
using FixIt.DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using AutoMapper.QueryableExtensions;

namespace FixIt.BLL.Services;

public class IssueService : IIssueService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public IssueService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginatedList<IssueListDto>> GetCitizenIssuesAsync(string citizenId, IssueFilterDto filter)
    {
        var query = _unitOfWork.Issues.GetAll()
            .Where(i => i.CitizenId == citizenId);

        if (filter.Statuses != null && filter.Statuses.Any())
            query = query.Where(i => filter.Statuses.Contains(i.Status));
        
        if (filter.Categories != null && filter.Categories.Any())
            query = query.Where(i => filter.Categories.Contains(i.Category));

        query = filter.SortBy?.ToLower() switch
        {
            "date" => filter.SortOrder?.ToLower() == "desc"
                ? query.OrderByDescending(i => i.SubmittedAt)
                : query.OrderBy(i => i.SubmittedAt),
            "status" => filter.SortOrder?.ToLower() == "desc"
                ? query.OrderByDescending(i => i.Status)
                : query.OrderBy(i => i.Status),
            _ => query.OrderByDescending(i => i.SubmittedAt)
        };

        var mappedQuery = query.ProjectTo<IssueListDto>(_mapper.ConfigurationProvider);

        return await PaginatedList<IssueListDto>.CreateAsync(mappedQuery, filter.PageIndex, filter.PageSize);
    }
}
