using gestaopedagogica.Data;
using gestaopedagogica.Models;
using gestaopedagogica.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

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
            [Required(ErrorMessage = "O utilizador/email é obrigatório.")]
            public string Username { get; set; } = "";

            [Required(ErrorMessage = "A senha é obrigatória.")]
            [DataType(DataType.Password)]
            public string Password { get; set; } = "";

            public bool RememberMe { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var usernameOrEmail = Input.Username.Trim();

            // 1. Localizar o user (Email ou Username)
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Email.ToUpper() == usernameOrEmail.ToUpper()
                                       || u.UserName.ToUpper() == usernameOrEmail.ToUpper());

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Utilizador não encontrado.");
                return Page();
            }

            // 2. Tentar o Login
            // Usamos user.UserName pois o PasswordSignInAsync espera o UserName oficial da DB
            var result = await _signInManager.PasswordSignInAsync(user.UserName, Input.Password, Input.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                var destination = await DeterminarRetorno(user.Id);

                // CRUCIAL: Forçamos minúsculas porque o Linux do Render é Case-Sensitive
                destination = destination.ToLower().Trim();

                Console.WriteLine($"[LOGIN SUCESSO] Redirecionando para: {destination}");

                // LocalRedirect é mais seguro para rotas internas
                return LocalRedirect(destination);
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "Conta bloqueada temporariamente.");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Senha incorreta.");
            }

            return Page();
        }

        private async Task<string> DeterminarRetorno(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return "/";

            var roles = await _userManager.GetRolesAsync(user);

            // IMPORTANTE: Retornar as strings já em MINÚSCULAS para evitar 404 no Render
            if (roles.Contains("Admin")) return "/admin/dashboardadmin";

            var professor = await _professorService.GetProfessorByUserIdAsync(userId);
            if (professor != null) return "/professor/dashboardprofessor";

            var alunos = await _alunoService.GetAlunosAsync();
            var aluno = alunos?.FirstOrDefault(a => a.UserId == userId);
            if (aluno != null) return "/aluno/dashboardaluno";

            return "/";
        }
    }
}