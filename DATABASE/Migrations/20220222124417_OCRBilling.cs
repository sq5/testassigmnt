using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DATABASE.Migrations
{
    public partial class OCRBilling : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Amount",
                table: "NonFormDocs",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ContractorId",
                table: "NonFormDocs",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DocDate",
                table: "NonFormDocs",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "OCRSplit",
                table: "NonFormDocs",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "OCRSplit",
                table: "ExtConnections",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OCRQuota",
                table: "Clients",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OCRUsed",
                table: "Clients",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AspNetUserTokens",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(128) CHARACTER SET utf8mb4",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserTokens",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(128) CHARACTER SET utf8mb4",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "ProviderKey",
                table: "AspNetUserLogins",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(128) CHARACTER SET utf8mb4",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserLogins",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(128) CHARACTER SET utf8mb4",
                oldMaxLength: 128);

            migrationBuilder.CreateIndex(
                name: "IX_NonFormDocs_ContractorId",
                table: "NonFormDocs",
                column: "ContractorId");

            migrationBuilder.AddForeignKey(
                name: "FK_NonFormDocs_Contractors_ContractorId",
                table: "NonFormDocs",
                column: "ContractorId",
                principalTable: "Contractors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NonFormDocs_Contractors_ContractorId",
                table: "NonFormDocs");

            migrationBuilder.DropIndex(
                name: "IX_NonFormDocs_ContractorId",
                table: "NonFormDocs");

            migrationBuilder.DropColumn(
                name: "Amount",
                table: "NonFormDocs");

            migrationBuilder.DropColumn(
                name: "ContractorId",
                table: "NonFormDocs");

            migrationBuilder.DropColumn(
                name: "DocDate",
                table: "NonFormDocs");

            migrationBuilder.DropColumn(
                name: "OCRSplit",
                table: "NonFormDocs");

            migrationBuilder.DropColumn(
                name: "OCRSplit",
                table: "ExtConnections");

            migrationBuilder.DropColumn(
                name: "OCRQuota",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "OCRUsed",
                table: "Clients");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AspNetUserTokens",
                type: "varchar(128) CHARACTER SET utf8mb4",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserTokens",
                type: "varchar(128) CHARACTER SET utf8mb4",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "ProviderKey",
                table: "AspNetUserLogins",
                type: "varchar(128) CHARACTER SET utf8mb4",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserLogins",
                type: "varchar(128) CHARACTER SET utf8mb4",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string));
        }
    }
}
