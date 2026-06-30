using System.ComponentModel.DataAnnotations;

namespace FixIt.Common.DTOs;

public class AddCommentDto
{
    [Required]
    public int IssueId { get; set; }

    [Required(ErrorMessage = "Comment text is required.")]
    [StringLength(1000, MinimumLength = 1, ErrorMessage = "Comment must be between 1 and 1000 characters.")]
    public string Text { get; set; } = string.Empty;
}
