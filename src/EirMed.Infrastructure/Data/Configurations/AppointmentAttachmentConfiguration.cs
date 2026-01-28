using EirMed.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EirMed.Infrastructure.Data.Configurations;

public class AppointmentAttachmentConfiguration : IEntityTypeConfiguration<AppointmentAttachment>
{
    public void Configure(EntityTypeBuilder<AppointmentAttachment> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(a => a.FileName)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(a => a.FileUrl)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(a => a.StoragePath)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(a => a.ContentType)
            .HasMaxLength(100);

        builder.Property(a => a.FileSizeBytes)
            .IsRequired();

        builder.Property(a => a.CreatedAt).IsRequired();

        // Indexes
        builder.HasIndex(a => a.AppointmentId);
    }
}
