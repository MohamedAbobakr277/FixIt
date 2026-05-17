using AutoMapper;
using FixIt.BLL.Interfaces;
using FixIt.Common.DTOs;
using FixIt.DAL.Entities;
using FixIt.DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace FixIt.BLL.Services;

public class IssueDetailsService : IIssueDetailsService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public IssueDetailsService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IssueDetailsDto?> GetIssueDetailsAsync(int issueId)
    {
        var issue = await _unitOfWork.Issues.GetAll(
            i => i.Citizen,
            i => i.MaintenanceSchedule,
            i => i.MaintenanceReport,
            i => i.Rating
        ).FirstOrDefaultAsync(i => i.IssueId == issueId);

        if (issue == null)
            return null;

        return _mapper.Map<IssueDetailsDto>(issue);
    }
}
