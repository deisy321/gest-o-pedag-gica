using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace gestaopedagogica.Migrations
{
    /// <inheritdoc />
    public partial class CorrigirRelacionamentoTrabalhoVertente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfessorId",
                table: "Modulos");

            migrationBuilder.DropColumn(
                name: "ProfessorName",
                table: "Modulos");

            migrationBuilder.DropColumn(
                name: "DataNascimento",
                table: "Alunos");

            migrationBuilder.DropColumn(
                name: "NumeroAluno",
                table: "Alunos");

            migrationBuilder.AddColumn<string>(
                name: "Ano",
                table: "Turmas",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProfessorResponsavel",
                table: "Turmas",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "TrabalhoId1",
                table: "TrabalhoVertentes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Professor",
                table: "Modulos",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "TurmaId",
                table: "Alunos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Cursos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "text", nullable: false),
                    Descricao = table.Column<string>(type: "text", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cursos", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrabalhoVertentes_TrabalhoId1",
                table: "TrabalhoVertentes",
                column: "TrabalhoId1");

            migrationBuilder.CreateIndex(
                name: "IX_Alunos_TurmaId",
                table: "Alunos",
                column: "TurmaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Alunos_Turmas_TurmaId",
                table: "Alunos",
                column: "TurmaId",
                principalTable: "Turmas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TrabalhoVertentes_Trabalhos_TrabalhoId1",
                table: "TrabalhoVertentes",
                column: "TrabalhoId1",
                principalTable: "Trabalhos",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Alunos_Turmas_TurmaId",
                table: "Alunos");

            migrationBuilder.DropForeignKey(
                name: "FK_TrabalhoVertentes_Trabalhos_TrabalhoId1",
                table: "TrabalhoVertentes");

            migrationBuilder.DropTable(
                name: "Cursos");

            migrationBuilder.DropIndex(
                name: "IX_TrabalhoVertentes_TrabalhoId1",
                table: "TrabalhoVertentes");

            migrationBuilder.DropIndex(
                name: "IX_Alunos_TurmaId",
                table: "Alunos");

            migrationBuilder.DropColumn(
                name: "Ano",
                table: "Turmas");

            migrationBuilder.DropColumn(
                name: "ProfessorResponsavel",
                table: "Turmas");

            migrationBuilder.DropColumn(
                name: "TrabalhoId1",
                table: "TrabalhoVertentes");

            migrationBuilder.DropColumn(
                name: "Professor",
                table: "Modulos");

            migrationBuilder.DropColumn(
                name: "TurmaId",
                table: "Alunos");

            migrationBuilder.AddColumn<string>(
                name: "ProfessorId",
                table: "Modulos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProfessorName",
                table: "Modulos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataNascimento",
                table: "Alunos",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "NumeroAluno",
                table: "Alunos",
                type: "text",
                nullable: true);
        }
    }
}
