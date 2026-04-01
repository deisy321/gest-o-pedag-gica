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
            [Required] public string Username { get; set; } = "";
            [Required][DataType(DataType.Password)] public string Password { get; set; } = "";
            public bool RememberMe { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var usernameOrEmail = Input.Username.Trim().ToLower();

            // 1. Localizar o user
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == usernameOrEmail
                                       || u.UserName.ToLower() == usernameOrEmail);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Utilizador não encontrado.");
                return Page();
            }

            // 2. Login Real (PasswordSignInAsync é necessário para criar o cookie corretamente)
            var result = await _signInManager.PasswordSignInAsync(user.UserName, Input.Password, Input.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                var destination = await DeterminarRetorno(user.Id);

                // Força a URL final para minúsculas e remove espaços
                destination = destination.ToLower().Trim();

                Console.WriteLine($"[LOGIN OK] Redirecionando para: {destination}");

                return Redirect(destination);
            }

            ModelState.AddModelError(string.Empty, "Senha incorreta.");
            return Page();
        }

        private async Task<string> DeterminarRetorno(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            var roles = await _userManager.GetRolesAsync(user);

            if (roles.Contains("Admin")) return "/admin/dashboard";

            var professor = await _professorService.GetProfessorByUserIdAsync(userId);
            if (professor != null) return "/professor/dashboardprofessor";

            var alunos = await _alunoService.GetAlunosAsync();
            var aluno = alunos?.FirstOrDefault(a => a.UserId == userId);
            if (aluno != null) return "/aluno/dashboardaluno";

            return "/";
        }
    }
}