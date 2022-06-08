using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DATABASE.Migrations
{
    public partial class TasksUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GeneralResolution",
                table: "UsersTasks");

            migrationBuilder.AlterColumn<string>(
                name: "Users",
                table: "UsersTasks",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(1000) CHARACTER SET utf8mb4",
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<string>(
                name: "TaskType",
                table: "UsersTasks",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(20) CHARACTER SET utf8mb4",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartDate",
                table: "UsersTasks",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)");

            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "UsersTasks",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "UsersTasks",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Stage",
                table: "UsersTasks",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "TaskText",
                table: "UsersTasks",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "State",
                table: "Metadatas",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "State",
                table: "Contracts",
                maxLength: 50,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Created",
                table: "UsersTasks");

            migrationBuilder.DropColumn(
                name: "Order",
                table: "UsersTasks");

            migrationBuilder.DropColumn(
                name: "Stage",
                table: "UsersTasks");

            migrationBuilder.DropColumn(
                name: "TaskText",
                table: "UsersTasks");

            migrationBuilder.DropColumn(
                name: "State",
                table: "Metadatas");

            migrationBuilder.DropColumn(
                name: "State",
                table: "Contracts");

            migrationBuilder.AlterColumn<string>(
                name: "Users",
                table: "UsersTasks",
                type: "varchar(1000) CHARACTER SET utf8mb4",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "TaskType",
                table: "UsersTasks",
                type: "varchar(20) CHARACTER SET utf8mb4",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartDate",
                table: "UsersTasks",
                type: "datetime(6)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "GeneralResolution",
                table: "UsersTasks",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }
    }
}
