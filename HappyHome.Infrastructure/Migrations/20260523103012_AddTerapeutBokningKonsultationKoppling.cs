using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HappyHome.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTerapeutBokningKonsultationKoppling : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BokningId",
                table: "Konsultationer",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TerapeutId",
                table: "Konsultationer",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Terapeuter",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Förnamn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Efternamn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Epost = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Roll = table.Column<int>(type: "int", nullable: false),
                    Specialiseringar = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AktivFromDatum = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Aktiv = table.Column<bool>(type: "bit", nullable: false),
                    Skapad = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Terapeuter", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tidsluckor",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TerapeutId = table.Column<int>(type: "int", nullable: false),
                    Start = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Slut = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Skapad = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tidsluckor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tidsluckor_Terapeuter_TerapeutId",
                        column: x => x.TerapeutId,
                        principalTable: "Terapeuter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Bokningar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KlientId = table.Column<int>(type: "int", nullable: false),
                    TidsluckaId = table.Column<int>(type: "int", nullable: false),
                    TerapiTyp = table.Column<int>(type: "int", nullable: false),
                    AnledningTillBesok = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Skapad = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bokningar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bokningar_Klienter_KlientId",
                        column: x => x.KlientId,
                        principalTable: "Klienter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bokningar_Tidsluckor_TidsluckaId",
                        column: x => x.TidsluckaId,
                        principalTable: "Tidsluckor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Konsultationer_BokningId",
                table: "Konsultationer",
                column: "BokningId",
                unique: true,
                filter: "[BokningId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Konsultationer_TerapeutId",
                table: "Konsultationer",
                column: "TerapeutId");

            migrationBuilder.CreateIndex(
                name: "IX_Bokningar_KlientId",
                table: "Bokningar",
                column: "KlientId");

            migrationBuilder.CreateIndex(
                name: "IX_Bokningar_TidsluckaId",
                table: "Bokningar",
                column: "TidsluckaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Terapeuter_Epost",
                table: "Terapeuter",
                column: "Epost",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tidsluckor_TerapeutId_Start",
                table: "Tidsluckor",
                columns: new[] { "TerapeutId", "Start" });

            migrationBuilder.AddForeignKey(
                name: "FK_Konsultationer_Bokningar_BokningId",
                table: "Konsultationer",
                column: "BokningId",
                principalTable: "Bokningar",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Konsultationer_Terapeuter_TerapeutId",
                table: "Konsultationer",
                column: "TerapeutId",
                principalTable: "Terapeuter",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Konsultationer_Bokningar_BokningId",
                table: "Konsultationer");

            migrationBuilder.DropForeignKey(
                name: "FK_Konsultationer_Terapeuter_TerapeutId",
                table: "Konsultationer");

            migrationBuilder.DropTable(
                name: "Bokningar");

            migrationBuilder.DropTable(
                name: "Tidsluckor");

            migrationBuilder.DropTable(
                name: "Terapeuter");

            migrationBuilder.DropIndex(
                name: "IX_Konsultationer_BokningId",
                table: "Konsultationer");

            migrationBuilder.DropIndex(
                name: "IX_Konsultationer_TerapeutId",
                table: "Konsultationer");

            migrationBuilder.DropColumn(
                name: "BokningId",
                table: "Konsultationer");

            migrationBuilder.DropColumn(
                name: "TerapeutId",
                table: "Konsultationer");
        }
    }
}
