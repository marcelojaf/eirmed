using EirMed.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EirMed.Infrastructure.Data.Configurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(a => a.Data)
            .IsRequired();

        builder.Property(a => a.Especialidade)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.QueixaPrincipal)
            .HasMaxLength(500);

        builder.Property(a => a.ObservacoesGerais)
            .HasMaxLength(2000);

        builder.Property(a => a.Diagnosticos)
            .HasMaxLength(2000);

        builder.Property(a => a.CreatedAt).IsRequired();

        // Indexes
        builder.HasIndex(a => a.DoctorId);
        builder.HasIndex(a => a.Data);

        builder.HasMany(a => a.Exams)
            .WithOne(e => e.Appointment)
            .HasForeignKey(e => e.AppointmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.Prescriptions)
            .WithOne(p => p.Appointment)
            .HasForeignKey(p => p.AppointmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.Attachments)
            .WithOne(at => at.Appointment)
            .HasForeignKey(at => at.AppointmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
