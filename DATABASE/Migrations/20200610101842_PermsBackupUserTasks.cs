using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DATABASE.Migrations
{
    public partial class PermsBackupUserTasks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BackUpPassword",
                table: "Clients",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BackUpProvider",
                table: "Clients",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BackUpUser",
                table: "Clients",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "UnicPerms",
                table: "Clients",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ClientsTasks",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Created = table.Column<DateTime>(nullable: false),
                    Active = table.Column<bool>(nullable: false),
                    Task = table.Column<string>(maxLength: 50, nullable: false),
                    State = table.Column<string>(maxLength: 50, nullable: true),
                    StartDate = table.Column<DateTime>(nullable: true),
                    EndDate = table.Column<DateTime>(nullable: true),
                    ClientId = table.Column<int>(nullable: false),
                    Log = table.Column<string>(maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientsTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientsTasks_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReestrPerms",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    DeniedReestr = table.Column<string>(maxLength: 30, nullable: false),
                    User = table.Column<string>(maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReestrPerms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UsersTasks",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Active = table.Column<bool>(nullable: false),
                    Users = table.Column<string>(maxLength: 1000, nullable: false),
                    StartDate = table.Column<DateTime>(nullable: false),
                    EndDate = table.Column<DateTime>(nullable: true),
                    DeadLine = table.Column<DateTime>(nullable: true),
                    MetadataId = table.Column<long>(nullable: false),
                    ContractId = table.Column<int>(nullable: false),
                    Comment = table.Column<string>(maxLength: 500, nullable: true),
                    Resolution = table.Column<string>(maxLength: 20, nullable: true),
                    TaskType = table.Column<string>(maxLength: 20, nullable: true),
                    GeneralResolution = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsersTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsersTasks_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UsersTasks_Metadatas_MetadataId",
                        column: x => x.MetadataId,
                        principalTable: "Metadatas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClientsTasks_ClientId",
                table: "ClientsTasks",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_UsersTasks_ContractId",
                table: "UsersTasks",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_UsersTasks_MetadataId",
                table: "UsersTasks",
                column: "MetadataId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClientsTasks");

            migrationBuilder.DropTable(
                name: "ReestrPerms");

            migrationBuilder.DropTable(
                name: "UsersTasks");

            migrationBuilder.DropColumn(
                name: "BackUpPassword",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "BackUpProvider",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "BackUpUser",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "UnicPerms",
                table: "Clients");
        }
    }
}
