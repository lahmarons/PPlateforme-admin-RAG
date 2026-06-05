using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdaraTech.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentContenu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "Contenu",
                table: "Documents",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Contenu",
                table: "Documents");
        }
    }
}
