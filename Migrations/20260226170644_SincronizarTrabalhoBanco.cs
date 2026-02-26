using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace gestaopedagogica.Migrations
{
    /// <inheritdoc />
    public partial class SincronizarTrabalhoBanco : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DisciplinaId",
                table: "Trabalhos",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ModuloId",
                table: "Trabalhos",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TurmaId",
                table: "Trabalhos",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Comentarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TrabalhoId = table.Column<int>(type: "integer", nullable: false),
                    AutorId = table.Column<string>(type: "text", nullable: true),
                    Conteudo = table.Column<string>(type: "text", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    ComentarioPaiId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comentarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comentarios_AspNetUsers_AutorId",
                        column: x => x.AutorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Comentarios_Comentarios_ComentarioPaiId",
                        column: x => x.ComentarioPaiId,
                        principalTable: "Comentarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Comentarios_Trabalhos_TrabalhoId",
                        column: x => x.TrabalhoId,
                        principalTable: "Trabalhos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Trabalhos_DisciplinaId",
                table: "Trabalhos",
                column: "DisciplinaId");

            migrationBuilder.CreateIndex(
                name: "IX_Trabalhos_ModuloId",
                table: "Trabalhos",
                column: "ModuloId");

            migrationBuilder.CreateIndex(
                name: "IX_Trabalhos_TurmaId",
                table: "Trabalhos",
                column: "TurmaId");

            migrationBuilder.CreateIndex(
                name: "IX_Comentarios_AutorId",
                table: "Comentarios",
                column: "AutorId");

            migrationBuilder.CreateIndex(
                name: "IX_Comentarios_ComentarioPaiId",
                table: "Comentarios",
                column: "ComentarioPaiId");

            migrationBuilder.CreateIndex(
                name: "IX_Comentarios_TrabalhoId",
                table: "Comentarios",
                column: "TrabalhoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Trabalhos_Disciplinas_DisciplinaId",
                table: "Trabalhos",
                column: "DisciplinaId",
                principalTable: "Disciplinas",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Trabalhos_Modulos_ModuloId",
                table: "Trabalhos",
                column: "ModuloId",
                principalTable: "Modulos",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Trabalhos_Turmas_TurmaId",
                table: "Trabalhos",
                column: "TurmaId",
                principalTable: "Turmas",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Trabalhos_Disciplinas_DisciplinaId",
                table: "Trabalhos");

            migrationBuilder.DropForeignKey(
                name: "FK_Trabalhos_Modulos_ModuloId",
                table: "Trabalhos");

            migrationBuilder.DropForeignKey(
                name: "FK_Trabalhos_Turmas_TurmaId",
                table: "Trabalhos");

            migrationBuilder.DropTable(
                name: "Comentarios");

            migrationBuilder.DropIndex(
                name: "IX_Trabalhos_DisciplinaId",
                table: "Trabalhos");

            migrationBuilder.DropIndex(
                name: "IX_Trabalhos_ModuloId",
                table: "Trabalhos");

            migrationBuilder.DropIndex(
                name: "IX_Trabalhos_TurmaId",
                table: "Trabalhos");

            migrationBuilder.DropColumn(
                name: "DisciplinaId",
                table: "Trabalhos");

            migrationBuilder.DropColumn(
                name: "ModuloId",
                table: "Trabalhos");

            migrationBuilder.DropColumn(
                name: "TurmaId",
                table: "Trabalhos");
        }
    }
}
