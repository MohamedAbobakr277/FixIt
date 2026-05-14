using FixIt.Common.DTOs;
using System.Threading.Tasks;

namespace FixIt.BLL.Interfaces;

public interface IRatingAdminService
{
    Task<AdminRatingsPageDto> GetRatingsPageDataAsync();
}
