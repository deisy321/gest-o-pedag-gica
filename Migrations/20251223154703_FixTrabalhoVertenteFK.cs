using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace gestaopedagogica.Migrations
{
    /// <inheritdoc />
    public partial class FixTrabalhoVertenteFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrabalhoVertentes_Trabalhos_TrabalhoId1",
                table: "TrabalhoVertentes");

            migrationBuilder.DropIndex(
                name: "IX_TrabalhoVertentes_TrabalhoId1",
                table: "TrabalhoVertentes");

            migrationBuilder.DropColumn(
                name: "TrabalhoId1",
                table: "TrabalhoVertentes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TrabalhoId1",
                table: "TrabalhoVertentes",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrabalhoVertentes_TrabalhoId1",
                table: "TrabalhoVertentes",
                column: "TrabalhoId1");

            migrationBuilder.AddForeignKey(
                name: "FK_TrabalhoVertentes_Trabalhos_TrabalhoId1",
                table: "TrabalhoVertentes",
                column: "TrabalhoId1",
                principalTable: "Trabalhos",
                principalColumn: "Id");
        }
    }
}
