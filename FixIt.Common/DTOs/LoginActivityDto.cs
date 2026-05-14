namespace FixIt.Common.DTOs;

public class LoginActivityDto
{
    public string Device { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public bool IsCurrentSession { get; set; }
}
