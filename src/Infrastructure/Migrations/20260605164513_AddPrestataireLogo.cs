using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdaraTech.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPrestataireLogo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Logo",
                table: "Prestataires",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Logo",
                table: "Prestataires");
        }
    }
}
