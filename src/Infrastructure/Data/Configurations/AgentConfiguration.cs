using IdaraTech.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdaraTech.Infrastructure.Data.Configurations;

public class AgentConfiguration : IEntityTypeConfiguration<Agent>
{
    public void Configure(EntityTypeBuilder<Agent> builder)
    {
        builder.Property(e => e.Matricule)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.Grade)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.IsActive)
            .IsRequired();

        builder.HasOne(e => e.Service)
            .WithMany(s => s.Agents)
            .HasForeignKey(e => e.ServiceId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(e => e.DemandesToitees)
            .WithOne(d => d.AgentTraitant)
            .HasForeignKey(d => d.AgentId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
