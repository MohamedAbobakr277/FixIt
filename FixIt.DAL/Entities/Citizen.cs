using FixIt.Common.Enums;

namespace FixIt.DAL.Entities;

    public class Citizen : User
{
    public string? Address { get; set; }
    public string? ProfilePicture { get; set; }
 
    // Navigation properties
    public ICollection<Issue> Issues { get; set; } = new List<Issue>();
    public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
 
    public Citizen()
    {
        Role = UserRole.Citizen;
    }
}