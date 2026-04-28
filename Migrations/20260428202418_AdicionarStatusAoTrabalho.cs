using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace gestaopedagogica.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarStatusAoTrabalho : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Trabalhos",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Trabalhos");
        }
    }
}
