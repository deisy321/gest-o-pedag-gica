# ?? GUIA RÁPIDO - APLICAR ESTILOS EM TODAS AS PÁGINAS

## ? Método Rápido (Copiar & Colar)

### Passo 1: Adicionar ao Topo
```razor
@layout MainLayout
```

### Passo 2: Envolver Conteúdo
```razor
<div class="page-admin">  <!-- ou page-student, page-teacher -->
    <!-- SEU CONTEÚDO AQUI -->
</div>
```

### Passo 3: Adicionar Cabeçalho
```razor
<div class="admin-header-row">
    <div>
   <h1 class="page-title">Título da Página</h1>
        <p class="page-subtitle">Descrição breve</p>
    </div>
    <div class="d-flex gap-2">
        <!-- Botões aqui -->
    </div>
</div>
```

### Passo 4: Substituir Cards/Tabelas

#### ANTES (Antigo)
```html
<div class="card shadow-sm">
    <div class="card-header">
        <h5>Título</h5>
    </div>
    <div class="card-body">
        <!-- Conteúdo -->
  </div>
</div>
```

#### DEPOIS (Novo - Moderno)
```html
<div class="custom-card elevation-2">
    <div class="card-header-custom">
    <h5>Título</h5>
    </div>
    <div class="card-body-custom">
   <!-- Conteúdo -->
    </div>
</div>
```

### Passo 5: Substituir Tabelas

#### ANTES
```html
<table class="table table-hover">
    <!-- Linhas -->
</table>
```

#### DEPOIS
```html
<div style="overflow-x: auto;">
    <table class="custom-table">
    <!-- Linhas -->
    </table>
</div>
```

### Passo 6: Substituir Botões

#### ANTES
```html
<button class="btn btn-primary">Botão</button>
<button class="btn btn-secondary">Botão</button>
```

#### DEPOIS
```html
<button class="btn btn-custom btn-custom-primary">Botão</button>
<button class="btn btn-custom btn-custom-secondary">Botão</button>
<button class="btn btn-custom btn-custom-success">Botão</button>
<button class="btn btn-custom btn-custom-danger">Botão</button>
<button class="btn btn-custom btn-custom-outline">Botão</button>
```

### Passo 7: Substituir Alertas

#### ANTES
```html
<div class="alert alert-success">Mensagem</div>
<div class="alert alert-danger">Erro</div>
```

#### DEPOIS
```html
<div class="alert-custom alert-success">
    <i class="bi bi-check-circle-fill"></i>
    <div>Mensagem de sucesso</div>
</div>
<div class="alert-custom alert-danger">
    <i class="bi bi-exclamation-triangle-fill"></i>
    <div>Mensagem de erro</div>
</div>
```

---

## ?? Checklist Rápida para Cada Página

```
Página: ________________________

- [ ] `@layout MainLayout` adicionado
- [ ] `<div class="page-admin/student/teacher">` na raiz
- [ ] `.admin-header-row` com títulos e botões
- [ ] `.custom-card elevation-2` em cards principais
- [ ] `.custom-table` em tabelas
- [ ] `.btn btn-custom btn-custom-*` em botões
- [ ] `.alert-custom` em alertas
- [ ] `.form-control` em inputs
- [ ] `.form-select` em selects
- [ ] Testado em mobile ?
```

---

## ?? Cores para Botões

| Uso | Classe | Cor |
|-----|--------|-----|
| Ação Principal | `btn-custom-primary` | ?? Azul |
| Ação Secundária | `btn-custom-secondary` | ?? Teal |
| Sucesso/Criar | `btn-custom-success` | ?? Verde |
| Perigo/Deletar | `btn-custom-danger` | ?? Vermelho |
| Outline/Voltar | `btn-custom-outline` | ? Transparente |

---

## ?? Cores para Cards de Stats

