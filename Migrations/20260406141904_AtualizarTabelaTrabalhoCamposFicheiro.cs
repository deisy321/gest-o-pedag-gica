using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace gestaopedagogica.Migrations
{
    /// <inheritdoc />
    public partial class AtualizarTabelaTrabalhoCamposFicheiro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DataEntrega",
                table: "Trabalhos",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "FicheiroBytes",
                table: "Trabalhos",
                type: "bytea",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FicheiroContentType",
                table: "Trabalhos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FicheiroNome",
                table: "Trabalhos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Nota",
                table: "Trabalhos",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "PushSubscriptions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataEntrega",
                table: "Trabalhos");

            migrationBuilder.DropColumn(
                name: "FicheiroBytes",
                table: "Trabalhos");

            migrationBuilder.DropColumn(
                name: "FicheiroContentType",
                table: "Trabalhos");

            migrationBuilder.DropColumn(
                name: "FicheiroNome",
                table: "Trabalhos");

            migrationBuilder.DropColumn(
                name: "Nota",
                table: "Trabalhos");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "PushSubscriptions");
        }
    }
}
