using System.ComponentModel.DataAnnotations;

namespace FixIt.Common.DTOs;

public class UpdateProfileDto
{
    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
}
