using FixIt.DAL.Entities;
using FixIt.DAL.Data.Configurations;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FixIt.DAL.Data;

public class FixItDbContext : IdentityDbContext<ApplicationUser>
{
    public FixItDbContext(DbContextOptions<FixItDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new ApplicationUserConfiguration());
        modelBuilder.ApplyConfiguration(new CitizenConfiguration());
        modelBuilder.ApplyConfiguration(new AdminConfiguration());
        modelBuilder.ApplyConfiguration(new IssueConfiguration());
        modelBuilder.ApplyConfiguration(new MaintenanceScheduleConfiguration());
        modelBuilder.ApplyConfiguration(new MaintenanceReportConfiguration());
        modelBuilder.ApplyConfiguration(new RatingConfiguration());
        modelBuilder.ApplyConfiguration(new PaymentConfiguration());
    }

    public DbSet<Citizen> Citizens { get; set; }
    public DbSet<Admin> Admins { get; set; }
    public DbSet<Issue> Issues { get; set; }
    public DbSet<MaintenanceSchedule> MaintenanceSchedules { get; set; }
    public DbSet<MaintenanceReport> MaintenanceReports { get; set; }
    public DbSet<Rating> Ratings { get; set; }
    public DbSet<LoginHistory> LoginHistories { get; set; }
    public DbSet<IssueComment> IssueComments { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<AdminNotification> AdminNotifications { get; set; }
    public DbSet<Payment> Payments { get; set; }
}
