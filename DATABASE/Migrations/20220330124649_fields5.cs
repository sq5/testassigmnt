using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DATABASE.Migrations
{
    public partial class fields5 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Bool3",
                table: "AdditionalFields",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Bool4",
                table: "AdditionalFields",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Datetime3",
                table: "AdditionalFields",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Datetime4",
                table: "AdditionalFields",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Int3",
                table: "AdditionalFields",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Int4",
                table: "AdditionalFields",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "String6",
                table: "AdditionalFields",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "String7",
                table: "AdditionalFields",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "String8",
                table: "AdditionalFields",
                nullable: true);
                
        var sql = @"
            ALTER VIEW ContractsExtended AS
                SELECT a.*, b.String1, b.String2, b.String3, b.String4, b.String5, b.String6, b.String7, b.String8, b.Datetime1, b.Datetime2, b.Datetime3, b.Datetime4, b.Int1, b.Int2, b.Int3, b.Int4, b.Bool1, b.Bool2, b.Bool3, b.Bool4
FROM contracts a
left JOIN AdditionalFields b on b.ContractId = a.Id";
                migrationBuilder.Sql(sql);
         sql = @"
            ALTER VIEW MetadatasExtended AS
                SELECT a.*, b.String1, b.String2, b.String3, b.String4, b.String5, b.String6, b.String7, b.String8, b.Datetime1, b.Datetime2, b.Datetime3, b.Datetime4, b.Int1, b.Int2, b.Int3, b.Int4, b.Bool1, b.Bool2, b.Bool3, b.Bool4
FROM metadatas a
left JOIN AdditionalFields b on b.MetaId = a.Id";
                migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Bool3",
                table: "AdditionalFields");

            migrationBuilder.DropColumn(
                name: "Bool4",
                table: "AdditionalFields");

            migrationBuilder.DropColumn(
                name: "Datetime3",
                table: "AdditionalFields");

            migrationBuilder.DropColumn(
                name: "Datetime4",
                table: "AdditionalFields");

            migrationBuilder.DropColumn(
                name: "Int3",
                table: "AdditionalFields");

            migrationBuilder.DropColumn(
                name: "Int4",
                table: "AdditionalFields");

            migrationBuilder.DropColumn(
                name: "String6",
                table: "AdditionalFields");

            migrationBuilder.DropColumn(
                name: "String7",
                table: "AdditionalFields");

            migrationBuilder.DropColumn(
                name: "String8",
                table: "AdditionalFields");
        }
    }
}
