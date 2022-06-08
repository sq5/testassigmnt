using Microsoft.EntityFrameworkCore.Migrations;

namespace DATABASE.Migrations
{
    public partial class AddlookuporganizationtoNFD : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_NonFormDocs_OrganizationId",
                table: "NonFormDocs",
                column: "OrganizationId");

            migrationBuilder.AddForeignKey(
                name: "FK_NonFormDocs_Organizations_OrganizationId",
                table: "NonFormDocs",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NonFormDocs_Organizations_OrganizationId",
                table: "NonFormDocs");

            migrationBuilder.DropIndex(
                name: "IX_NonFormDocs_OrganizationId",
                table: "NonFormDocs");
        }
    }
}
