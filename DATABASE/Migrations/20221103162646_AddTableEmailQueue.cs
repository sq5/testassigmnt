using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DATABASE.Migrations
{
    public partial class AddTableEmailQueue : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmailQueue",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Created = table.Column<DateTime>(nullable: false),
                    Sent = table.Column<DateTime>(nullable: true),
                    Recipients = table.Column<string>(nullable: false),
                    Subject = table.Column<string>(nullable: true),
                    Body = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailQueue", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmailQueueDocFile",
                columns: table => new
                {
                    EmailQueueId = table.Column<long>(nullable: false),
                    DocFileId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailQueueDocFile", x => new { x.EmailQueueId, x.DocFileId });
                    table.ForeignKey(
                        name: "FK_EmailQueueDocFile_Files_DocFileId",
                        column: x => x.DocFileId,
                        principalTable: "Files",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmailQueueDocFile_EmailQueue_EmailQueueId",
                        column: x => x.EmailQueueId,
                        principalTable: "EmailQueue",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmailQueue_Sent",
                table: "EmailQueue",
                column: "Sent");

            migrationBuilder.CreateIndex(
                name: "IX_EmailQueueDocFile_DocFileId",
                table: "EmailQueueDocFile",
                column: "DocFileId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailQueueDocFile");

            migrationBuilder.DropTable(
                name: "EmailQueue");

        }
    }
}
