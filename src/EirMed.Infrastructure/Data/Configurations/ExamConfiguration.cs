using EirMed.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EirMed.Infrastructure.Data.Configurations;

public class ExamConfiguration : IEntityTypeConfiguration<Exam>
{
    public void Configure(EntityTypeBuilder<Exam> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(e => e.TipoExame)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(e => e.Nome)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.DataRealizacao)
            .IsRequired();

        builder.Property(e => e.Laboratorio)
            .HasMaxLength(200);

        builder.Property(e => e.Resultados)
            .HasMaxLength(4000);

        builder.Property(e => e.FileUrl)
            .HasMaxLength(500);

        builder.Property(e => e.CreatedAt).IsRequired();

        // Indexes
        builder.HasIndex(e => e.AppointmentId);
        builder.HasIndex(e => e.TipoExame);
        builder.HasIndex(e => e.DataRealizacao);
        builder.HasIndex(e => e.Nome);
    }
}
