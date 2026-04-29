using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdaraTech.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SimplifyPrestataire : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateInscription",
                table: "Prestataires");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Prestataires");

            migrationBuilder.DropColumn(
                name: "Localisation",
                table: "Prestataires");

            migrationBuilder.DropColumn(
                name: "NoteMoyenne",
                table: "Prestataires");

            migrationBuilder.DropColumn(
                name: "Specialite",
                table: "Prestataires");

            migrationBuilder.DropColumn(
                name: "Telephone",
                table: "Prestataires");

            migrationBuilder.AddColumn<int>(
                name: "PrestataireId",
                table: "Services",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Prestataires",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Services_PrestataireId",
                table: "Services",
                column: "PrestataireId");

            migrationBuilder.AddForeignKey(
                name: "FK_Services_Prestataires_PrestataireId",
                table: "Services",
                column: "PrestataireId",
                principalTable: "Prestataires",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Services_Prestataires_PrestataireId",
                table: "Services");

            migrationBuilder.DropIndex(
                name: "IX_Services_PrestataireId",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "PrestataireId",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Prestataires");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DateInscription",
                table: "Prestataires",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Prestataires",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Localisation",
                table: "Prestataires",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "NoteMoyenne",
                table: "Prestataires",
                type: "float(4)",
                precision: 4,
                scale: 2,
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "Specialite",
                table: "Prestataires",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Telephone",
                table: "Prestataires",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");
        }
    }
}