```html
<!-- Azul -->
<div class="stat-card card-blue">
    <i class="bi bi-people stat-card-icon"></i>
    <div class="stat-card-content">
        <h3>Total de Alunos</h3>
        <p>@count</p>
    </div>
</div>

<!-- Verde -->
<div class="stat-card card-green">
    <i class="bi bi-check stat-card-icon"></i>
    <div class="stat-card-content">
        <h3>Concluído</h3>
        <p>@count</p>
    </div>
</div>

<!-- Amarelo -->
<div class="stat-card card-yellow">
    <i class="bi bi-clock stat-card-icon"></i>
    <div class="stat-card-content">
 <h3>Pendente</h3>
        <p>@count</p>
    </div>
</div>

<!-- Vermelho -->
<div class="stat-card card-red">
    <i class="bi bi-exclamation stat-card-icon"></i>
    <div class="stat-card-content">
    <h3>Erro</h3>
   <p>@count</p>
    </div>
</div>
```

---

## ?? Localizar e Substituir (Visual Studio)

### Ctrl+H para abrir Find & Replace

#### 1. Botões Bootstrap ? Botões Modernos
**Find:** `class="btn btn-primary"`
**Replace:** `class="btn btn-custom btn-custom-primary"`

#### 2. Cards Bootstrap ? Cards Modernos
**Find:** `<div class="card`
**Replace:** `<div class="custom-card elevation-2`

#### 3. Alertas Bootstrap ? Alertas Modernos
**Find:** `<div class="alert alert-success`
**Replace:** `<div class="alert-custom alert-success`

#### 4. Tabelas Bootstrap ? Tabelas Modernas
**Find:** `<table class="table`
**Replace:** `<table class="custom-table`

---

## ?? Testar Responsividade

1. Abra a página no navegador
2. Pressione `F12` (DevTools)
3. Clique em `?? Toggle device toolbar`
4. Teste em:
   - iPhone 12/13
   - Galaxy S20/S21
   - iPad
   - Desktop 1920px

---

## ?? Problemas Comuns

### ? Botão não funciona
```html
<!-- ERRADO -->
<button class="btn btn-primary">Botão</button>

<!-- CERTO -->
<button class="btn btn-custom btn-custom-primary">Botão</button>
```

### ? Tabela não responsiva
```html
<!-- ERRADO -->
<table class="table">

<!-- CERTO -->
<div style="overflow-x: auto;">
    <table class="custom-table">
</div>
```

### ? Card não tem sombra
```html
<!-- ERRADO -->
<div class="card">

<!-- CERTO -->
<div class="custom-card elevation-2">
```

### ? Alerta sem ícone
```html
<!-- ERRADO -->
<div class="alert alert-success">Mensagem</div>

<!-- CERTO -->
<div class="alert-custom alert-success">
    <i class="bi bi-check-circle-fill"></i>
    <div>Mensagem</div>
</div>
```

---

## ? Templates Prontos para Copiar

### Template 1: Lista/Tabela
```razor
<div class="page-admin">
    <div class="admin-header-row">
        <div>
            <h1 class="page-title">Minha Lista</h1>
        <p class="page-subtitle">Visualize e gerencie itens</p>
        </div>
    <button class="btn btn-custom btn-custom-primary">
        <i class="bi bi-plus-lg"></i> Novo Item
        </button>
    </div>

    <div class="custom-card elevation-2">
   <div class="card-header-custom">
    <h5>Itens</h5>
        </div>
  <div style="overflow-x: auto;">
            <table class="custom-table">
           <thead>
       <tr>
    <th>Nome</th>
      <th>Status</th>
              <th class="text-center">Ações</th>
     </tr>
         </thead>
      <tbody>
            @foreach (var item in items)
       {
    <tr>
        <td>@item.Nome</td>
           <td>@item.Status</td>
   <td class="text-center">
      <button class="btn btn-custom btn-custom-primary" style="padding: 6px 12px;">
              <i class="bi bi-eye"></i>
         </button>
             </td>
       </tr>
                }
     </tbody>
            </table>
     </div>
    </div>
</div>
```

