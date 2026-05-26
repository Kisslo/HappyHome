using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HappyHome.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddKlientPersonnummerKonsultationSymptomBakgrund : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Bakgrund",
                table: "Konsultationer",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Symptom",
                table: "Konsultationer",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Personnummer",
                table: "Klienter",
                type: "nvarchar(13)",
                maxLength: 13,
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE [Klienter]
                SET [Personnummer] = FORMAT([Födelsedatum], 'yyMMdd') + '-' + RIGHT('0000' + CAST([Id] AS VARCHAR(4)), 4)
                WHERE [Personnummer] IS NULL OR [Personnummer] = '';
                """);

            migrationBuilder.AlterColumn<string>(
                name: "Personnummer",
                table: "Klienter",
                type: "nvarchar(13)",
                maxLength: 13,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(13)",
                oldMaxLength: 13,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Klienter_Personnummer",
                table: "Klienter",
                column: "Personnummer",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Klienter_Personnummer",
                table: "Klienter");

            migrationBuilder.DropColumn(
                name: "Bakgrund",
                table: "Konsultationer");

            migrationBuilder.DropColumn(
                name: "Symptom",
                table: "Konsultationer");

            migrationBuilder.DropColumn(
                name: "Personnummer",
                table: "Klienter");
        }
    }
}
