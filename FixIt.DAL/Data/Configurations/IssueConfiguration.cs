using FixIt.Common.Enums;
using FixIt.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FixIt.DAL.Data.Configurations
{
    public class IssueConfiguration : IEntityTypeConfiguration<Issue>
    {
        public void Configure(EntityTypeBuilder<Issue> builder)
        {
            builder.ToTable("Issues");

            builder.HasKey(i => i.IssueId);

            builder.Property(i => i.IssueId)
                .ValueGeneratedOnAdd();

            builder.Property(i => i.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(i => i.Description)
                .IsRequired();

            builder.Property(i => i.Location)
                .IsRequired()
                .HasMaxLength(300);

            builder.Property(i => i.ImageUrl)
                .HasMaxLength(500)
                .IsRequired(false);

            builder.Property(i => i.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasDefaultValue(IssueStatus.New);

            builder.Property(i => i.Category)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(i => i.Priority)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(i => i.AdminNotes)
                .HasMaxLength(1000)
                .IsRequired(false);

            builder.Property(i => i.SubmittedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(i => i.UpdatedAt)
                .IsRequired();

            // Issue → MaintenanceSchedule (One-to-One Composition)
            builder.HasOne(i => i.MaintenanceSchedule)
                .WithOne(ms => ms.Issue)
                .HasForeignKey<MaintenanceSchedule>(ms => ms.IssueId)
                .OnDelete(DeleteBehavior.Cascade);

            // Issue → MaintenanceReport (One-to-One Composition)
            builder.HasOne(i => i.MaintenanceReport)
                .WithOne(mr => mr.Issue)
                .HasForeignKey<MaintenanceReport>(mr => mr.IssueId)
                .OnDelete(DeleteBehavior.Cascade);

            // Issue → Rating (One-to-One Composition)
            builder.HasOne(i => i.Rating)
                .WithOne(r => r.Issue)
                .HasForeignKey<Rating>(r => r.IssueId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
