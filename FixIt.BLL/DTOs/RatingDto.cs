using System;

namespace FixIt.BLL.DTOs;

public class RatingDto
{
    public int Stars { get; set; }
    public string? Comment { get; set; }
    public DateTime SubmittedAt { get; set; }
}
