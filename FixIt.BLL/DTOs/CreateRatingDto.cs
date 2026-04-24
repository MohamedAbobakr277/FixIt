using System.ComponentModel.DataAnnotations;

namespace FixIt.BLL.DTOs;

public class CreateRatingDto
{
    public int IssueId { get; set; }
    
    [Display(Name = "Rating")]
    public int Stars { get; set; }
    
    public string? Comment { get; set; }
}
