using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DATABASE.Migrations
{
    public partial class diadoc : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EDIId",
                table: "Metadatas",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EDIProcessed",
                table: "Metadatas",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "EDIProvider",
                table: "Metadatas",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EDIState",
                table: "Metadatas",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EDIId",
                table: "Contracts",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EDIProcessed",
                table: "Contracts",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "EDIProvider",
                table: "Contracts",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EDIState",
                table: "Contracts",
                maxLength: 50,
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

            migrationBuilder.CreateTable(
                name: "EDISettings",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ClientID = table.Column<int>(nullable: false),
                    EDILogin = table.Column<string>(maxLength: 300, nullable: false),
                    EDIPassword = table.Column<string>(maxLength: 300, nullable: true),
                    EDIProvider = table.Column<string>(maxLength: 50, nullable: false),
                    EDIUserID = table.Column<string>(maxLength: 300, nullable: false),
                    LastEvent = table.Column<string>(maxLength: 100, nullable: true),
                    LastEventDate = table.Column<DateTime>(nullable: true),
                    OrganizationKPP = table.Column<string>(maxLength: 50, nullable: true),
                    OrganizationINN = table.Column<string>(maxLength: 50, nullable: true),
                    OrganizationName = table.Column<string>(maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EDISettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EDISettings_Clients_ClientID",
                        column: x => x.ClientID,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EDISettings_ClientID",
                table: "EDISettings",
                column: "ClientID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EDISettings");

            migrationBuilder.DropColumn(
                name: "EDIId",
                table: "Metadatas");

            migrationBuilder.DropColumn(
                name: "EDIProcessed",
                table: "Metadatas");

            migrationBuilder.DropColumn(
                name: "EDIProvider",
                table: "Metadatas");

            migrationBuilder.DropColumn(
                name: "EDIState",
                table: "Metadatas");

            migrationBuilder.DropColumn(
                name: "EDIId",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "EDIProcessed",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "EDIProvider",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "EDIState",
                table: "Contracts");

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
