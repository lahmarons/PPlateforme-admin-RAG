using IdaraTech.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdaraTech.Infrastructure.Data.Configurations;

public class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Nom)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.Description)
            .HasMaxLength(1000);

        builder.HasMany(e => e.Agents)
            .WithOne(a => a.Service)
            .HasForeignKey(a => a.ServiceId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(e => e.Demandes)
            .WithOne(d => d.ServiceConcerne)
            .HasForeignKey(d => d.ServiceId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
