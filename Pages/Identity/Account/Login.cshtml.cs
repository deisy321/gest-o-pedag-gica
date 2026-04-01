using gestaopedagogica.Data;
using gestaopedagogica.Models;
using gestaopedagogica.Services;
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
        private readonly ApplicationDbContext _context;
        private readonly ProfessorService _professorService;
        private readonly AlunoService _alunoService;

        public LoginModel(SignInManager<ApplicationUser> signInManager,
                          UserManager<ApplicationUser> userManager,
                          ApplicationDbContext context,
                          ProfessorService professorService,
                          AlunoService alunoService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;
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

        public void OnGet()
        {
            // Apenas exibe a página de login
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var usernameOrEmail = Input.Username.Trim().ToLower();

            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == usernameOrEmail
                                       || u.UserName.ToLower() == usernameOrEmail);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Utilizador não encontrado.");
                return Page();
            }

            var passwordValid = await _userManager.CheckPasswordAsync(user, Input.Password);
            if (!passwordValid)
            {
                ModelState.AddModelError(string.Empty, "Senha incorreta.");
                return Page();
            }

            // Efetua login
            await _signInManager.SignInAsync(user, Input.RememberMe);

            // Determinar o tipo de usuário e redirecionar
            var returnUrl = await DeterminarRetorno(user.Id);

            // IMPORTANTE: LocalRedirect garante que o redirecionamento funcione no Render/Linux
            return LocalRedirect(returnUrl);
        }

        private async Task<string> DeterminarRetorno(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                var roles = await _userManager.GetRolesAsync(user);

                // Verificação de Admin - Rota com 'A' maiúsculo
                if (roles.Contains("Admin"))
                {
                    return "/Admin/Dashboard";
                }

                // Verificação de Professor - Rota com 'P' maiúsculo
                var professor = await _professorService.GetProfessorByUserIdAsync(userId);
                if (professor != null)
                {
                    return "/Professor/DashboardProfessor";
                }

                // Verificação de Aluno - Rota com 'A' maiúsculo (Confirmado que funciona no Render)
                var alunos = await _alunoService.GetAlunosAsync();
                var aluno = alunos?.FirstOrDefault(a => a.UserId == userId);
                if (aluno != null)
                {
                    return "/Aluno/DashboardAluno";
                }

                // Padrão: voltar para home
                return "/";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao determinar retorno: {ex.Message}");
                return "/";
            }
        }
    }
}