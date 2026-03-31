# ? CHECKLIST PRÁTICO - Testar Eliminação de Disciplina

## ?? Passos para Testar

### 1?? **Preparação**
- [ ] Parar o IIS Express (se necessário)
- [ ] Fazer `dotnet build --no-restore` 
- [ ] Iniciar a aplicação
- [ ] Login como Administrador
- [ ] Navegue para `/admin/GerirDisciplinas`

### 2?? **Teste 1: Eliminar Disciplina Vazia (Sem Módulos)**

**Setup:**
```
1. Ir para /admin/CriarDisciplina
2. Nome: "Teste Vazio"
3. Selecionar um curso
4. Criar
```

**Teste:**
```
1. Voltar para /admin/GerirDisciplinas
2. Procurar "Teste Vazio"
3. Clicar "Eliminar"
4. Confirmar no diálogo
```

**Resultado esperado:** ? Mensagem "Disciplina eliminada com sucesso!"

---

### 3?? **Teste 2: Eliminar Disciplina com Módulos (Sem Trabalhos)**

**Setup:**
```
1. Ir para /admin/CriarDisciplina
   Nome: "Teste com Módulos"
2. Criar
3. Ir para /admin/GerirModulos
4. Clicar "Novo Módulo"
5. Nome: "Módulo Teste"
   Disciplina: "Teste com Módulos"
6. Criar
```

**Teste:**
```
1. Voltar para /admin/GerirDisciplinas
2. Procurar "Teste com Módulos"
3. Clicar "Eliminar"
4. Confirmar
```

**Resultado esperado:** ? Mensagem "Disciplina eliminada com sucesso!"
**O módulo também deve ser eliminado**

---

### 4?? **Teste 3: Eliminar Disciplina com Professor Atribuído**

**Setup:**
```
1. Ir para /admin/GerirTurmas
2. Clicar "Nova Turma"
3. Escolher um curso
4. Nome: "Turma Teste"
5. Ano: "2024"
6. Adicionar Professor:
   - Professor: (selecione um)
   - Disciplina: (crie uma new "Teste Professor")
7. Criar turma
```

**Teste:**
```
1. Ir para /admin/GerirDisciplinas
2. Procurar "Teste Professor"
3. Clicar "Eliminar"
4. Confirmar
```

**Resultado esperado:** ? Mensagem "Disciplina eliminada com sucesso!"
**A atribuição de professor também deve ser removida**

---

### 5?? **Teste 4: Teste Completo (Com Tudo)**

**Setup:**
```
1. Criar Disciplina "Teste Completo"
2. Criar Módulo "Mod 1" vinculado a essa disciplina
3. Criar Turma "Turma X"
4. Adicionar Professor com "Teste Completo"
5. Adicionar Módulo "Mod 1" à turma
6. (Opcionalmente) Criar um Trabalho para "Mod 1"
```

**Teste:**
```
1. Ir para /admin/GerirDisciplinas
2. Procurar "Teste Completo"
3. Clicar "Eliminar"
4. Confirmar
```

**Resultado esperado:** ? Mensagem "Disciplina eliminada com sucesso!"
**Deve remover tudo em cascata:**
- [ ] TurmaProfessor
- [ ] TurmaModulo (se havia)
- [ ] Trabalho (se havia)
- [ ] Modulo
- [ ] Disciplina

---

## ?? Debug: Se Algo Falhar

### ? Erro Recebido: "Foreign key constraint violation"

**Ação 1:**
```
1. Abra o Browser DevTools (F12)
2. Vá para Console
3. Procure pela mensagem de erro completa
4. Screenshot do erro
```

**Ação 2:**
```
1. Abra PostgreSQL (ou pgAdmin)
2. Execute:
   SELECT * FROM "TurmaProfessores" 
   WHERE "DisciplinaId" = (
     SELECT "Id" FROM "Disciplinas" 
     WHERE "Nome" = 'Teste Completo'
   );
```

**Ação 3:**
```
Se encontrou registos, significa que:
- O DeleteDisciplinaAsync não removeu corretamente
- Verifique se TurmaProfessor tem dados órfãos
```

---

## ?? Notas de Teste

**Escreva aqui o resultado de cada teste:**

### Teste 1: Disciplina Vazia
```
Data: ___________
Resultado: [ ] Sucesso [ ] Falha
Erro (se houver): ___________________
```

### Teste 2: Com Módulos
```
Data: ___________
Resultado: [ ] Sucesso [ ] Falha
Erro (se houver): ___________________
```

### Teste 3: Com Professor
```
Data: ___________
Resultado: [ ] Sucesso [ ] Falha
Erro (se houver): ___________________
```

### Teste 4: Completo
```
Data: ___________
Resultado: [ ] Sucesso [ ] Falha
Erro (se houver): ___________________
```

---

## ?? Critério de Sucesso

- ? Todos os 4 testes passam sem erro
- ? Mensagem de sucesso aparece após cada eliminação
- ? Disciplina desaparece da lista
- ? Não há dados órfãos no banco de dados

---

## ?? Suporte

Se algo falhar:

1. **Verifique os logs:**
   ```
   Browser Console (F12) ? Console tab
   Visual Studio ? Output window
   ```

2. **Limpe o projeto:**
   ```
   Build ? Clean Solution
   Build ? Rebuild Solution
   ```

3. **Resete a base de dados (último recurso):**
   ```
   dotnet ef database drop
   dotnet ef database update
   ```

---

## ? Após Sucesso

Quando todos os testes forem bem-sucedidos:

- [ ] Commit as mudanças ao Git
- [ ] Documente quaisquer problemas encontrados
- [ ] Teste com dados reais
- [ ] Informe o utilizador sobre a nova funcionalidade

**Parabéns! ?? A funcionalidade está pronta!**

