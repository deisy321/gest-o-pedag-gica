using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace gestaopedagogica.Migrations
{
    /// <inheritdoc />
    public partial class RemoverTurmaDoIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Turmas_TurmaId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_TurmaId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "TurmaId",
                table: "AspNetUsers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TurmaId",
                table: "AspNetUsers",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_TurmaId",
                table: "AspNetUsers",
                column: "TurmaId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Turmas_TurmaId",
                table: "AspNetUsers",
                column: "TurmaId",
                principalTable: "Turmas",
                principalColumn: "Id");
        }
    }
}
