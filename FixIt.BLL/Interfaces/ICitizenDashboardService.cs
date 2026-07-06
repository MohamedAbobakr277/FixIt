using FixIt.Common.DTOs;

namespace FixIt.BLL.Interfaces;

public interface ICitizenDashboardService
{
    Task<CitizenDashboardDto> GetDashboardDataAsync(string citizenId);
    Task<CitizenProfileDto> GetProfileDataAsync(string citizenId);
    Task<bool> UpdateProfileAsync(string citizenId, FixIt.BLL.DTOs.UpdateProfileDto dto);
    Task<bool> UpdateNotificationsAsync(string citizenId, FixIt.BLL.DTOs.UpdateNotificationsDto dto);
}
