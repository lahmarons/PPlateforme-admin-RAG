using IdaraTech.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdaraTech.Infrastructure.Data.Configurations;

public class UtilisateurConfiguration : IEntityTypeConfiguration<Utilisateur>
{
    public void Configure(EntityTypeBuilder<Utilisateur> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Nom)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.Email)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.MotDePasse)
            .HasMaxLength(500)
            .IsRequired();

        builder.HasDiscriminator<string>("Discriminator")
            .HasValue<Agent>("Agent")
            .HasValue<Citoyen>("Citoyen")
            .HasValue<Admin>("Admin");
    }
}
