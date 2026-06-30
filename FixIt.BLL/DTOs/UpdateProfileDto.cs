using FixIt.Common.DTOs;
using Microsoft.AspNetCore.Http;

namespace FixIt.BLL.DTOs;

public class UpdateProfileDto : FixIt.Common.DTOs.UpdateProfileDto
{
    public IFormFile? ProfilePicture { get; set; }
}
