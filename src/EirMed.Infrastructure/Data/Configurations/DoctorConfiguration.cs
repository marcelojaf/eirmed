using EirMed.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EirMed.Infrastructure.Data.Configurations;

public class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
{
    public void Configure(EntityTypeBuilder<Doctor> builder)
    {
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(d => d.Nome)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(d => d.Especialidade)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(d => d.RegistroProfissional)
            .HasMaxLength(50);

        builder.Property(d => d.ClinicaHospital)
            .HasMaxLength(200);

        builder.Property(d => d.Contato)
            .HasMaxLength(200);

        builder.Property(d => d.CreatedAt).IsRequired();

        // Indexes
        builder.HasIndex(d => d.UserId);
        builder.HasIndex(d => d.Especialidade);
        builder.HasIndex(d => new { d.UserId, d.Nome });

        builder.HasMany(d => d.Appointments)
            .WithOne(a => a.Doctor)
            .HasForeignKey(a => a.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
