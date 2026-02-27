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

        // 1. Retorna Turmas com Curso (Resolve o erro da página GerirTurmas)
        public async Task<List<Turma>> GetTurmasComCursoAsync()
        {
            return await GetTurmasAsync();
        }

        // 2. Retorna Turmas com todas as dependências (Usado no Admin)
        public async Task<List<Turma>> GetTurmasAsync()
        {
            try
            {
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
                Console.WriteLine($"❌ [TurmaService] Erro em GetTurmasAsync: {ex.Message}");
                return new List<Turma>();
            }
        }

        // 3. Obter turmas específicas de um Professor (Usado no Dashboard Professor)
        public async Task<List<Turma>> GetTurmasPorProfessorAsync(int professorId)
        {
            try
            {
                return await _context.Turmas
                    .Include(t => t.Curso)
                    .Include(t => t.Alunos)
                    .Where(t => t.Professores.Any(tp => tp.ProfessorId == professorId))
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [TurmaService] Erro ao carregar turmas do professor: {ex.Message}");
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

        // 4. Método de Criação de Turma (Resiliente a erros de tracking)
        public async Task AddTurmaAsync(Turma turma)
        {
            try
            {
                if (turma.Alunos == null) turma.Alunos = new();
                if (turma.Professores == null) turma.Professores = new();
                if (turma.Modulos == null) turma.Modulos = new();

                if (turma.Curso != null)
                    _context.Entry(turma.Curso).State = EntityState.Unchanged;

                if (turma.Professores.Any())
                {
                    foreach (var tp in turma.Professores)
                    {
                        if (tp.Professor != null) _context.Entry(tp.Professor).State = EntityState.Unchanged;
                        if (tp.Disciplina != null) _context.Entry(tp.Disciplina).State = EntityState.Unchanged;
                    }
                }

                if (turma.Alunos.Any())
                {
                    foreach (var aluno in turma.Alunos)
                        _context.Entry(aluno).State = EntityState.Unchanged;
                }

                _context.Turmas.Add(turma);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao gravar turma: {ex.InnerException?.Message ?? ex.Message}");
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

        // 5. Gestão de Módulos da Turma
        public async Task AddTurmaModuloAsync(int turmaId, int moduloId)
        {
            var existe = await _context.TurmaModulos.AnyAsync(tm => tm.TurmaId == turmaId && tm.ModuloId == moduloId);
            if (existe) return;

            _context.TurmaModulos.Add(new TurmaModulo { TurmaId = turmaId, ModuloId = moduloId });
            await _context.SaveChangesAsync();
        }

        public async Task RemoveTurmaModuloAsync(int turmaId, int moduloId)
        {
            var tm = await _context.TurmaModulos.FirstOrDefaultAsync(x => x.TurmaId == turmaId && x.ModuloId == moduloId);
            if (tm != null)
            {
                _context.TurmaModulos.Remove(tm);
                await _context.SaveChangesAsync();
            }
        }

        // 6. Gestão de Professores da Turma
        public async Task AddTurmaProfessorAsync(int turmaId, int professorId, int disciplinaId)
        {
            var existe = await _context.TurmaProfessores.AnyAsync(tp => tp.TurmaId == turmaId && tp.ProfessorId == professorId && tp.DisciplinaId == disciplinaId);
            if (existe) return;

            _context.TurmaProfessores.Add(new TurmaProfessor { TurmaId = turmaId, ProfessorId = professorId, DisciplinaId = disciplinaId });
            await _context.SaveChangesAsync();
        }

        public async Task RemoveTurmaProfessorAsync(int turmaId, int professorId, int disciplinaId)
        {
            var tp = await _context.TurmaProfessores.FirstOrDefaultAsync(x => x.TurmaId == turmaId && x.ProfessorId == professorId && x.DisciplinaId == disciplinaId);
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
