using FixIt.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FixIt.DAL.Data.Configurations
{
    public class AdminConfiguration : IEntityTypeConfiguration<Admin>
    {
        public void Configure(EntityTypeBuilder<Admin> builder)
        {
            builder.Property(a => a.Department)
                .HasMaxLength(150)
                .IsRequired(false);

            // One Admin → Many Issues
            builder.HasMany(a => a.Issues)
                .WithOne(i => i.Admin)
                .HasForeignKey(i => i.AdminId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            // One Admin → Many MaintenanceReports
            builder.HasMany(a => a.MaintenanceReports)
                .WithOne(mr => mr.Admin)
                .HasForeignKey(mr => mr.AdminId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
