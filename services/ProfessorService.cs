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

        // Retorna todos os professores
        public async Task<List<Professor>> GetProfessoresAsync()
        {
            return await _context.Professores.ToListAsync();
        }

        // Retorna professor pelo UserId do Identity
        public async Task<Professor?> GetProfessorByUserIdAsync(string userId)
        {
            return await _context.Professores
                                 .FirstOrDefaultAsync(p => p.UserId == userId);
        }
    }
}
