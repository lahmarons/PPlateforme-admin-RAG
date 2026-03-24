using IdaraTech.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdaraTech.Infrastructure.Data.Configurations;

public class AdminConfiguration : IEntityTypeConfiguration<Admin>
{
    public void Configure(EntityTypeBuilder<Admin> builder)
    {
        builder.HasMany(e => e.ServicesGeres)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "AdminService",
                r => r.HasOne<Service>().WithMany().HasForeignKey("ServiceId"),
                l => l.HasOne<Admin>().WithMany().HasForeignKey("AdminId"),
                je => je.HasKey("AdminId", "ServiceId"));
    }
}
