using gestaopedagogica.Data;
using gestaopedagogica.Models;
using Microsoft.EntityFrameworkCore;

namespace gestaopedagogica.Services
{
    public class DisciplinaService
    {
        private readonly ApplicationDbContext _context;

        public DisciplinaService(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ NOVO MÉTODO: Carrega os cursos para o dropdown da página Criar
        public async Task<List<Curso>> GetCursosAsync()
        {
            try
            {
                return await _context.Cursos
                    .OrderBy(c => c.Nome)
                    .ToListAsync() ?? new List<Curso>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERRO SQL CURSOS: {ex.Message}");
                return new List<Curso>();
            }
        }

        public async Task<List<Disciplina>> GetDisciplinasAsync()
        {
            try
            {
                return await _context.Disciplinas
                    .Include(d => d.Curso) // ✅ Carrega o Curso obrigatório
                    .OrderBy(d => d.Nome)
                    .ToListAsync() ?? new List<Disciplina>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERRO SQL DISCIPLINAS: {ex.Message}");
                return new List<Disciplina>();
            }
        }

        public async Task AddDisciplinaAsync(Disciplina disciplina)
        {
            if (disciplina == null) return;

            // Se o CursoId for 0, o EF pode tentar criar um curso novo ou dar erro de FK
            if (disciplina.CursoId == 0) throw new Exception("Selecione um curso válido.");

            _context.Disciplinas.Add(disciplina);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteDisciplinaAsync(int id)
        {
            var disciplina = await _context.Disciplinas.FindAsync(id);
            if (disciplina != null)
            {
                _context.Disciplinas.Remove(disciplina);
                await _context.SaveChangesAsync();
            }
        }
    }
}
