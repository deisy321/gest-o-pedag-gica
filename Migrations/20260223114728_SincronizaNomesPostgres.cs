using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace gestaopedagogica.Migrations
{
    public partial class SincronizaNomesPostgres : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. COMENTADO: Ignorar erros de chaves que não existem
            /*
            migrationBuilder.DropForeignKey(name: "FK_Alunos_AspNetUsers_UserId", table: "Alunos");
            migrationBuilder.DropForeignKey(name: "FK_Alunos_Turmas_TurmaId", table: "Alunos");
            migrationBuilder.DropIndex(name: "IX_Alunos_UserId", table: "Alunos");
            */

            // 2. CRIAÇÃO DAS COLUNAS: 
            // Estas três já foram criadas na execução anterior antes do erro do "Numero"
            // Se o comando falhar novamente nelas, comente-as também.
            migrationBuilder.AddColumn<string>(
                name: "CriterioAptidao",
                table: "Modulos",
                type: "text",
                nullable: false,
                defaultValue: "Pendente");

            migrationBuilder.AddColumn<string>(
                name: "CriterioCompetencia",
                table: "Modulos",
                type: "text",
                nullable: false,
                defaultValue: "Pendente");

            migrationBuilder.AddColumn<string>(
                name: "CriterioConhecimento",
                table: "Modulos",
                type: "text",
                nullable: false,
                defaultValue: "Pendente");

            // AJUSTE: COMENTADO porque o Postgres diz que esta coluna JÁ EXISTE (Erro 42701)
            /*
            migrationBuilder.AddColumn<string>(
                name: "Numero",
                table: "Modulos",
                type: "text",
                nullable: false,
                defaultValue: "0");
            */

            // 3. TABELA DE LIGAÇÃO: Criar se não existir
            /*
            migrationBuilder.CreateTable(
                name: "turmamodulos",
                columns: table => new
                {
                    turmaid = table.Column<int>(type: "integer", nullable: false),
                    moduloid = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_turmamodulos", x => new { x.turmaid, x.moduloid });
                });
            */
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Pode ficar vazio para esta correção de sincronização
        }
    }
}
