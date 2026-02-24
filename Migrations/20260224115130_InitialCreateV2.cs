using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace gestaopedagogica.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_turmamodulos_Modulos_moduloid",
                table: "turmamodulos");

            migrationBuilder.DropForeignKey(
                name: "FK_turmamodulos_Turmas_turmaid",
                table: "turmamodulos");

            migrationBuilder.DropForeignKey(
                name: "FK_Turmas_Cursos_CursoId",
                table: "Turmas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TurmaProfessores",
                table: "TurmaProfessores");

            migrationBuilder.DropPrimaryKey(
                name: "PK_turmamodulos",
                table: "turmamodulos");

            migrationBuilder.RenameTable(
                name: "turmamodulos",
                newName: "TurmaModulos");

            migrationBuilder.RenameColumn(
                name: "moduloid",
                table: "TurmaModulos",
                newName: "ModuloId");

            migrationBuilder.RenameColumn(
                name: "turmaid",
                table: "TurmaModulos",
                newName: "TurmaId");

            migrationBuilder.RenameIndex(
                name: "IX_turmamodulos_moduloid",
                table: "TurmaModulos",
                newName: "IX_TurmaModulos_ModuloId");

            migrationBuilder.AlterColumn<int>(
                name: "CursoId",
                table: "Turmas",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "DisciplinaId",
                table: "TurmaProfessores",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DisciplinaId",
                table: "Modulos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TurmaProfessores",
                table: "TurmaProfessores",
                columns: new[] { "TurmaId", "ProfessorId", "DisciplinaId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_TurmaModulos",
                table: "TurmaModulos",
                columns: new[] { "TurmaId", "ModuloId" });

            migrationBuilder.CreateTable(
                name: "Disciplinas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "text", nullable: false),
                    CursoId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Disciplinas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Disciplinas_Cursos_CursoId",
                        column: x => x.CursoId,
                        principalTable: "Cursos",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_TurmaProfessores_DisciplinaId",
                table: "TurmaProfessores",
                column: "DisciplinaId");

            migrationBuilder.CreateIndex(
                name: "IX_Modulos_DisciplinaId",
                table: "Modulos",
                column: "DisciplinaId");

            migrationBuilder.CreateIndex(
                name: "IX_Disciplinas_CursoId",
                table: "Disciplinas",
                column: "CursoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Modulos_Disciplinas_DisciplinaId",
                table: "Modulos",
                column: "DisciplinaId",
                principalTable: "Disciplinas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TurmaModulos_Modulos_ModuloId",
                table: "TurmaModulos",
                column: "ModuloId",
                principalTable: "Modulos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TurmaModulos_Turmas_TurmaId",
                table: "TurmaModulos",
                column: "TurmaId",
                principalTable: "Turmas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TurmaProfessores_Disciplinas_DisciplinaId",
                table: "TurmaProfessores",
                column: "DisciplinaId",
                principalTable: "Disciplinas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Turmas_Cursos_CursoId",
                table: "Turmas",
                column: "CursoId",
                principalTable: "Cursos",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Modulos_Disciplinas_DisciplinaId",
                table: "Modulos");

            migrationBuilder.DropForeignKey(
                name: "FK_TurmaModulos_Modulos_ModuloId",
                table: "TurmaModulos");

            migrationBuilder.DropForeignKey(
                name: "FK_TurmaModulos_Turmas_TurmaId",
                table: "TurmaModulos");

            migrationBuilder.DropForeignKey(
                name: "FK_TurmaProfessores_Disciplinas_DisciplinaId",
                table: "TurmaProfessores");

            migrationBuilder.DropForeignKey(
                name: "FK_Turmas_Cursos_CursoId",
                table: "Turmas");

            migrationBuilder.DropTable(
                name: "Disciplinas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TurmaProfessores",
                table: "TurmaProfessores");

            migrationBuilder.DropIndex(
                name: "IX_TurmaProfessores_DisciplinaId",
                table: "TurmaProfessores");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TurmaModulos",
                table: "TurmaModulos");

            migrationBuilder.DropIndex(
                name: "IX_Modulos_DisciplinaId",
                table: "Modulos");

            migrationBuilder.DropColumn(
                name: "DisciplinaId",
                table: "TurmaProfessores");

            migrationBuilder.DropColumn(
                name: "DisciplinaId",
                table: "Modulos");

            migrationBuilder.RenameTable(
                name: "TurmaModulos",
                newName: "turmamodulos");

            migrationBuilder.RenameColumn(
                name: "ModuloId",
                table: "turmamodulos",
                newName: "moduloid");

            migrationBuilder.RenameColumn(
                name: "TurmaId",
                table: "turmamodulos",
                newName: "turmaid");

            migrationBuilder.RenameIndex(
                name: "IX_TurmaModulos_ModuloId",
                table: "turmamodulos",
                newName: "IX_turmamodulos_moduloid");

            migrationBuilder.AlterColumn<int>(
                name: "CursoId",
                table: "Turmas",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TurmaProfessores",
                table: "TurmaProfessores",
                columns: new[] { "TurmaId", "ProfessorId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_turmamodulos",
                table: "turmamodulos",
                columns: new[] { "turmaid", "moduloid" });

            migrationBuilder.AddForeignKey(
                name: "FK_turmamodulos_Modulos_moduloid",
                table: "turmamodulos",
                column: "moduloid",
                principalTable: "Modulos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_turmamodulos_Turmas_turmaid",
                table: "turmamodulos",
                column: "turmaid",
                principalTable: "Turmas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Turmas_Cursos_CursoId",
                table: "Turmas",
                column: "CursoId",
                principalTable: "Cursos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
