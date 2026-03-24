using IdaraTech.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdaraTech.Infrastructure.Data.Configurations;

public class DemandeConfiguration : IEntityTypeConfiguration<Demande>
{
    public void Configure(EntityTypeBuilder<Demande> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Description)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(e => e.Statut)
            .HasConversion<int>();

        builder.HasOne(e => e.Citoyen)
            .WithMany(c => c.Demandes)
            .HasForeignKey(e => e.CitoyenId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(e => e.ServiceConcerne)
            .WithMany(s => s.Demandes)
            .HasForeignKey(e => e.ServiceId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.AgentTraitant)
            .WithMany(a => a.DemandesToitees)
            .HasForeignKey(e => e.AgentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(e => e.Documents)
            .WithOne(d => d.Demande)
            .HasForeignKey(d => d.DemandeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
