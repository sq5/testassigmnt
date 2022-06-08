using Microsoft.EntityFrameworkCore.Migrations;

namespace DATABASE.Migrations
{
    public partial class InputsSync : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "SyncInputs",
                table: "ExtExchangeSettings",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SyncOnlyApprovedInputs",
                table: "ExtExchangeSettings",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SyncInputs",
                table: "ExtExchangeSettings");

            migrationBuilder.DropColumn(
                name: "SyncOnlyApprovedInputs",
                table: "ExtExchangeSettings");
        }
    }
}
