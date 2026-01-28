using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EirMed.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFileStorageFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StoragePath",
                table: "AppointmentAttachments",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "FileSizeBytes",
                table: "AppointmentAttachments",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StoragePath",
                table: "AppointmentAttachments");

            migrationBuilder.DropColumn(
                name: "FileSizeBytes",
                table: "AppointmentAttachments");
        }
    }
}
