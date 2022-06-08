using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DATABASE.Migrations
{
    public partial class Addfieldsmetadataandcontract : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EDIIsIncoming",
                table: "Metadatas",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EDILocalSigned",
                table: "Metadatas",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EdiNeedExport",
                table: "Metadatas",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaymentDate",
                table: "Metadatas",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentNumber",
                table: "Metadatas",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EDIIsIncoming",
                table: "Contracts",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EDILocalSigned",
                table: "Contracts",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EdiNeedExport",
                table: "Contracts",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EDIIsIncoming",
                table: "Metadatas");

            migrationBuilder.DropColumn(
                name: "EDILocalSigned",
                table: "Metadatas");

            migrationBuilder.DropColumn(
                name: "EdiNeedExport",
                table: "Metadatas");

            migrationBuilder.DropColumn(
                name: "PaymentDate",
                table: "Metadatas");

            migrationBuilder.DropColumn(
                name: "PaymentNumber",
                table: "Metadatas");

            migrationBuilder.DropColumn(
                name: "EDIIsIncoming",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "EDILocalSigned",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "EdiNeedExport",
                table: "Contracts");
        }
    }
}
