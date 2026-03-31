# ?? GUIA DE CLASSES CSS - APLICAR EM TODAS AS PAGES

## ?? Layout Geral

```html
<!-- Div Principal (TODAS as pages devem ter uma dessas) -->
<div class="page-admin">      <!-- Para pages de Admin -->
<div class="page-student">    <!-- Para pages de Aluno -->
<div class="page-teacher">    <!-- Para pages de Professor -->
```

## ?? Padrão de Cabeçalho

```html
<div class="admin-header-row">
    <div>
     <h1 class="page-title">Título da Página</h1>
      <p class="page-subtitle">Subtítulo ou descrição</p>
    </div>
    <div class="d-flex gap-2" style="flex-wrap: wrap;">
    <a href="..." class="btn btn-custom btn-custom-outline">
  <i class="bi bi-arrow-left"></i> Voltar
    </a>
        <button class="btn btn-custom btn-custom-primary" @onclick="...">
 <i class="bi bi-plus-lg"></i> Novo Item
        </button>
    </div>
</div>
```

## ?? Cards

```html
<!-- Card Principal -->
<div class="custom-card elevation-2">
    <div class="card-body-custom">
        <!-- Conteúdo -->
    </div>
</div>

<!-- Card com Cabeçalho -->
<div class="custom-card elevation-2">
    <div class="card-header-custom">
     <h5>Título do Card</h5>
    </div>
    <div class="card-body-custom">
        <!-- Conteúdo -->
 </div>
</div>

<!-- Card com Rodapé -->
<div class="custom-card">
    <div class="card-body-custom">
        <!-- Conteúdo -->
    </div>
    <div class="card-footer-custom">
 <!-- Ações -->
    </div>
</div>
```

## ?? Botões

```html
<!-- Botões Customizados -->
<button class="btn btn-custom btn-custom-primary">Primário</button>
<button class="btn btn-custom btn-custom-secondary">Secundário</button>
<button class="btn btn-custom btn-custom-success">Sucesso</button>
<button class="btn btn-custom btn-custom-danger">Perigo</button>
<button class="btn btn-custom btn-custom-outline">Outline</button>

<!-- QA Items (Estilos especiais) -->
<button class="qa-item purple">Purple</button>
<button class="qa-item green">Green</button>
<button class="qa-item blue">Blue</button>
<button class="qa-item dark-blue">Dark Blue</button>
```

## ?? Tabelas

```html
<div style="overflow-x: auto;">
    <table class="custom-table">
        <thead>
         <tr>
   <th>Coluna 1</th>
        <th>Coluna 2</th>
    <th class="text-center">Ações</th>
         </tr>
    </thead>
        <tbody>
   <tr>
       <td>Dados...</td>
            <td>Dados...</td>
    <td class="text-center">
          <button class="btn btn-custom btn-custom-primary" style="padding: 6px 12px;">
            <i class="bi bi-pencil"></i>
     </button>
   </td>
            </tr>
  </tbody>
    </table>
</div>
```

## ?? Formulários

```html
<div class="form-group">
    <label class="form-label">
        <i class="bi bi-envelope"></i> Email
    </label>
    <input class="form-control" type="email" placeholder="Digite seu email" />
</div>

<div class="form-group">
  <label class="form-label">
        <i class="bi bi-list"></i> Selecione
    </label>
    <select class="form-select">
        <option>Opção 1</option>
        <option>Opção 2</option>
    </select>
</div>
```

## ?? Alertas

```html
<!-- Sucesso -->
<div class="alert-custom alert-success">
    <i class="bi bi-check-circle-fill"></i>
    <div>Operação realizada com sucesso!</div>
</div>

<!-- Erro -->
<div class="alert-custom alert-danger">
    <i class="bi bi-exclamation-triangle-fill"></i>
    <div>Ocorreu um erro!</div>
</div>

<!-- Aviso -->
<div class="alert-custom alert-warning">
    <i class="bi bi-exclamation-circle-fill"></i>
  <div>Atenção: Operação pendente</div>
</div>

<!-- Info -->
<div class="alert-custom alert-info">
    <i class="bi bi-info-circle-fill"></i>
<div>Informação importante</div>
</div>
```

## ??? Badges

```html
<span class="badge-custom badge-primary">Primary</span>
<span class="badge-custom badge-success">Success</span>
<span class="badge-custom badge-danger">Danger</span>
<span class="badge-custom badge-warning">Warning</span>
<span class="badge-custom badge-info">Info</span>
```

## ?? Modais

