# ?? Guia de Remoção de Disciplinas - Hierarquia de Chaves Estrangeiras

## ?? Estrutura de Relacionamentos

```
Disciplina (1)
??? TurmaProfessor (N) ? Chave Estrangeira: DisciplinaId
??? Modulo (N)
?   ??? TurmaModulo (N)
?   ??? Trabalho (N)
?   ?   ??? TrabalhoVertente (N)
?   ? ??? Comentario (N)
?   ??? Professor (através de Modulo.ProfessorId)
```

## ?? Ordem de Eliminação (CRÍTICA)

Para eliminar uma **Disciplina** corretamente, deve-se remover na seguinte ordem:

### 1?? **TurmaProfessor** (Mais Alto na Hierarquia)
```sql
DELETE FROM "TurmaProfessores" WHERE "DisciplinaId" = @id
```
**Razão:** Esta tabela tem uma chave estrangeira direta para `Disciplina.Id`

### 2?? **Comentario** (Vinculado a Trabalho)
```sql
DELETE FROM "Comentarios" 
WHERE "TrabalhoId" IN (
  SELECT t."Id" FROM "Trabalhos" t
  WHERE t."ModuloId" IN (
    SELECT m."Id" FROM "Modulos" m
    WHERE m."DisciplinaId" = @id
  )
)
```

### 3?? **TrabalhoVertente** (Vinculado a Trabalho)
```sql
DELETE FROM "TrabalhoVertentes" 
WHERE "TrabalhoId" IN (
  SELECT t."Id" FROM "Trabalhos" t
  WHERE t."ModuloId" IN (
    SELECT m."Id" FROM "Modulos" m
    WHERE m."DisciplinaId" = @id
)
)
```

### 4?? **Trabalho** (Vinculado a Modulo)
```sql
DELETE FROM "Trabalhos" 
WHERE "ModuloId" IN (
  SELECT m."Id" FROM "Modulos" m
  WHERE m."DisciplinaId" = @id
)
```

### 5?? **TurmaModulo** (Relação N:N)
```sql
DELETE FROM "TurmaModulos" 
WHERE "ModuloId" IN (
  SELECT m."Id" FROM "Modulos" m
  WHERE m."DisciplinaId" = @id
)
```

### 6?? **Modulo** (Vinculado a Disciplina)
```sql
DELETE FROM "Modulos" WHERE "DisciplinaId" = @id
```

### 7?? **Disciplina** (Objetivo Final)
```sql
DELETE FROM "Disciplinas" WHERE "Id" = @id
```

---

## ? Solução Implementada (C#)

```csharp
public async Task DeleteDisciplinaAsync(int id)
{
    try
    {
        // 1. Carregar disciplina com módulos
        var disciplina = await _context.Disciplinas
    .Include(d => d.Modulos)
    .FirstOrDefaultAsync(d => d.Id == id);

        if (disciplina == null)
 throw new Exception("Disciplina não encontrada.");

      // 2. REMOVER PRIMEIRO: TurmaProfessor (referência direta a Disciplina)
        var atribuicoes = await _context.TurmaProfessores
 .Where(tp => tp.DisciplinaId == id)
            .ToListAsync();

        if (atribuicoes.Any())
        {
    _context.TurmaProfessores.RemoveRange(atribuicoes);
            await _context.SaveChangesAsync(); // Salva logo
        }

 // 3. Se houver módulos, remover todos os dependentes
        if (disciplina.Modulos?.Any() == true)
        {
       var moduloIds = disciplina.Modulos.Select(m => m.Id).ToList();

// Obter IDs dos trabalhos desta disciplina
        var trabalhoIds = await _context.Trabalhos
            .Where(t => t.ModuloId.HasValue && moduloIds.Contains(t.ModuloId.Value))
    .Select(t => t.Id)
       .ToListAsync();

            if (trabalhoIds.Any())
            {
    // Remover comentários
         var comentarios = await _context.Comentarios
      .Where(c => trabalhoIds.Contains(c.TrabalhoId))
       .ToListAsync();
      if (comentarios.Any())
      _context.Comentarios.RemoveRange(comentarios);

       // Remover vertentes
         var vertentes = await _context.TrabalhoVertentes
                  .Where(tv => trabalhoIds.Contains(tv.TrabalhoId))
                    .ToListAsync();
                if (vertentes.Any())
   _context.TrabalhoVertentes.RemoveRange(vertentes);

                // Remover trabalhos
     var trabalhos = await _context.Trabalhos
 .Where(t => trabalhoIds.Contains(t.Id))
          .ToListAsync();
                if (trabalhos.Any())
        _context.Trabalhos.RemoveRange(trabalhos);
            }

       // Remover TurmaModulo
  var turmaModulos = await _context.TurmaModulos
    .Where(tm => moduloIds.Contains(tm.ModuloId))
          .ToListAsync();
if (turmaModulos.Any())
           _context.TurmaModulos.RemoveRange(turmaModulos);

            // Remover módulos
        _context.Modulos.RemoveRange(disciplina.Modulos);
        }

 // 4. Remover a disciplina
        _context.Disciplinas.Remove(disciplina);

 // 5. Salvar tudo
 await _context.SaveChangesAsync();
    }
    catch (DbUpdateException dbEx)
    {
        throw new Exception("Erro de banco de dados: Verifique as referências ativas.", dbEx);
    }
}
```

---

## ?? Problemas Comuns e Soluções

### ? Erro: "Foreign key constraint violation"
**Causa:** Tentou remover uma linha que ainda tem referências.
**Solução:** Verifique a ordem de eliminação e certifique-se de que remove primeiro os dependentes.

### ? Erro: "Cannot delete or update a parent row"
**Causa:** Há uma chave estrangeira bloqueando a exclusão.
**Solução:** Use `PRAGMA foreign_keys = OFF;` temporariamente (?? Cuidado!) ou remova os filhos primeiro.

### ? Erro: "The process cannot access the file"
**Causa:** IIS Express tem o arquivo travado.
**Solução:** Reinicie o IIS Express ou faça Build > Clean > Rebuild.

---

## ?? Debug: Verificar Relacionamentos no PostgreSQL

```sql
-- Ver todas as foreign keys
SELECT constraint_name, table_name, column_name 
FROM information_schema.key_column_usage 
WHERE referenced_table_name = 'Disciplinas';

-- Ver dados específicos
SELECT * FROM "TurmaProfessores" WHERE "DisciplinaId" = @id;
SELECT * FROM "Modulos" WHERE "DisciplinaId" = @id;

-- Contar referências
SELECT COUNT(*) FROM "TurmaProfessores" WHERE "DisciplinaId" = @id;
```

---

## ? Melhorias Futuras

1. **Soft Delete**: Marca como deletado em vez de remover
   ```csharp
   public DateTime? DeletedAt { get; set; }
   ```

2. **Cascade Delete**: Deixe o EF Core fazer a cascata automática
   ```csharp
   .OnDelete(DeleteBehavior.Cascade);
   ```

3. **Auditoria**: Registar quem removeu o quê e quando
   ```csharp
   public string DeletedBy { get; set; }
   public DateTime DeletedAt { get; set; }
   ```

4. **Backup Automático**: Fazer backup antes de remover dados críticos
   ```csharp
   await BackupService.CreateBackupAsync(disciplina);
   ```

