using Microsoft.EntityFrameworkCore.Migrations;

namespace DATABASE.Migrations
{
    public partial class ocr : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OCRState",
                table: "NonFormDocs",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OCRVerified",
                table: "NonFormDocs",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OCRXML",
                table: "NonFormDocs",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "OCR",
                table: "ExtConnections",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OCRState",
                table: "NonFormDocs");

            migrationBuilder.DropColumn(
                name: "OCRVerified",
                table: "NonFormDocs");

            migrationBuilder.DropColumn(
                name: "OCRXML",
                table: "NonFormDocs");

            migrationBuilder.DropColumn(
                name: "OCR",
                table: "ExtConnections");
        }
    }
}
