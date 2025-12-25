using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using gestaopedagogica.Data;

namespace gestaopedagogica.Pages.Identity.Account
{
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;

        public LogoutModel(SignInManager<ApplicationUser> signInManager)
        {
            _signInManager = signInManager;
        }

        // Mensagem que será exibida na página
        [TempData]
        public string StatusMessage { get; set; }

        // Permite logout via GET
        public async Task<IActionResult> OnGetAsync()
        {
            await _signInManager.SignOutAsync();
            StatusMessage = "Sessão terminada com sucesso! A redirecionar para login...";
            return Page();
        }

        // Permite logout via POST
        public async Task<IActionResult> OnPostAsync()
        {
            await _signInManager.SignOutAsync();
            StatusMessage = "Sessão terminada com sucesso! A redirecionar para login...";
            return Page();
        }
    }
}
