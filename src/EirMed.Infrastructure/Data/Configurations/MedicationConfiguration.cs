using EirMed.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EirMed.Infrastructure.Data.Configurations;

public class MedicationConfiguration : IEntityTypeConfiguration<Medication>
{
    public void Configure(EntityTypeBuilder<Medication> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(m => m.Nome)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(m => m.PrincipioAtivo)
            .HasMaxLength(200);

        builder.Property(m => m.Dosagem)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.FormaFarmaceutica)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(m => m.TipoUso)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(m => m.QuantidadeAtual)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(m => m.QuantidadeMinima)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(m => m.CreatedAt).IsRequired();

        // Indexes
        builder.HasIndex(m => m.UserId);
        builder.HasIndex(m => m.Nome);
        builder.HasIndex(m => m.TipoUso);

        builder.HasMany(m => m.Prescriptions)
            .WithOne(p => p.Medication)
            .HasForeignKey(p => p.MedicationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
