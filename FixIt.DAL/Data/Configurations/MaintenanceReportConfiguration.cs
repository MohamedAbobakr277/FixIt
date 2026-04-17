using FixIt.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FixIt.DAL.Data.Configurations
{
    public class MaintenanceReportConfiguration : IEntityTypeConfiguration<MaintenanceReport>
    {
        public void Configure(EntityTypeBuilder<MaintenanceReport> builder)
        {
            builder.ToTable("MaintenanceReports");

            builder.HasKey(mr => mr.ReportId);

            builder.Property(mr => mr.ReportId)
                .ValueGeneratedOnAdd();

            builder.Property(mr => mr.Summary)
                .IsRequired();

            builder.Property(mr => mr.WorkerNotes)
                .IsRequired(false);

            builder.Property(mr => mr.BeforeImageUrl)
                .HasMaxLength(500)
                .IsRequired(false);

            builder.Property(mr => mr.AfterImageUrl)
                .HasMaxLength(500)
                .IsRequired(false);

            builder.Property(mr => mr.SubmittedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // Unique constraint — enforces One-to-One with Issue
            builder.HasIndex(mr => mr.IssueId)
                .IsUnique();
        }
    }

}
