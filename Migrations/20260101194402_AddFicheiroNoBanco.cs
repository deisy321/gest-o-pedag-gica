using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace gestaopedagogica.Migrations
{
    /// <inheritdoc />
    public partial class AddFicheiroNoBanco : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FicheiroPath",
                table: "TrabalhoVertentes");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DataEnvio",
                table: "TrabalhoVertentes",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<byte[]>(
                name: "FicheiroBytes",
                table: "TrabalhoVertentes",
                type: "bytea",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FicheiroContentType",
                table: "TrabalhoVertentes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FicheiroNome",
                table: "TrabalhoVertentes",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FicheiroBytes",
                table: "TrabalhoVertentes");

            migrationBuilder.DropColumn(
                name: "FicheiroContentType",
                table: "TrabalhoVertentes");

            migrationBuilder.DropColumn(
                name: "FicheiroNome",
                table: "TrabalhoVertentes");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DataEnvio",
                table: "TrabalhoVertentes",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FicheiroPath",
                table: "TrabalhoVertentes",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