### Template 2: Formulário
```razor
<div class="page-admin">
    <div class="admin-header-row">
        <div>
            <h1 class="page-title">Criar Novo</h1>
     <p class="page-subtitle">Preencha o formulário</p>
  </div>
        <a href="/volta" class="btn btn-custom btn-custom-outline">
     <i class="bi bi-arrow-left"></i> Voltar
        </a>
    </div>

    <div class="custom-card elevation-2" style="max-width: 600px; margin: 0 auto;">
        <div class="card-body-custom">
            <EditForm Model="item" OnValidSubmit="Salvar">
          <div class="form-group">
  <label class="form-label">Nome</label>
        <input class="form-control" @bind="item.Nome" />
       </div>

    <div class="form-group">
   <label class="form-label">Descrição</label>
           <textarea class="form-control" @bind="item.Descricao" rows="3"></textarea>
            </div>

        <div class="d-flex gap-2" style="margin-top: 2rem;">
          <a href="/volta" class="btn btn-custom btn-custom-outline" style="flex: 1;">
           <i class="bi bi-x"></i> Cancelar
          </a>
          <button type="submit" class="btn btn-custom btn-custom-success" style="flex: 1;">
                 <i class="bi bi-check2"></i> Salvar
               </button>
    </div>
         </EditForm>
        </div>
    </div>
</div>
```

### Template 3: Stats + Detalhes
```razor
<div class="page-admin">
    <div class="admin-header-row">
        <div>
   <h1 class="page-title">Dashboard</h1>
       <p class="page-subtitle">Visualize suas estatísticas</p>
        </div>
    </div>

 <div class="row g-3" style="margin-bottom: 2rem;">
        <div class="col-md-3">
     <div class="stat-card card-blue">
     <i class="bi bi-people stat-card-icon"></i>
 <div class="stat-card-content">
          <h3>Total</h3>
       <p>@total</p>
 </div>
            </div>
    </div>
    </div>

    <div class="custom-card elevation-2">
   <div class="card-header-custom">
     <h5>Detalhes</h5>
    </div>
        <div class="card-body-custom">
            <!-- Conteúdo -->
    </div>
    </div>
</div>
```

---

## ?? Ícones Disponíveis

Use qualquer ícone da [Bootstrap Icons](https://icons.getbootstrap.com/):

```html
<i class="bi bi-home"></i><!-- Casa -->
<i class="bi bi-people"></i>     <!-- Pessoas -->
<i class="bi bi-file-text"></i>     <!-- Arquivo -->
<i class="bi bi-calendar"></i>      <!-- Calendário -->
<i class="bi bi-plus-lg"></i> <!-- Mais -->
<i class="bi bi-pencil"></i>        <!-- Editar -->
<i class="bi bi-trash"></i>    <!-- Deletar -->
<i class="bi bi-eye"></i> <!-- Ver -->
<i class="bi bi-check-circle"></i>  <!-- Sucesso -->
<i class="bi bi-exclamation-triangle"></i>  <!-- Aviso -->
```

---

## ? Verificação Final

Antes de fazer commit:

- [ ] Todos `@layout MainLayout` presentes?
- [ ] Todos pages envolvidos em `<div class="page-*">`?
- [ ] Tabelas com `.custom-table`?
- [ ] Botões com `.btn-custom`?
- [ ] Testado em mobile?
- [ ] Build sem erros?
- [ ] Git commit feito?

---

## ?? Resumo

| O Que | Antes | Depois |
|------|-------|--------|
| Layout | Container-fluid | `page-admin/student/teacher` |
| Cards | `.card` | `.custom-card elevation-2` |
| Tabelas | `.table` | `.custom-table` |
| Botões | `.btn-primary` | `.btn-custom btn-custom-primary` |
| Alertas | `.alert` | `.alert-custom alert-success` |
| Estilo | Bootstrap padrão | Profissional moderno |
| Mobile | Ruim | 100% responsivo |

---

**Dúvidas? Consulte `ATUALIZACAO_TODAS_PAGES.md`**

**Bom trabalho! ??**

