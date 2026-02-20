using gestaopedagogica.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace gestaopedagogica.Pages.Identity.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public LoginModel(SignInManager<ApplicationUser> signInManager,
                          UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required(ErrorMessage = "O campo Email/Utilizador é obrigatório.")]
            public string Username { get; set; } = "";

            [Required(ErrorMessage = "O campo Senha é obrigatório.")]
            [DataType(DataType.Password)]
            public string Password { get; set; } = "";

            public bool RememberMe { get; set; }
        }

        public void OnGet()
        {
            // Apenas exibe a página de login
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            // Normaliza para minúsculas para evitar problemas de case-sensitive
            var usernameOrEmail = Input.Username.Trim().ToLower();

            // Busca usuário pelo Email ou UserName (insensível a maiúsculas)
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == usernameOrEmail
                                       || u.UserName.ToLower() == usernameOrEmail);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Utilizador não encontrado.");
                return Page();
            }

            // Verifica se a senha está correta
            var passwordValid = await _userManager.CheckPasswordAsync(user, Input.Password);
            if (!passwordValid)
            {
                ModelState.AddModelError(string.Empty, "Senha incorreta.");
                return Page();
            }

            // Efetua login
            await _signInManager.SignInAsync(user, Input.RememberMe);

            // Redireciona para página inicial
            return Redirect("/");
        }
    }
}
