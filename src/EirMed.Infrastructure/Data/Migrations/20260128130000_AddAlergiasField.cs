using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EirMed.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAlergiasField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Alergias",
                table: "Users",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Alergias",
                table: "Users");
        }
    }
}
