using gestaopedagogica.Components;
using gestaopedagogica.Data;
using gestaopedagogica.Services;
using gestaopedagogica.Shared;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddHttpContextAccessor();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Configuração de Antiforgery para evitar erros de validação no Login
builder.Services.AddAntiforgery(options => {
    options.HeaderName = "X-CSRF-TOKEN";
});

builder.Services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<ApplicationUser>>();

// Register application state for user role
builder.Services.AddSingleton<UserState>();

// --- CONEXÃO COM O BANCO DE DADOS ---
var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                      ?? builder.Configuration.GetConnectionString("DefaultConnection");

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

// --- CONFIGURAÇÃO DO OLLAMA (IA) ---
builder.Services.AddHttpClient<IAService>(client =>
{
    var ollamaUrl = Environment.GetEnvironmentVariable("OllamaConfig__BaseUrl")
                    ?? "http://127.0.0.1:11434/";

    client.BaseAddress = new Uri(ollamaUrl);
    client.Timeout = TimeSpan.FromMinutes(5);
});

// Configuração do Identity (Roles: Admin, Professor, Aluno)
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Ajuste das Políticas para usar "Admin" em vez de "Administrador"
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdministratorRole", policy => policy.RequireRole("Admin"));
    options.AddPolicy("RequireProfessorRole", policy => policy.RequireRole("Professor"));
    options.AddPolicy("RequireAlunoRole", policy => policy.RequireRole("Aluno"));
});

// Ativar erros detalhados do Blazor para ajudar no relatório da PAP
builder.Services.Configure<Microsoft.AspNetCore.Components.Server.CircuitOptions>(options =>
{
    options.DetailedErrors = true;
});

var app = builder.Build();

// --- CONFIGURAÇÃO PARA O RENDER (LINUX PROXY) ---
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// Apply migrations and seed
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        // Criar roles (Admin, Professor, Aluno)
        string[] roles = new[] { "Admin", "Professor", "Aluno" };
        foreach (var r in roles)
        {
            if (!roleManager.RoleExistsAsync(r).Result)
                roleManager.CreateAsync(new IdentityRole(r)).Wait();
        }

        // Criar usuários de teste
        await CriarUtilizadorSeNaoExistir(userManager, "admin@local", "Admin123!", "Admin");
        await CriarUtilizadorSeNaoExistir(userManager, "prof@local", "Prof123!", "Professor");
        await CriarUtilizadorSeNaoExistir(userManager, "aluno@local", "Aluno123!", "Aluno");

        Console.WriteLine("✅ Banco de dados pronto!");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Erro no Seed do banco de dados.");
    }
}

async Task CriarUtilizadorSeNaoExistir(UserManager<ApplicationUser> userManager, string email, string senha, string role)
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

// Pipeline de requisições
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Importante: A ordem correta para o Identity funcionar
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();