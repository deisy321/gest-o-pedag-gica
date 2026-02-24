using gestaopedagogica.Data;
using gestaopedagogica.Models;
using Microsoft.EntityFrameworkCore;

namespace gestaopedagogica.Services
{
    public class AlunoService
    {
        private readonly ApplicationDbContext _context;

        public AlunoService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Obter todos os alunos
        public async Task<List<Aluno>> GetAlunosAsync()
        {
            return await _context.Alunos
                .Include(a => a.Turma)
                .AsNoTracking()
                .ToListAsync();
        }

        // Obter um aluno pelo ID
        public async Task<Aluno?> GetAlunoByIdAsync(int id)
        {
            return await _context.Alunos
                .Include(a => a.Turma)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        // Adicionar um novo aluno
        public async Task AddAlunoAsync(Aluno aluno)
        {
            _context.Alunos.Add(aluno);
            await _context.SaveChangesAsync();
        }

        // Atualizar um aluno existente
        public async Task UpdateAlunoAsync(Aluno aluno)
        {
            _context.Alunos.Update(aluno);
            await _context.SaveChangesAsync();
        }

        // Remover um aluno
        public async Task RemoveAlunoAsync(Aluno aluno)
        {
            _context.Alunos.Remove(aluno);
            await _context.SaveChangesAsync();
        }
    }
}
