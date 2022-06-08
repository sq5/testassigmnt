using Microsoft.EntityFrameworkCore.Migrations;

namespace DATABASE.Migrations
{
    public partial class ocr2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DocTypeId",
                table: "NonFormDocs",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_NonFormDocs_DocTypeId",
                table: "NonFormDocs",
                column: "DocTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_NonFormDocs_DocTypes_DocTypeId",
                table: "NonFormDocs",
                column: "DocTypeId",
                principalTable: "DocTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NonFormDocs_DocTypes_DocTypeId",
                table: "NonFormDocs");

            migrationBuilder.DropIndex(
                name: "IX_NonFormDocs_DocTypeId",
                table: "NonFormDocs");

            migrationBuilder.DropColumn(
                name: "DocTypeId",
                table: "NonFormDocs");
        }
    }
}
