using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HappyHome.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialKlientKonsultation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Klienter",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Förnamn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Efternamn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Epost = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Telefon = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Födelsedatum = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Skapad = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Klienter", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Konsultationer",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KlientId = table.Column<int>(type: "int", nullable: false),
                    Typ = table.Column<int>(type: "int", nullable: false),
                    Datum = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Anteckningar = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Diagnosförslag = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Skapad = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Konsultationer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Konsultationer_Klienter_KlientId",
                        column: x => x.KlientId,
                        principalTable: "Klienter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Klienter_Epost",
                table: "Klienter",
                column: "Epost",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Konsultationer_KlientId",
                table: "Konsultationer",
                column: "KlientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Konsultationer");

            migrationBuilder.DropTable(
                name: "Klienter");
        }
    }
}
