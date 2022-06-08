using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DATABASE.Migrations
{
    public partial class RecycleBin : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeleteDate",
                table: "Metadatas",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "Metadatas",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeleteDate",
                table: "Contracts",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "Contracts",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeleteDate",
                table: "Metadatas");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "Metadatas");

            migrationBuilder.DropColumn(
                name: "DeleteDate",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "Contracts");
        }
    }
}
