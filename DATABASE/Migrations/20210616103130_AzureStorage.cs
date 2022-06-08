using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DATABASE.Migrations
{
    public partial class AzureStorage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BlobUrl",
                table: "Files",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AlterColumn<byte[]>(name: "FileBin", table: "Files", nullable: true);
        }
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BlobUrl",
                table: "Files");

            migrationBuilder.AlterColumn<byte[]>(name: "FileBin", table: "Files", nullable: false);
        }
    }
}
