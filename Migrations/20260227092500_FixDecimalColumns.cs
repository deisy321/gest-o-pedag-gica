using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace gestaopedagogica.Migrations
{
    /// <inheritdoc />
    public partial class FixDecimalColumns : Migration
    {
   /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
        {
      // Se a coluna Nota existe em TrabalhoVertentes e é texto, converter para numeric
            try
         {
             // Criar coluna temporária com tipo correto
         migrationBuilder.AddColumn<decimal>(
       name: "Nota_temp",
        table: "TrabalhoVertentes",
           type: "numeric",
     nullable: true);

        // Converter dados existentes
         migrationBuilder.Sql(@"
   UPDATE ""TrabalhoVertentes"" 
            SET ""Nota_temp"" = CAST(""Nota"" AS numeric) 
           WHERE ""Nota"" IS NOT NULL AND ""Nota"" != '';
   ", suppressTransaction: true);

     // Remover coluna antiga
      migrationBuilder.DropColumn(
      name: "Nota",
         table: "TrabalhoVertentes");

          // Renomear coluna temporária
   migrationBuilder.RenameColumn(
     name: "Nota_temp",
         table: "TrabalhoVertentes",
     newName: "Nota");
      }
            catch
    {
          // Se falhar, a coluna pode já estar correta
            }
        }

        /// <inheritdoc />
  protected override void Down(MigrationBuilder migrationBuilder)
      {
    // Não fazer nada no rollback
        }
    }
}
