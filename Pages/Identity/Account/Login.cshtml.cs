using gestaopedagogica.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace gestaopedagogica.Pages.Identity.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(
            SignInManager<ApplicationUser> signInManager, 
            UserManager<ApplicationUser> userManager,
            ILogger<LoginModel> logger)
    {
            _signInManager = signInManager;
        _userManager = userManager;
         _logger = logger;
        }

        [BindProperty]
        public LoginInputModel Input { get; set; } = new();

   public void OnGet()
        {
            _logger.LogInformation("Página de login acessada");
        }

      public async Task<IActionResult> OnPostAsync()
        {
          _logger.LogInformation($"Tentativa de login com username: {Input?.Username}");

     if (!ModelState.IsValid)
            {
            _logger.LogWarning($"ModelState inválido. Erros: {string.Join(", ", ModelState.Values.SelectMany(v => v.Errors))}");
    return Page();
            }

      try
  {
           // Procura pelo username primeiro
           var user = await _userManager.FindByNameAsync(Input.Username);

           // Se não encontrar, procura pelo email
        if (user == null)
         {
      user = await _userManager.FindByEmailAsync(Input.Username);
        _logger.LogInformation($"Utilizador não encontrado por username, tentando email");
        }

   if (user == null)
              {
         _logger.LogWarning($"Utilizador '{Input.Username}' não encontrado");
     ModelState.AddModelError(string.Empty, "Utilizador ou email não encontrado.");
           return Page();
           }

    _logger.LogInformation($"Utilizador encontrado: {user.UserName}. Tentando autenticar...");

                // Tenta fazer signin
   var result = await _signInManager.PasswordSignInAsync(
     user,
    Input.Password,
    Input.RememberMe,
    lockoutOnFailure: false);

           if (result.Succeeded)
 {
   _logger.LogInformation($"Utilizador {user.UserName} autenticado com sucesso");

      // Obtém as roles
           var roles = await _userManager.GetRolesAsync(user);
      _logger.LogInformation($"Roles do utilizador: {string.Join(", ", roles)}");

   // Redireciona baseado na role
 if (roles.Contains("Administrador"))
         {
  _logger.LogInformation("Redirecionando para Dashboard Admin");
         return Redirect("/admin/DashboardAdmin");
             }
        else if (roles.Contains("Professor"))
         {
            _logger.LogInformation("Redirecionando para Dashboard Professor");
             return Redirect("/professor/DashboardProfessor");
 }
     else if (roles.Contains("Aluno"))
         {
         _logger.LogInformation("Redirecionando para Dashboard Aluno");
           return Redirect("/aluno/DashboardAluno");
 }

         _logger.LogWarning("Utilizador sem role definida. Redirecionando para home");
         return Redirect("/");
     }

          if (result.IsLockedOut)
       {
        _logger.LogWarning($"Conta bloqueada: {user.UserName}");
     ModelState.AddModelError(string.Empty, "Conta bloqueada. Contacte o administrador.");
               return Page();
          }

   _logger.LogWarning($"Senha incorreta para {user.UserName}");
       ModelState.AddModelError(string.Empty, "Senha incorreta.");
  return Page();
            }
            catch (Exception ex)
            {
   _logger.LogError(ex, "Exceção ao processar login");
             ModelState.AddModelError(string.Empty, "Ocorreu um erro ao processar o login.");
     return Page();
   }
     }

    public class LoginInputModel
      {
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool RememberMe { get; set; }
      }
    }
}
