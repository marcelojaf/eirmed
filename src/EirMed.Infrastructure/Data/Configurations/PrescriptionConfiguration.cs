using EirMed.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EirMed.Infrastructure.Data.Configurations;

public class PrescriptionConfiguration : IEntityTypeConfiguration<Prescription>
{
    public void Configure(EntityTypeBuilder<Prescription> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(p => p.DataInicio)
            .IsRequired();

        builder.Property(p => p.Motivo)
            .HasMaxLength(500);

        builder.Property(p => p.CreatedAt).IsRequired();

        // Indexes
        builder.HasIndex(p => p.MedicationId);
        builder.HasIndex(p => p.AppointmentId);
        builder.HasIndex(p => p.DataInicio);
    }
}
