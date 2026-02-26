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
                Console.WriteLine("[TurmaService] Iniciando GetTurmasAsync...");

                var turmas = await _context.Turmas
                    .Include(t => t.Curso)
                    .Include(t => t.Alunos)
                    .Include(t => t.Professores)
                        .ThenInclude(tp => tp.Professor)
                    .Include(t => t.Professores)
                        .ThenInclude(tp => tp.Disciplina)
                    .Include(t => t.Modulos)
                        .ThenInclude(tm => tm.Modulo)
                    .AsNoTracking()
                    .ToListAsync();

                Console.WriteLine($"[TurmaService] ✅ GetTurmasAsync retornou {turmas.Count} turmas");
                foreach (var t in turmas)
                {
                    Console.WriteLine($"  - Turma: {t.Nome} (ID: {t.Id}), Curso: {t.Curso?.Nome ?? "N/A"}, Alunos: {t.Alunos?.Count ?? 0}, Professores: {t.Professores?.Count ?? 0}");
                }

                return turmas;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [TurmaService] Erro em GetTurmasAsync: {ex.Message}");
                Console.WriteLine($"❌ [TurmaService] Stack Trace: {ex.StackTrace}");
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
                Console.WriteLine($"[TurmaService] 📝 Iniciando AddTurmaAsync para turma: {turma.Nome}");
                Console.WriteLine($"[TurmaService] - CursoId: {turma.CursoId}");
                Console.WriteLine($"[TurmaService] - Alunos: {turma.Alunos?.Count ?? 0}");
                Console.WriteLine($"[TurmaService] - Professores: {turma.Professores?.Count ?? 0}");

                // Garantir que as listas não são null
                if (turma.Alunos == null)
                    turma.Alunos = new();
                if (turma.Professores == null)
                    turma.Professores = new();
                if (turma.Modulos == null)
                    turma.Modulos = new();

                // Marca o curso como unchanged se existe
                if (turma.Curso != null)
                    _context.Entry(turma.Curso).State = EntityState.Unchanged;

                // Se há professores, marca como unchanged
                if (turma.Professores.Any())
                {
                    foreach (var tp in turma.Professores)
                    {
                        if (tp.Professor != null)
                            _context.Entry(tp.Professor).State = EntityState.Unchanged;
                        if (tp.Disciplina != null)
                            _context.Entry(tp.Disciplina).State = EntityState.Unchanged;
                    }
                }

                // Se há alunos, marca como unchanged
                if (turma.Alunos.Any())
                {
                    foreach (var aluno in turma.Alunos)
                    {
                        _context.Entry(aluno).State = EntityState.Unchanged;
                    }
                }

                _context.Turmas.Add(turma);
                await _context.SaveChangesAsync();

                Console.WriteLine($"✅ [TurmaService] Turma '{turma.Nome}' (ID: {turma.Id}) criada com sucesso!");
            }
            catch (Exception ex)
            {
                var msg = ex.InnerException?.Message ?? ex.Message;
                Console.WriteLine($"❌ [TurmaService] ERRO AO GRAVAR TURMA: {msg}");
                Console.WriteLine($"❌ [TurmaService] Stack Trace: {ex.StackTrace}");
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
            try
            {
                Console.WriteLine("[TurmaService] Carregando turmas SIMPLES (sem includes)...");
    
                var turmas = await _context.Turmas
                    .AsNoTracking()
                    .ToListAsync();
     
                Console.WriteLine($"[TurmaService] ✅ Turmas simples carregadas: {turmas.Count}");
   
                return turmas;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [TurmaService] Erro ao carregar turmas simples: {ex.Message}");
                return new List<Turma>();
            }
        }

        // Novo método: Obter turmas de um professor específico
        public async Task<List<Turma>> GetTurmasDoProfessorAsync(int professorId)
        {
            try
            {
                Console.WriteLine($"[TurmaService] Carregando turmas do professor {professorId}...");

                var turmas = await _context.Turmas
                    .Include(t => t.Curso)
                    .Include(t => t.Alunos)
                    .Include(t => t.Professores)
                    .Where(t => t.Professores.Any(tp => tp.ProfessorId == professorId))
                    .AsNoTracking()
                    .ToListAsync();

                Console.WriteLine($"[TurmaService] ✅ Turmas do professor carregadas: {turmas.Count}");

                return turmas;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [TurmaService] Erro ao carregar turmas do professor: {ex.Message}");
                return new List<Turma>();
            }
        }

        // Novo método melhorado com include gradual
        public async Task<List<Turma>> GetTurmasComCursoAsync()
        {
            try
            {
                Console.WriteLine("[TurmaService] Carregando turmas COM CURSO e ALUNOS...");

                var turmas = await _context.Turmas
                    .Include(t => t.Curso)
                    .Include(t => t.Alunos)
                    .AsNoTracking()
                    .ToListAsync();

                Console.WriteLine($"[TurmaService] ✅ Turmas com curso carregadas: {turmas.Count}");
                foreach (var t in turmas)
                {
                    Console.WriteLine($"  - {t.Nome}: {t.Alunos?.Count ?? 0} alunos");
                }

                return turmas;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [TurmaService] Erro ao carregar turmas com curso: {ex.Message}");
                Console.WriteLine($"❌ [TurmaService] Tentando fallback para turmas simples...");
     
                // Fallback: tenta carregar sem alunos
                try
                {
                    var turmasSimples = await _context.Turmas
                        .Include(t => t.Curso)
                        .AsNoTracking()
                        .ToListAsync();
     
                    Console.WriteLine($"[TurmaService] ✅ Fallback OK: {turmasSimples.Count} turmas");
                    return turmasSimples;
                }
                catch (Exception ex2)
                {
                    Console.WriteLine($"❌ [TurmaService] Fallback também falhou: {ex2.Message}");
                    return new List<Turma>();
                }
            }
        }
    }
}