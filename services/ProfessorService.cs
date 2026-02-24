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

        // NOVO MÉTODO: Adiciona um professor à base de dados
        // Este método resolve o erro de "AddProfessorAsync não encontrado"
        public async Task AddProfessorAsync(Professor professor)
        {
            if (professor == null) return;

            _context.Professores.Add(professor);
            await _context.SaveChangesAsync(); // O await aqui resolve o aviso de execução síncrona
        }

        // Retorna todos os professores
        public async Task<List<Professor>> GetProfessoresAsync()
        {
            return await _context.Professores.ToListAsync();
        }

        // Retorna professor pelo UserId do Identity
        public async Task<Professor?> GetProfessorByUserIdAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return null;

            return await _context.Professores
                                 .FirstOrDefaultAsync(p => p.UserId == userId);
        }

        // Retorna professor pelo Email (Username) do login
        public async Task<Professor?> GetProfessorByEmailAsync(string email)
        {
            if (string.IsNullOrEmpty(email)) return null;

            return await _context.Professores
                                 .FirstOrDefaultAsync(p => p.Email == email);
        }
    }
}
