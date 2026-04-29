using IdaraTech.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdaraTech.Infrastructure.Data.Configurations;

public class PrestataireConfiguration : IEntityTypeConfiguration<Prestataire>
{
    public void Configure(EntityTypeBuilder<Prestataire> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Nom)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.Description)
            .HasMaxLength(1000);

        builder.HasMany(e => e.Services)
            .WithOne(s => s.Prestataire)
            .HasForeignKey(s => s.PrestataireId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
