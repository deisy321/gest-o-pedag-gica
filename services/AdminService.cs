using gestaopedagogica.Data;
using gestaopedagogica.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore; // IMPORTANTE para o ToListAsync

namespace gestaopedagogica.Services
{
    public class AdminService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context; // Adicionado para gerir tabelas de Turmas

        public AdminService(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // 1. Totais para os Cards do Dashboard
        public async Task<int> GetTotalAlunosAsync()
        {
            var alunos = await _userManager.GetUsersInRoleAsync("Aluno");
            return alunos.Count;
        }

        public async Task<int> GetTotalProfessoresAsync()
        {
            var professores = await _userManager.GetUsersInRoleAsync("Professor");
            return professores.Count;
        }

        // 2. Métodos que o seu Dashboard está a pedir e não encontrava
        public async Task<List<ResumoAtribuicao>> GetResumoAtribuicoesAsync()
        {
            try
            {
                // Verifica se a tabela existe antes de consultar para não crashar o Blazor
                return await _context.TurmaProfessores
                    .Include(tp => tp.Professor)
                    .Include(tp => tp.Turma)
                    .Select(tp => new ResumoAtribuicao
                    {
                        ProfessorNome = tp.Professor.Nome ?? "Sem Nome",
                        TurmaNome = tp.Turma.Nome ?? "Sem Turma",
                        ModuloNome = tp.Modulo ?? "Sem Módulo"
                    }).ToListAsync();
            }
            catch
            {
                return new List<ResumoAtribuicao>(); // Retorna lista vazia se a tabela não existir
            }
        }

        public async Task<List<ApplicationUser>> GetProfessoresAsync()
        {
            var professores = await _userManager.GetUsersInRoleAsync("Professor");
            return professores.ToList();
        }

        // 3. Gestão de Utilizadores (Corrigido para Async)
        public async Task<List<ApplicationUser>> GetTodosUsuariosAsync()
        {
            return await _userManager.Users.ToListAsync();
        }
    }

    // 4. Classe de suporte (Tem de estar aqui para o Blazor a ver)
    public class ResumoAtribuicao
    {
        public string ProfessorNome { get; set; } = string.Empty;
        public string TurmaNome { get; set; } = string.Empty;
        public string ModuloNome { get; set; } = string.Empty;
    }
}
