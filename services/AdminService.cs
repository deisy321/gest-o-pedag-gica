using gestaopedagogica.Data;
using gestaopedagogica.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace gestaopedagogica.Services
{
    public class AdminService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public AdminService(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // 1. Totais para os Cards
        public async Task<int> GetTotalAlunosAsync()
        {
            // Se tiveres uma tabela Alunos, é mais rápido usar: await _context.Alunos.CountAsync();
            var alunos = await _userManager.GetUsersInRoleAsync("Aluno");
            return alunos.Count;
        }

        public async Task<int> GetTotalProfessoresAsync()
        {
            var professores = await _userManager.GetUsersInRoleAsync("Professor");
            return professores.Count;
        }

        // 2. MÉTODO CORRIGIDO (Nome alterado para bater certo com a Dashboard)
        public async Task<List<ResumoAtribuicao>> GetAtribuicoesAtivasAsync()
        {
            try
            {
                return await _context.TurmaProfessores
                    .Include(tp => tp.Professor)
                    .Include(tp => tp.Turma)
                    .Include(tp => tp.Disciplina) // Inclui a nova relação de Disciplina
                    .Select(tp => new ResumoAtribuicao
                    {
                        ProfessorNome = tp.Professor.Nome ?? "Sem Nome",
                        TurmaNome = tp.Turma.Nome ?? "Sem Turma",
                        // Lógica: Se houver Disciplina, mostra o Nome dela, senão mostra o campo Modulo (string)
                        ModuloNome = tp.Disciplina != null ? tp.Disciplina.Nome : (tp.Modulo ?? "Sem Disciplina")
                    }).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao carregar atribuições: {ex.Message}");
                return new List<ResumoAtribuicao>();
            }
        }

        public async Task<List<ApplicationUser>> GetProfessoresAsync()
        {
            var professores = await _userManager.GetUsersInRoleAsync("Professor");
            return professores.ToList();
        }

        public async Task<List<ApplicationUser>> GetTodosUsuariosAsync()
        {
            return await _userManager.Users.ToListAsync();
        }
    }

    // Classe de suporte
    public class ResumoAtribuicao
    {
        public string ProfessorNome { get; set; } = string.Empty;
        public string TurmaNome { get; set; } = string.Empty;
        public string ModuloNome { get; set; } = string.Empty;
    }
}
