using gestaopedagogica.Components;
using gestaopedagogica.Data;
using gestaopedagogica.Services;
using gestaopedagogica.Shared;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddHttpContextAccessor();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddAntiforgery(options => {
    options.HeaderName = "X-CSRF-TOKEN";
});
builder.Services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<ApplicationUser>>();
// Register application state for user role
builder.Services.AddSingleton<UserState>();

// --- AJUSTE NA CONEXÃO COM O BANCO DE DADOS ---
// Primeiro tenta ler a variável de ambiente do Render, se não existir, usa o appsettings local.
var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                      ?? builder.Configuration.GetConnectionString("DefaultConnection");

// Registro do DbContextFactory e DbContext usando a string dinâmica
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString)
);

// Registrar serviços da aplicação
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ModuloService>();
builder.Services.AddScoped<TurmaService>();
builder.Services.AddScoped<TrabalhoService>();
builder.Services.AddScoped<AdminService>();
builder.Services.AddScoped<ProfessorService>();
builder.Services.AddScoped<AlunoService>();
builder.Services.AddScoped<DisciplinaService>();
builder.Services.AddScoped<CursoService>();

// --- AJUSTE NA URL DO OLLAMA (IA) ---
builder.Services.AddHttpClient<IAService>(client =>
{
    // Tenta ler o link que colaste no Render, se for nulo (PC local), usa o IP padrão
    var ollamaUrl = Environment.GetEnvironmentVariable("OllamaConfig__BaseUrl")
                    ?? "http://127.0.0.1:11434/";

    client.BaseAddress = new Uri(ollamaUrl);
    client.Timeout = TimeSpan.FromMinutes(5);
});

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

var app = builder.Build();

// Apply migrations and seed
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();

        // AJUSTE: Removido o IF Development para que os usuários sejam criados no Render também
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        // Criar roles se não existirem
        string[] roles = new[] { "Administrador", "Professor", "Aluno" };
        foreach (var r in roles)
        {
            if (!roleManager.RoleExistsAsync(r).Result)
                roleManager.CreateAsync(new IdentityRole(r)).Wait();
        }

        // Criar usuários principais
        await CriarUtilizadorSeNaoExistir(userManager, "admin@local", "Admin123!", "Administrador");
        await CriarUtilizadorSeNaoExistir(userManager, "prof@local", "Prof123!", "Professor");
        await CriarUtilizadorSeNaoExistir(userManager, "aluno@local", "Aluno123!", "Aluno");

        Console.WriteLine("✅ Banco de dados pronto e usuários criados!");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Erro ao aplicar migrações ou seed no banco de dados.");
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
            Console.WriteLine($"✅ Criado: {email} ({role})");
        }
    }
}

// Configure the HTTP request pipeline
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
app.MapControllers();
app.MapRazorPages();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();