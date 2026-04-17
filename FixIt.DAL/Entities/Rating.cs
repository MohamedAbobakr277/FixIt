namespace FixIt.DAL.Entities;

public class Rating
{
    public int RatingId { get; set; }
    public int Stars { get; set; }
    public string? Comment { get; set; }
    public DateTime SubmittedAt { get; set; }

    // Foreign Keys
    public int IssueId { get; set; }
    public int CitizenId { get; set; }

    // Navigation properties
    public Issue? Issue { get; set; }
    public Citizen? Citizen { get; set; }
}