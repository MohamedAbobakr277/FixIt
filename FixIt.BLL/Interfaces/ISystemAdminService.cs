using FixIt.BLL.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FixIt.BLL.Interfaces;

public interface ISystemAdminService
{
    Task<SystemDashboardStatsDto> GetSystemDashboardStatsAsync();
    Task<IEnumerable<UserManagementDto>> GetAllUsersAsync();
    Task<bool> ToggleUserLockStatusAsync(string userId);
    Task<bool> ChangeUserRoleAsync(string userId, string newRole);
}
