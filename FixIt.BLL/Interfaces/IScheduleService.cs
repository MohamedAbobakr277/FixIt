using FixIt.BLL.DTOs;

namespace FixIt.BLL.Interfaces;

public interface IScheduleService
{
    Task<SchedulePageDto> GetSchedulePageAsync();
    Task<CreateScheduleDto?> GetCreateScheduleDtoAsync(int issueId);
    Task<bool> CreateScheduleAsync(CreateScheduleDto dto);
}
