using FixIt.BLL.DTOs;

namespace FixIt.BLL.Interfaces;

public interface IReportService
{
    Task<ReportsPageDto> GetReportsPageAsync();
    Task<CreateReportDto?> GetCreateReportDtoAsync(int issueId);
    Task<bool> SubmitReportAsync(CreateReportDto dto, string adminId);
}
