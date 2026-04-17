using FixIt.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FixIt.DAL.Data.Configurations
{
    public class MaintenanceScheduleConfiguration : IEntityTypeConfiguration<MaintenanceSchedule>
    {
        public void Configure(EntityTypeBuilder<MaintenanceSchedule> builder)
        {
            builder.ToTable("MaintenanceSchedules");

            builder.HasKey(ms => ms.ScheduleId);

            builder.Property(ms => ms.ScheduleId)
                .ValueGeneratedOnAdd();

            builder.Property(ms => ms.VisitDate)
                .IsRequired();

            builder.Property(ms => ms.EstimatedCost)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(ms => ms.WorkerName)
                .HasMaxLength(150)
                .IsRequired(false);

            builder.Property(ms => ms.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // Unique constraint — enforces One-to-One with Issue
            builder.HasIndex(ms => ms.IssueId)
                .IsUnique();
        }
    }
}
