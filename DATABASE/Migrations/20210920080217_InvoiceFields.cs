using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DATABASE.Migrations
{
    public partial class InvoiceFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "AmountToPay",
                table: "Metadatas",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DocDateInvoice",
                table: "Metadatas",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DocNumInvoice",
                table: "Metadatas",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Paid",
                table: "Metadatas",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AmountToPay",
                table: "Metadatas");

            migrationBuilder.DropColumn(
                name: "DocDateInvoice",
                table: "Metadatas");

            migrationBuilder.DropColumn(
                name: "DocNumInvoice",
                table: "Metadatas");

            migrationBuilder.DropColumn(
                name: "Paid",
                table: "Metadatas");
        }
    }
}
