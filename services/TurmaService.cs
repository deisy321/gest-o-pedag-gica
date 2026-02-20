using gestaopedagogica.Data;
using gestaopedagogica.Models;
using Microsoft.EntityFrameworkCore;

namespace gestaopedagogica.Services
{
    public class TurmaService
    {
        private readonly ApplicationDbContext _context;

        public TurmaService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Turma>> GetTurmasAsync()
        {
            // CORREÇÃO: Adicionado Include para Professores e o ThenInclude para carregar o NOME do professor
            return await _context.Turmas
                .Include(t => t.Alunos)
                .Include(t => t.Professores)
                    .ThenInclude(tp => tp.Professor)
                .AsNoTracking() // Melhora a performance e garante dados frescos
                .ToListAsync();
        }

        public async Task<Turma?> GetTurmaByIdAsync(int id)
        {
            return await _context.Turmas
                .Include(t => t.Alunos)
                .Include(t => t.Professores)
                    .ThenInclude(tp => tp.Professor)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task AddTurmaAsync(Turma turma)
        {
            // Garante que o EF não tente criar professores duplicados, apenas a ligação
            _context.Turmas.Add(turma);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateTurmaAsync(Turma turma)
        {
            _context.Turmas.Update(turma);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveTurmaAsync(Turma turma)
        {
            _context.Turmas.Remove(turma);
            await _context.SaveChangesAsync();
        }
    }
}
