using FixIt.Common.DTOs;
using FixIt.BLL.DTOs;

namespace FixIt.BLL.Interfaces;

public interface IAdminProfileService
{
    Task<AdminProfileDto> GetAdminProfileAsync(string adminId);
    Task<bool> UpdateAdminProfileAsync(string adminId, FixIt.BLL.DTOs.UpdateProfileDto dto);
    Task<bool> UpdateAdminNotificationsAsync(string adminId, UpdateNotificationsDto dto);
}
