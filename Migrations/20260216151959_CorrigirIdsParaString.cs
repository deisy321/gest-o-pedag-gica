using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace gestaopedagogica.Migrations
{
    public partial class CorrigirIdsParaString : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Altera ProfessorId na tabela Trabalhos de int para string
            migrationBuilder.AlterColumn<string>(
                name: "ProfessorId",
                table: "Trabalhos",
                type: "text",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            // Altera AlunoId na tabela Trabalhos de int para string
            migrationBuilder.AlterColumn<string>(
                name: "AlunoId",
                table: "Trabalhos",
                type: "text",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reverte ProfessorId para int
            migrationBuilder.AlterColumn<int>(
                name: "ProfessorId",
                table: "Trabalhos",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            // Reverte AlunoId para int
            migrationBuilder.AlterColumn<int>(
                name: "AlunoId",
                table: "Trabalhos",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
