using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HappyHome.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAnvändare : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Användare",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Epost = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    LösenordHash = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Roll = table.Column<int>(type: "int", nullable: false),
                    KlientId = table.Column<int>(type: "int", nullable: true),
                    TerapeutId = table.Column<int>(type: "int", nullable: true),
                    Skapad = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Användare", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Användare_Klienter_KlientId",
                        column: x => x.KlientId,
                        principalTable: "Klienter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Användare_Terapeuter_TerapeutId",
                        column: x => x.TerapeutId,
                        principalTable: "Terapeuter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Användare_Epost",
                table: "Användare",
                column: "Epost",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Användare_KlientId",
                table: "Användare",
                column: "KlientId");

            migrationBuilder.CreateIndex(
                name: "IX_Användare_TerapeutId",
                table: "Användare",
                column: "TerapeutId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Användare");
        }
    }
}
