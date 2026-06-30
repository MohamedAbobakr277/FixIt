namespace FixIt.DAL.Entities;

public class Citizen : ApplicationUser
{
    public string? Address { get; set; }

    // Navigation properties
    public ICollection<Issue> Issues { get; set; } = new List<Issue>();
    public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
}