using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DATABASE.Migrations
{
    public partial class Cards : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ModifiedById",
                table: "Metadatas");

            migrationBuilder.DropColumn(
                name: "ModifiedById",
                table: "Contracts");

            migrationBuilder.AlterColumn<string>(
                name: "Source",
                table: "Metadatas",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(30) CHARACTER SET utf8mb4",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<int>(
                name: "OrganizationId",
                table: "Metadatas",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Metadatas",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Comment",
                table: "Metadatas",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Contact",
                table: "Metadatas",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Metadatas",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Delivery",
                table: "Metadatas",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "Metadatas",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "PeriodFrom",
                table: "Metadatas",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PeriodTo",
                table: "Metadatas",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Reciever",
                table: "Metadatas",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Reestr",
                table: "DocTypes",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "Amount",
                table: "Contracts",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "AmountWOVAT",
                table: "Contracts",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Comment",
                table: "Contracts",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Contracts",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "Contracts",
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DocKindId",
                table: "Contracts",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DocTypeId",
                table: "Contracts",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "Contracts",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                table: "Contracts",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Subject",
                table: "Contracts",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "VAT",
                table: "Contracts",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Versions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MetadataId = table.Column<long>(nullable: true),
                    ContractId = table.Column<int>(nullable: true),
                    Action = table.Column<string>(maxLength: 255, nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    User = table.Column<string>(maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Versions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Versions_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Versions_Metadatas_MetadataId",
                        column: x => x.MetadataId,
                        principalTable: "Metadatas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_DocKindId",
                table: "Contracts",
                column: "DocKindId");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_DocTypeId",
                table: "Contracts",
                column: "DocTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_OrganizationId",
                table: "Contracts",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Versions_ContractId",
                table: "Versions",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_Versions_MetadataId",
                table: "Versions",
                column: "MetadataId");

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_DocKinds_DocKindId",
                table: "Contracts",
                column: "DocKindId",
                principalTable: "DocKinds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_DocTypes_DocTypeId",
                table: "Contracts",
                column: "DocTypeId",
                principalTable: "DocTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_Organizations_OrganizationId",
                table: "Contracts",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_DocKinds_DocKindId",
                table: "Contracts");

            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_DocTypes_DocTypeId",
                table: "Contracts");

            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_Organizations_OrganizationId",
                table: "Contracts");

            migrationBuilder.DropTable(
                name: "Versions");

            migrationBuilder.DropIndex(
                name: "IX_Contracts_DocKindId",
                table: "Contracts");

            migrationBuilder.DropIndex(
                name: "IX_Contracts_DocTypeId",
                table: "Contracts");

            migrationBuilder.DropIndex(
                name: "IX_Contracts_OrganizationId",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "Metadatas");

            migrationBuilder.DropColumn(
                name: "Comment",
                table: "Metadatas");

            migrationBuilder.DropColumn(
                name: "Contact",
                table: "Metadatas");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Metadatas");

            migrationBuilder.DropColumn(
                name: "Delivery",
                table: "Metadatas");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "Metadatas");

            migrationBuilder.DropColumn(
                name: "PeriodFrom",
                table: "Metadatas");

            migrationBuilder.DropColumn(
                name: "PeriodTo",
                table: "Metadatas");

            migrationBuilder.DropColumn(
                name: "Reciever",
                table: "Metadatas");

            migrationBuilder.DropColumn(
                name: "Reestr",
                table: "DocTypes");

            migrationBuilder.DropColumn(
                name: "Amount",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "AmountWOVAT",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "Comment",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "DocKindId",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "DocTypeId",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "Subject",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "VAT",
                table: "Contracts");

            migrationBuilder.AlterColumn<string>(
                name: "Source",
                table: "Metadatas",
                type: "varchar(30) CHARACTER SET utf8mb4",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "OrganizationId",
                table: "Metadatas",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedById",
                table: "Metadatas",
                type: "varchar(200) CHARACTER SET utf8mb4",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ModifiedById",
                table: "Contracts",
                type: "varchar(200) CHARACTER SET utf8mb4",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }
    }
}
