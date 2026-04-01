using gestaopedagogica.Data;
using gestaopedagogica.Models;
using gestaopedagogica.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace gestaopedagogica.Pages.identity.account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ProfessorService _professorService;
        private readonly AlunoService _alunoService;

        public LoginModel(SignInManager<ApplicationUser> signInManager,
                          UserManager<ApplicationUser> userManager,
                          ProfessorService professorService,
                          AlunoService alunoService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _professorService = professorService;
            _alunoService = alunoService;
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

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var usernameOrEmail = Input.Username.Trim().ToLower();

            // 1. Procurar o utilizador
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == usernameOrEmail
                                       || u.UserName.ToLower() == usernameOrEmail);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Utilizador ou senha inválidos.");
                return Page();
            }

            // 2. Tentar o Login (Uso de PasswordSignInAsync para garantir a criação do Cookie)
            var result = await _signInManager.PasswordSignInAsync(user.UserName, Input.Password, Input.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                // 3. Determinar para onde o utilizador deve ir
                var destination = await DeterminarRetorno(user.Id);

                // Limpeza extra para o Linux (Render)
                destination = destination.ToLower().Trim();

                Console.WriteLine($"[LOGIN] Sucesso para {user.Email}. Redirecionando para: {destination}");

                return Redirect(destination);
            }

            ModelState.AddModelError(string.Empty, "Tentativa de login inválida.");
            return Page();
        }

        private async Task<string> DeterminarRetorno(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null) return "/";

                var roles = await _userManager.GetRolesAsync(user);

                // IMPORTANTE: Rotas sempre em minúsculas
                if (roles.Contains("Admin"))
                {
                    return "/admin/dashboard";
                }

                var professor = await _professorService.GetProfessorByUserIdAsync(userId);
                if (professor != null)
                {
                    return "/professor/dashboardprofessor";
                }

                var alunos = await _alunoService.GetAlunosAsync();
                var aluno = alunos?.FirstOrDefault(a => a.UserId == userId);
                if (aluno != null)
                {
                    return "/aluno/dashboardaluno";
                }

                return "/";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERRO REDIRECT] {ex.Message}");
                return "/";
            }
        }
    }
}