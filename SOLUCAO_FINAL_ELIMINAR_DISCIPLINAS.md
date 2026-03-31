# ? SOLUÇÃO COMPLETA - Eliminar Disciplinas

## ?? Problema Resolvido

**Erro anterior:** "Não foi possível eliminar a disciplina. Verifique se existem turmas, módulos ou notas associadas a ela."

**Causa:** A tabela `TurmaProfessor` tinha uma chave estrangeira para `DisciplinaId` que não estava sendo removida antes de tentar eliminar a disciplina.

---

## ?? Solução Implementada

### 1. **Arquivo Modificado: `services\DisciplinaService.cs`**

**Método corrigido:** `DeleteDisciplinaAsync(int id)`

**Alterações:**
- ? Remove primeiro `TurmaProfessor` (chave estrangeira direta)
- ? Remove `Comentario` (vinculados aos trabalhos)
- ? Remove `TrabalhoVertente` (vertentes dos trabalhos)
- ? Remove `Trabalho` (vinculados aos módulos)
- ? Remove `TurmaModulo` (relação N:N)
- ? Remove `Modulo` (vinculados à disciplina)
- ? Remove `Disciplina` (finalmente)

**Ordem de Operações:**
```
1. TurmaProfessor ? Chave estrangeira direta
   ? SaveChanges (importante!)
2. Comentario (trabalhos)
3. TrabalhoVertente (trabalhos)
4. Trabalho (módulos)
5. TurmaModulo (módulos)
6. Modulo
7. Disciplina
   ? SaveChanges (final)
```

### 2. **Arquivo Modificado: `Pages\Admin\GerirDisciplinas.razor`**

**Mudança:** Método `ExecutarEliminacao()` com feedback melhorado

**Benefícios:**
- ? Mensagem de sucesso clara
- ? Mensagem de erro detalhada
- ? Sugere ação alternativa se falhar
- ? Log no console para debug

---

## ?? Como Testar

### ? Teste 1: Eliminar Disciplina sem Módulos
1. Criar uma disciplina nova
2. Ir para `/admin/GerirDisciplinas`
3. Clicar "Eliminar"
4. **Resultado esperado:** Elimina com sucesso

### ? Teste 2: Eliminar Disciplina com Módulos mas sem Trabalhos
1. Criar disciplina + módulos
2. Ir para `/admin/GerirDisciplinas`
3. Clicar "Eliminar"
4. **Resultado esperado:** Elimina com sucesso (remove tudo em cascata)

### ? Teste 3: Eliminar Disciplina com Professores Atribuídos
1. Criar disciplina + atribuir professor em turma
2. Ir para `/admin/GerirDisciplinas`
3. Clicar "Eliminar"
4. **Resultado esperado:** Elimina com sucesso (remove TurmaProfessor primeiro)

### ? Teste 4: Eliminar Disciplina com Trabalhos
1. Criar disciplina + módulo + trabalho com vertentes
2. Ir para `/admin/GerirDisciplinas`
3. Clicar "Eliminar"
4. **Resultado esperado:** Elimina com sucesso (cascata completa)

---

## ?? Fluxo de Eliminação Detalhado

```csharp
// O código agora faz isto:

try {
    1. Carrega disciplina com módulos
    
    2. Remove TurmaProfessor (FK: DisciplinaId)
       ?? SaveChanges() ? Crucial! Evita deadlock
    
  3. Se tem módulos:
       a. Encontra todos os Trabalho.Id para estes módulos
       b. Remove Comentario vinculados aos trabalhos
       c. Remove TrabalhoVertente vinculadas aos trabalhos
       d. Remove Trabalho vinculados aos módulos
     e. Remove TurmaModulo (relação M:N)
       f. Remove Modulo
    
    4. Remove Disciplina
    
    5. SaveChanges() ? Final
    
} catch (DbUpdateException) {
  ?? Log detalhado do erro PostgreSQL
} catch (Exception) {
    ?? Mensagem amigável ao utilizador
}
```

