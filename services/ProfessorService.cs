using gestaopedagogica.Data;
using gestaopedagogica.Models;
using Microsoft.EntityFrameworkCore;

namespace gestaopedagogica.Services
{
    public class ProfessorService
    {
        private readonly ApplicationDbContext _context;

        public ProfessorService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Adiciona um novo professor à base de dados
        public async Task AddProfessorAsync(Professor professor)
        {
            if (professor == null) return;

            _context.Professores.Add(professor);
            await _context.SaveChangesAsync();
        }

        // Retorna todos os professores
        public async Task<List<Professor>> GetProfessoresAsync()
        {
            return await _context.Professores
                                 .AsNoTracking()
                                 .ToListAsync();
        }

        // Retorna professor pelo UserId do Identity
        public async Task<Professor?> GetProfessorByUserIdAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return null;

            return await _context.Professores
                                 .AsNoTracking()
                                 .FirstOrDefaultAsync(p => p.UserId == userId);
        }

        // Retorna professor pelo Email (Username) do login
        public async Task<Professor?> GetProfessorByEmailAsync(string email)
        {
            if (string.IsNullOrEmpty(email)) return null;

            return await _context.Professores
                                 .AsNoTracking()
                                 .FirstOrDefaultAsync(p => p.Email == email);
        }

        // Retorna todos os trabalhos de um professor, filtrando corretamente turmas e módulos
        public async Task<List<Trabalho>> GetTrabalhosDoProfessorAsync(int professorId)
        {
            return await _context.Trabalhos
                .Include(t => t.Modulo)
                .Include(t => t.Turma)
                    .ThenInclude(tu => tu.Modulos)
                .Include(t => t.Turma)
                    .ThenInclude(tu => tu.Professores)
                .Where(t =>
                    t.Turma.Professores.Any(tp => tp.ProfessorId == professorId) &&
                    t.Turma.Modulos.Any(tm => tm.ModuloId == t.ModuloId))
                .AsNoTracking()
                .ToListAsync();
        }

        // Retorna todas as turmas de um professor
        public async Task<List<Turma>> GetTurmasDoProfessorAsync(int professorId)
        {
            return await _context.Turmas
                .Include(t => t.Curso)
                .Include(t => t.Alunos)
                .Include(t => t.Professores)
                .Include(t => t.Modulos)
                .Where(t => t.Professores.Any(tp => tp.ProfessorId == professorId))
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