```html
<!-- Modal -->
@if (MostrarModal)
{
    <div class="modal-bg animate-fade-in">
        <div class="modal-card animate-slide-up">
            <div style="display: flex; justify-content: space-between; align-items: center;">
                <h3 class="text-gradient">Título Modal</h3>
          <button style="background: none; border: none; font-size: 1.5rem; cursor: pointer;" @onclick="Fechar">×</button>
            </div>

            <!-- Conteúdo do Modal -->

        <div class="d-flex gap-2" style="margin-top: 2rem;">
 <button class="btn btn-custom btn-custom-outline" style="flex: 1;" @onclick="Fechar">Cancelar</button>
       <button class="btn btn-custom btn-custom-success" style="flex: 1;" @onclick="Confirmar">Confirmar</button>
            </div>
        </div>
    </div>
}
```

## ?? Cores e Variáveis

```css
/* Variáveis definidas em :root */
--primary-color: #1e3a8a          /* Azul Escuro */
--secondary-color: #0d9488        /* Teal */
--success-color: #28a745          /* Verde */
--warning-color: #f6c23e          /* Amarelo */
--danger-color: #d9534f   /* Vermelho */
--info-color: #3498db        /* Azul Claro */
--light-bg: #f4f7f9          /* Background */
```

## ?? Utilitários Úteis

```html
<!-- Flexbox -->
<div class="d-flex gap-2">          <!-- Flex com espaçamento -->
<div class="d-flex justify-content-between">  <!-- Espaço entre -->
<div class="d-flex align-items-center">     <!-- Centralizado -->

<!-- Margin/Padding -->
<div class="mt-4">                  <!-- Margin Top -->
<div class="mb-4">      <!-- Margin Bottom -->
<div class="p-4">      <!-- Padding All -->

<!-- Shadow -->
<div class="shadow-sm">             <!-- Sombra Pequena -->
<div class="shadow-md">             <!-- Sombra Média -->
<div class="shadow-lg">           <!-- Sombra Grande -->

<!-- Text -->
<p class="text-center">           <!-- Centralizado -->
<p class="text-muted"> <!-- Cor Acinzentada -->
<p class="page-title">              <!-- Título Grande -->
<p class="page-subtitle">     <!-- Subtítulo -->
```

## ?? Padrão de Formulário Completo

```html
<div class="page-admin">
    <div class="admin-header-row">
        <div>
            <h1 class="page-title">Novo Item</h1>
       <p class="page-subtitle">Preencha os dados</p>
        </div>
        <a href="..." class="btn btn-custom btn-custom-outline">Voltar</a>
    </div>

    <div class="custom-card elevation-2" style="max-width: 600px; margin: 0 auto;">
        <div class="card-body-custom">
            <EditForm Model="item" OnValidSubmit="Salvar">
             <DataAnnotationsValidator />

           <div class="form-group">
    <label class="form-label">Campo 1</label>
        <InputText @bind-Value="item.Campo1" class="form-control" />
   <ValidationMessage For="@(() => item.Campo1)" class="text-danger small mt-1" />
    </div>

     <div class="form-group">
    <label class="form-label">Campo 2</label>
        <InputSelect @bind-Value="item.Campo2" class="form-select">
     <option value="">Selecione...</option>
 </InputSelect>
     </div>

             @if (!string.IsNullOrEmpty(ErrorMessage))
   {
   <div class="alert-custom alert-danger">
           <i class="bi bi-exclamation-triangle-fill"></i>
           <div>@ErrorMessage</div>
    </div>
    }

   <div class="d-flex gap-2" style="margin-top: 2rem;">
     <a href="..." class="btn btn-custom btn-custom-outline" style="flex: 1;">Cancelar</a>
            <button type="submit" class="btn btn-custom btn-custom-success" style="flex: 1;">Criar</button>
    </div>
      </EditForm>
        </div>
    </div>
</div>
```

## ? Checklist - Aplicar em Todas as Pages

- [ ] `<div class="page-admin/page-student/page-teacher">` na raiz
- [ ] `.admin-header-row` com título e botões
- [ ] `.custom-card` para cards principais
- [ ] `.custom-table` para tabelas
- [ ] `.btn btn-custom btn-custom-*` para botões
- [ ] `.form-control` e `.form-select` para formulários
- [ ] `.alert-custom` para mensagens
- [ ] `.modal-bg` e `.modal-card` para modais
- [ ] `@layout MainLayout` no topo
- [ ] Media queries testadas no mobile

## ?? Classes Responsivas

Todos os estilos são responsivos! Testadas em:
- Desktop (1920px+)
- Tablet (768px - 1024px)
- Mobile (até 480px)

## ?? Dicas

1. Use `style="flex-wrap: wrap;"` em flex containers para mobile
2. Use `style="overflow-x: auto;"` em tabelas para mobile
3. Use `style="max-width: 600px; margin: 0 auto;"` em formulários centralizados
4. Sempre add `@layout MainLayout` para navbar funcionar
5. Use ícones Bootstrap (`<i class="bi bi-*"></i>`)

