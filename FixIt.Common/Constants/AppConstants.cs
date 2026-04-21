namespace FixIt.Common.Constants;

public static class AppConstants
{
    // Pagination
    public const int DefaultPageSize = 10;
    public const int MaxPageSize = 50;

    // File Uploads
    public const int MaxImageSizeBytes = 5 * 1024 * 1024; // 5 MB
    public const string UploadsIssuesPath = "uploads/issues";
    public const string UploadsReportsPath = "uploads/reports";
    public static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

    // Roles
    public const string CitizenRole = "Citizen";
    public const string AdminRole = "Admin";
}
