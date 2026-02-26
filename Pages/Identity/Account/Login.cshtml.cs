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

            // ?? IMPORTANTE: Determinar o tipo de usuário e redirecionar apropriadamente
            var returnUrl = await DeterminarRetorno(user.Id);

            return Redirect(returnUrl);
        }

        private async Task<string> DeterminarRetorno(string userId)
        {
            try
            {
                // Verificar se é Admin (verificar roles)
                var user = await _userManager.FindByIdAsync(userId);
                var roles = await _userManager.GetRolesAsync(user);

                if (roles.Contains("Admin"))
                {
                    Console.WriteLine($"? [Login] Admin detectado: {user.Email}");
                    return "/admin/Dashboard";
                }

                // Verificar se é Professor
                var professor = await _professorService.GetProfessorByUserIdAsync(userId);
                if (professor != null)
                {
                    Console.WriteLine($"? [Login] Professor detectado: {professor.Nome} (Turmas: {professor.Turmas?.Count ?? 0})");
                    return "/professor/DashboardProfessor";
                }

                // Verificar se é Aluno
                var alunos = await _alunoService.GetAlunosAsync();
                var aluno = alunos?.FirstOrDefault(a => a.UserId == userId);
                if (aluno != null)
                {
                    Console.WriteLine($"? [Login] Aluno detectado: {aluno.Nome}");
                    Console.WriteLine($"   ?? Turma: {aluno.Turma?.Nome ?? "Sem turma"}");
                    return "/aluno/DashboardAluno";
                }

                // Padrão: voltar para home
                Console.WriteLine($"??  [Login] Tipo de utilizador não identificado para: {user.Email}");
                return "/";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? [Login] Erro ao determinar retorno: {ex.Message}");
                return "/";
            }
        }
    }
}
