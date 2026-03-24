using IdaraTech.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdaraTech.Infrastructure.Data.Configurations;

public class CitoyenConfiguration : IEntityTypeConfiguration<Citoyen>
{
    public void Configure(EntityTypeBuilder<Citoyen> builder)
    {
        builder.Property(e => e.Adresse)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(e => e.Telephone)
            .HasMaxLength(20)
            .IsRequired();

        builder.HasMany(e => e.Demandes)
            .WithOne(d => d.Citoyen)
            .HasForeignKey(d => d.CitoyenId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