---

## ?? Comparação: ANTES vs DEPOIS

### ANTES ?
```csharp
// Sem RemoveRange para TurmaProfessor
// Sem separar os SaveChanges
// Erro: Foreign key constraint violated
```

### DEPOIS ?
```csharp
// 1. Remove TurmaProfessor primeiro
_context.TurmaProfessores.RemoveRange(atribuicoes);
await _context.SaveChangesAsync(); // ? Crucial!

// 2. Remove cascata de dependentes
// ... (comentarios, vertentes, trabalhos)

// 3. Remove módulos
_context.Modulos.RemoveRange(disciplina.Modulos);

// 4. Remove disciplina
_context.Disciplinas.Remove(disciplina);

// 5. Salva tudo junto
await _context.SaveChangesAsync();
```

---

## ?? Se Ainda Tiver Erro

### Cenário 1: "Cannot access file - IIS Express locked"
```
Solução: Parar o IIS Express
    Ctrl+Alt+P (ou View > Stop Debugging)
    Fazer Clean > Rebuild
```

### Cenário 2: "Foreign key constraint still fails"
```
Possível causa: Há uma chave estrangeira que não foi descoberta
Debug:
1. Abra PostgreSQL e verifique:
   SELECT * FROM information_schema.table_constraints 
   WHERE table_name = 'TurmaProfessores';
   
2. Verifique quais colunas têm FK:
   SELECT * FROM information_schema.key_column_usage 
   WHERE table_name = 'Disciplinas';
```

### Cenário 3: "Some records still exist in related table"
```
Debug no GerirDisciplinas.razor:
1. Verifique no console do browser (F12)
2. O erro deveria aparecer com detalhes
3. Procure qual tabela ainda tem dados
```

---

## ?? Arquivos Afetados

| Arquivo | Tipo | Mudança |
|---------|------|---------|
| `services\DisciplinaService.cs` | ?? Modificado | Método `DeleteDisciplinaAsync` reescrito |
| `Pages\Admin\GerirDisciplinas.razor` | ?? Modificado | Método `ExecutarEliminacao` com feedback |
| `GUIA_REMOCAO_DISCIPLINAS.md` | ?? Novo | Documentação técnica completa |

---

## ? Próximas Recomendações

### 1. Implementar Confirmação com Modal (em vez de alert)
```razor
<Modal @ref="modal">
    <h5>Confirmar Eliminação?</h5>
    <p>Isto removerá: @selectedDisciplina.Modulos.Count módulos</p>
    <button @onclick="ConfirmarEliminacao">Eliminar Tudo</button>
    <button @onclick="Cancelar">Cancelar</button>
</Modal>
```

### 2. Implementar Soft Delete
```csharp
public DateTime? DeletedAt { get; set; }
public string DeletedBy { get; set; }
```

### 3. Adicionar Auditoria
```csharp
var auditLog = new AuditLog {
 Entity = "Disciplina",
    Action = "DELETE",
    EntityId = id,
    Username = currentUser,
    Timestamp = DateTime.UtcNow
};
await _context.AuditLogs.AddAsync(auditLog);
```

### 4. Backup Automático
```csharp
await BackupService.CreateBackupAsync(disciplina, "before_delete");
```

---

## ?? Lições Aprendidas

1. **Ordem importa em Entity Framework** - Remova sempre as dependências primeiro
2. **SaveChanges após remover FKs** - Evita deadlocks com PostgreSQL
3. **Treat SaveChanges como "commit"** - Cada SaveChanges é uma transação
4. **Logs são essenciais** - Use Console.WriteLine e try-catch para debug

---

## ? Status

- ? Erro de FK resolvido
- ? Eliminação em cascata implementada
- ? Feedback visual melhorado
- ? Documentação completa
- ? Código compilado sem erros

**Pode agora eliminar disciplinas com sucesso! ??**

