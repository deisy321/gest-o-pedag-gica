using gestaopedagogica.Components;
using gestaopedagogica.Data;
using gestaopedagogica.Services;
using gestaopedagogica.Shared;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// --- CORREÇÃO DE ROTAS PARA LINUX (RENDER) ---
builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
});

// 1. Serviços de UI e Acesso
builder.Services.AddHttpContextAccessor();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// 2. Configuração de Antiforgery
builder.Services.AddAntiforgery(options => {
    options.HeaderName = "X-CSRF-TOKEN";
});

// 3. Base de Dados
var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                      ?? builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// 4. Identity e Roles
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// 5. Configuração de Cookies (Ajustado para Render)
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/identity/account/login";
    options.LogoutPath = "/identity/account/logout";
    options.AccessDeniedPath = "/identity/account/accessdenied";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Força HTTPS para o cookie
});

// 6. Autorização e Políticas
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdministratorRole", policy => policy.RequireRole("Admin"));
    options.AddPolicy("RequireProfessorRole", policy => policy.RequireRole("Professor"));
    options.AddPolicy("RequireAlunoRole", policy => policy.RequireRole("Aluno"));
});

builder.Services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<ApplicationUser>>();
builder.Services.AddSingleton<UserState>();

// Serviços da App
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

// 7. Configuração para Proxy (Render/Linux) - CRÍTICO estar aqui no topo
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// 8. Migrações e Seed
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
                roleManager.CreateAsync(new IdentityRole(r)).Wait();
        }

        await CriarUser(userManager, "admin@local", "Admin123!", "Admin");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erro no Seed: {ex.Message}");
    }
}

async Task CriarUser(UserManager<ApplicationUser> um, string email, string senha, string role)
{
    if (await um.FindByEmailAsync(email) == null)
    {
        var user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true };
        var result = await um.CreateAsync(user, senha);
        if (result.Succeeded) await um.AddToRoleAsync(user, role);
    }
}

// 9. Pipeline de Execução
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// 10. Mapeamento
app.MapControllers();
app.MapRazorPages();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();