using IdaraTech.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdaraTech.Infrastructure.Data.Configurations;

public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Nom)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.Type)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasOne(e => e.Demande)
            .WithMany(d => d.Documents)
            .HasForeignKey(e => e.DemandeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
