using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DATABASE.Migrations
{
    public partial class fields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "AdditionalFields",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MetaId = table.Column<long>(nullable: false),
                    ContractId = table.Column<int>(nullable: false),
                    ClientId = table.Column<int>(nullable: false),
                    String1 = table.Column<string>(nullable: true),
                    String2 = table.Column<string>(nullable: true),
                    String3 = table.Column<string>(nullable: true),
                    String4 = table.Column<string>(nullable: true),
                    String5 = table.Column<string>(nullable: true),
                    Datetime1 = table.Column<DateTime>(nullable: false),
                    Datetime2 = table.Column<DateTime>(nullable: false),
                    Int1 = table.Column<int>(nullable: false),
                    Int2 = table.Column<int>(nullable: false),
                    Bool1 = table.Column<bool>(nullable: false),
                    Bool2 = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdditionalFields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdditionalFields_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AdditionalFieldsMappings",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ClientId = table.Column<int>(nullable: false),
                    FieldName = table.Column<string>(nullable: true),
                    FieldColumn = table.Column<string>(nullable: true),
                    FieldType = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdditionalFieldsMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdditionalFieldsMappings_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdditionalFields_ClientId",
                table: "AdditionalFields",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_AdditionalFieldsMappings_ClientId",
                table: "AdditionalFieldsMappings",
                column: "ClientId");

        var sql = @"
            CREATE VIEW ContractsExtended AS
                SELECT a.*, b.String1, b.String2, b.String3, b.String4, b.String5, b.Datetime1, b.Datetime2, b.Int1, b.Int2, b.Bool1, b.Bool2
FROM contracts a
left JOIN AdditionalFields b on b.ContractId = a.Id";
                migrationBuilder.Sql(sql);
         sql = @"
            CREATE VIEW MetadatasExtended AS
                SELECT a.*, b.String1, b.String2, b.String3, b.String4, b.String5, b.Datetime1, b.Datetime2, b.Int1, b.Int2, b.Bool1, b.Bool2
FROM metadatas a
left JOIN AdditionalFields b on b.MetaId = a.Id";
                migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        migrationBuilder.Sql(@"DROP VIEW ContractsExtended");
        migrationBuilder.Sql(@"DROP VIEW MetadatasExtended");
            migrationBuilder.DropTable(
                name: "AdditionalFields");

            migrationBuilder.DropTable(
                name: "AdditionalFieldsMappings");

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
