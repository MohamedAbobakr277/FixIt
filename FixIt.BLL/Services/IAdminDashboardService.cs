using FixIt.BLL.DTOs;

namespace FixIt.BLL.Interfaces;

public interface IAdminDashboardService
{
    Task<DashboardStatsDto> GetDashboardStatsAsync();
}
