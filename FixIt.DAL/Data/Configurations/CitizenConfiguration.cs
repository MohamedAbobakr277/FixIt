using FixIt.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FixIt.DAL.Data.Configurations
{
    public class CitizenConfiguration : IEntityTypeConfiguration<Citizen>
    {
        public void Configure(EntityTypeBuilder<Citizen> builder)
        {
            builder.Property(c => c.Address)
                .HasMaxLength(300)
                .IsRequired(false);

            builder.Property(c => c.ProfilePicture)
                .HasMaxLength(500)
                .IsRequired(false);

            // One Citizen → Many Issues
            builder.HasMany(c => c.Issues)
                .WithOne(i => i.Citizen)
                .HasForeignKey(i => i.CitizenId)
                .OnDelete(DeleteBehavior.Restrict);

            // One Citizen → Many Ratings
            builder.HasMany(c => c.Ratings)
                .WithOne(r => r.Citizen)
                .HasForeignKey(r => r.CitizenId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

}
