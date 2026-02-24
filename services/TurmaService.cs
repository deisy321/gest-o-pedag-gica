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

        // Retorna Turmas com todas as dependências carregadas
        public async Task<List<Turma>> GetTurmasAsync()
        {
            try
            {
                // Adicionamos .Include(t => t.Curso) para garantir que a turma apareça na lista
                return await _context.Turmas
                    .Include(t => t.Curso)
                    .Include(t => t.Alunos)
                    .Include(t => t.Professores).ThenInclude(tp => tp.Professor)
                    .Include(t => t.Professores).ThenInclude(tp => tp.Disciplina)
                    .Include(t => t.Modulos).ThenInclude(tm => tm.Modulo)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro em GetTurmasAsync: {ex.Message}");
                return new List<Turma>();
            }
        }

        public async Task<Turma?> GetTurmaByIdAsync(int id)
        {
            return await _context.Turmas
                .Include(t => t.Curso)
                .Include(t => t.Alunos)
                .Include(t => t.Professores).ThenInclude(tp => tp.Professor)
                .Include(t => t.Modulos).ThenInclude(tm => tm.Modulo)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        // MÉTODO DE CRIAÇÃO CORRIGIDO (Resiliente a erros de tracking)
        public async Task AddTurmaAsync(Turma turma)
        {
            try
            {
                // Marcamos objetos relacionados como "Unchanged" para o EF não tentar criá-los de novo
                if (turma.Curso != null)
                    _context.Entry(turma.Curso).State = EntityState.Unchanged;

                if (turma.Professores != null)
                {
                    foreach (var tp in turma.Professores)
                    {
                        if (tp.Professor != null) _context.Entry(tp.Professor).State = EntityState.Unchanged;
                        if (tp.Disciplina != null) _context.Entry(tp.Disciplina).State = EntityState.Unchanged;
                    }
                }

                if (turma.Alunos != null)
                {
                    foreach (var aluno in turma.Alunos)
                    {
                        _context.Entry(aluno).State = EntityState.Unchanged;
                    }
                }

                _context.Turmas.Add(turma);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                var msg = ex.InnerException?.Message ?? ex.Message;
                Console.WriteLine($"ERRO AO GRAVAR TURMA: {msg}");
                throw;
            }
        }

        public async Task RemoveTurmaAsync(Turma turma)
        {
            var t = await _context.Turmas.FindAsync(turma.Id);
            if (t != null)
            {
                _context.Turmas.Remove(t);
                await _context.SaveChangesAsync();
            }
        }

        // --- MÉTODOS DE MÓDULOS (Recuperados para tirar o erro de compilação) ---
        public async Task AddTurmaModuloAsync(int turmaId, int moduloId)
        {
            var existe = await _context.TurmaModulos
                .AnyAsync(tm => tm.TurmaId == turmaId && tm.ModuloId == moduloId);

            if (existe) return;

            var turmaModulo = new TurmaModulo { TurmaId = turmaId, ModuloId = moduloId };
            _context.TurmaModulos.Add(turmaModulo);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveTurmaModuloAsync(int turmaId, int moduloId)
        {
            var turmaModulo = await _context.TurmaModulos
                .FirstOrDefaultAsync(tm => tm.TurmaId == turmaId && tm.ModuloId == moduloId);

            if (turmaModulo != null)
            {
                _context.TurmaModulos.Remove(turmaModulo);
                await _context.SaveChangesAsync();
            }
        }

        // --- MÉTODOS DE PROFESSORES ---
        public async Task AddTurmaProfessorAsync(int turmaId, int professorId, int disciplinaId)
        {
            var existe = await _context.TurmaProfessores
                .AnyAsync(tp => tp.TurmaId == turmaId && tp.ProfessorId == professorId && tp.DisciplinaId == disciplinaId);

            if (existe) return;

            var tp = new TurmaProfessor { TurmaId = turmaId, ProfessorId = professorId, DisciplinaId = disciplinaId };
            _context.TurmaProfessores.Add(tp);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveTurmaProfessorAsync(int turmaId, int professorId, int disciplinaId)
        {
            var tp = await _context.TurmaProfessores
                .FirstOrDefaultAsync(x => x.TurmaId == turmaId && x.ProfessorId == professorId && x.DisciplinaId == disciplinaId);

            if (tp != null)
            {
                _context.TurmaProfessores.Remove(tp);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Turma>> GetTurmasSimplesAsync()
        {
            return await _context.Turmas.AsNoTracking().ToListAsync();
        }
    }
}
