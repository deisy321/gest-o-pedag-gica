using gestaopedagogica.Components;
using gestaopedagogica.Shared;
using gestaopedagogica.Data;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpContextAccessor();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddAntiforgery(options => {
    options.HeaderName = "X-CSRF-TOKEN";
});
builder.Services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<ApplicationUser>>();
// Register application state for user role
builder.Services.AddSingleton<UserState>();

// Configure DbContext with PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
 options.UseNpgsql(connectionString)
);

// Configure Identity using AddIdentity (supports roles)
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
 options.SignIn.RequireConfirmedAccount = false;
 options.Password.RequireNonAlphanumeric = false;
 options.Password.RequireUppercase = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdministratorRole", policy => policy.RequireRole("Administrador"));
    options.AddPolicy("RequireProfessorRole", policy => policy.RequireRole("Professor"));
    options.AddPolicy("RequireAlunoRole", policy => policy.RequireRole("Aluno"));
});

// ---- ATIVAR ERROS DETALHADOS DO BLAZOR ----
builder.Services.Configure<Microsoft.AspNetCore.Components.Server.CircuitOptions>(options =>
{
    options.DetailedErrors = true;
});
// --------------------------------------------

var app = builder.Build();

// Apply migrations and seed in development
using (var scope = app.Services.CreateScope())
{
 var services = scope.ServiceProvider;
 try
 {
 var context = services.GetRequiredService<ApplicationDbContext>();
 context.Database.Migrate();

 if (app.Environment.IsDevelopment())
 {
 // seed roles and a test admin/professor/aluno
 var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
 var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

 string[] roles = new[] { "Administrador", "Professor", "Aluno" };
 foreach (var r in roles)
 {
 if (!roleManager.RoleExistsAsync(r).Result)
 {
 roleManager.CreateAsync(new IdentityRole(r)).Wait();
 }
 }

 // Create admin
 var admin = userManager.FindByEmailAsync("admin@local").Result;
 if (admin == null)
 {
 admin = new ApplicationUser { UserName = "admin@local", Email = "admin@local" };
 userManager.CreateAsync(admin, "Admin123!").Wait();
 userManager.AddToRoleAsync(admin, "Administrador").Wait();
 }

 // Create professor
 var prof = userManager.FindByEmailAsync("prof@local").Result;
 if (prof == null)
 {
 prof = new ApplicationUser { UserName = "prof@local", Email = "prof@local" };
 userManager.CreateAsync(prof, "Prof123!").Wait();
 userManager.AddToRoleAsync(prof, "Professor").Wait();
 }

 // Create aluno
 var aluno = userManager.FindByEmailAsync("aluno@local").Result;
 if (aluno == null)
 {
 aluno = new ApplicationUser { UserName = "aluno@local", Email = "aluno@local" };
 userManager.CreateAsync(aluno, "Aluno123!").Wait();
 userManager.AddToRoleAsync(aluno, "Aluno").Wait();
 }

 // Criar utilizadores de teste adicionais
 // ALUNOS DE TESTE
 await CriarUtilizadorSeNaoExistir(userManager, "joao.silva@escola.com", "Joao123!", "Aluno", "Joao Silva");
 await CriarUtilizadorSeNaoExistir(userManager, "maria.santos@escola.com", "Maria123!", "Aluno", "Maria Santos");
 await CriarUtilizadorSeNaoExistir(userManager, "pedro.oliveira@escola.com", "Pedro123!", "Aluno", "Pedro Oliveira");
 await CriarUtilizadorSeNaoExistir(userManager, "ana.costa@escola.com", "Ana123!", "Aluno", "Ana Costa");

 // PROFESSORES DE TESTE
 await CriarUtilizadorSeNaoExistir(userManager, "carlos.ferreira@escola.com", "Carlos123!", "Professor", "Carlos Ferreira");
 await CriarUtilizadorSeNaoExistir(userManager, "isabel.martins@escola.com", "Isabel123!", "Professor", "Isabel Martins");
 await CriarUtilizadorSeNaoExistir(userManager, "luis.gomes@escola.com", "Luis123!", "Professor", "Luis Gomes");

 // ADMINISTRADOR DE TESTE ADICIONAL
 await CriarUtilizadorSeNaoExistir(userManager, "admin2@escola.com", "Admin123!", "Administrador", "Admin Dois");

 Console.WriteLine("? Utilizadores de teste criados com sucesso!");
 }
 }
 catch (Exception ex)
 {
 var logger = services.GetRequiredService<ILogger<Program>>();
 logger.LogError(ex, "An error occurred while migrating or seeding the database.");
 }
}

// Função auxiliar para criar utilizadores
async Task CriarUtilizadorSeNaoExistir(UserManager<ApplicationUser> userManager, string email, string senha, string role, string nomeCompleto = "")
{
 var user = await userManager.FindByEmailAsync(email);
 if (user == null)
 {
 user = new ApplicationUser 
 { 
  UserName = email, 
  Email = email,
  EmailConfirmed = true
 };
 
 var result = await userManager.CreateAsync(user, senha);
 if (result.Succeeded)
 {
  await userManager.AddToRoleAsync(user, role);
  Console.WriteLine($"? Criado: {email} ({role})");
 }
 else
 {
  Console.WriteLine($"? Erro ao criar {email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
 }
 }
 else
 {
  Console.WriteLine($"??  Utilizador já existe: {email}");
 }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
 app.UseExceptionHandler("/Error");
 app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
