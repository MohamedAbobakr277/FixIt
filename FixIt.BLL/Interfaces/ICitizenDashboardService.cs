using FixIt.Common.DTOs;

namespace FixIt.BLL.Interfaces;

public interface ICitizenDashboardService
{
    Task<CitizenDashboardDto> GetDashboardDataAsync(string citizenId);
    Task<CitizenProfileDto> GetProfileDataAsync(string citizenId);
}
