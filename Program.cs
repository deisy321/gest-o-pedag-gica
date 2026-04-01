using gestaopedagogica.Components;
using gestaopedagogica.Data;
using gestaopedagogica.Services;
using gestaopedagogica.Shared;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// --- 1. CONFIGURAÇÃO DE ROTAS ---
builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
});

// --- 2. SERVIÇOS DE UI E ACESSO ---
builder.Services.AddHttpContextAccessor();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// --- 3. CONFIGURAÇÃO DE ANTIFORGERY ---
builder.Services.AddAntiforgery(options => {
    options.HeaderName = "X-CSRF-TOKEN";
});

// --- 4. CONFIGURAÇÃO DA BASE DE DADOS ---
var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                      ?? builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// --- 5. IDENTITY E ROLES ---
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// --- 6. CONFIGURAÇÃO DE COOKIES (CORREÇÃO PARA REDIRECIONAMENTO 404) ---
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/identity/account/login";
    options.LogoutPath = "/identity/account/logout";
    options.AccessDeniedPath = "/identity/account/accessdenied";

    options.Cookie.Name = "GestaopedagogicaAuth";
    options.Cookie.HttpOnly = true;

    // CRUCIAL: Sem 'IsEssential', o login é perdido no redirecionamento do Render
    options.Cookie.IsEssential = true;

    // CRUCIAL: 'Always' porque o Render é HTTPS. 'Lax' permite o redirecionamento.
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;

    options.ExpireTimeSpan = TimeSpan.FromHours(2);
    options.SlidingExpiration = true;
});

// --- 7. AUTORIZAÇÃO E POLÍTICAS ---
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdministratorRole", policy => policy.RequireRole("Admin"));
    options.AddPolicy("RequireProfessorRole", policy => policy.RequireRole("Professor"));
    options.AddPolicy("RequireAlunoRole", policy => policy.RequireRole("Aluno"));
});

builder.Services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<ApplicationUser>>();
builder.Services.AddSingleton<UserState>();

// --- 8. SERVIÇOS DA APLICAÇÃO (TODOS OS SERVIÇOS) ---
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ModuloService>();
builder.Services.AddScoped<TurmaService>();
builder.Services.AddScoped<TrabalhoService>();
builder.Services.AddScoped<AdminService>();
builder.Services.AddScoped<ProfessorService>();
builder.Services.AddScoped<AlunoService>();
builder.Services.AddScoped<DisciplinaService>();
builder.Services.AddScoped<CursoService>();

builder.Services.AddHttpClient<IAService>(client =>
{
    var ollamaUrl = Environment.GetEnvironmentVariable("OllamaConfig__BaseUrl") ?? "http://127.0.0.1:11434/";
    client.BaseAddress = new Uri(ollamaUrl);
    client.Timeout = TimeSpan.FromMinutes(5);
});

var app = builder.Build();

// --- 9. CONFIGURAÇÃO PARA PROXY RENDER (DEVE SER A PRIMEIRA COISA) ---
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// --- 10. MIGRAÇÕES E SEED DATA AUTOMÁTICO ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        string[] roles = { "Admin", "Professor", "Aluno" };
        foreach (var r in roles)
        {
            if (!roleManager.RoleExistsAsync(r).Result)
            {
                roleManager.CreateAsync(new IdentityRole(r)).Wait();
            }
        }

        await CriarUser(userManager, "admin@local", "Admin123!", "Admin");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erro no Seed/Migração: {ex.Message}");
    }
}

// --- 11. PIPELINE DE EXECUÇÃO ---
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

// Em produção (Render), o proxy já trata o HTTPS, mas mantemos para segurança local
//app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ORDEM OBRIGATÓRIA: Authentication antes de Authorization
app.UseAuthentication();
app.UseAuthorization();

// --- 12. MAPEAMENTO DE ENDPOINTS ---
app.MapControllers();
app.MapBlazorHub();
app.MapRazorPages(); // Importante estar aqui para as rotas do Identity
app.MapFallbackToPage("/_host");

app.Run();

// --- MÉTODO AUXILIAR PARA SEED ---
async Task CriarUser(UserManager<ApplicationUser> um, string email, string senha, string role)
{
    if (await um.FindByEmailAsync(email) == null)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };
        var result = await um.CreateAsync(user, senha);
        if (result.Succeeded)
        {
            await um.AddToRoleAsync(user, role);
        }
    }
}