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

        // --- NOVO MÉTODO PARA FILTRAR DISCIPLINAS E MÓDULOS POR PROFESSOR E TURMA ---
        public async Task<List<Disciplina>> GetDisciplinasPorProfessorETurmaAsync(string professorUserId, int turmaId)
        {
            // 1. Primeiro, buscamos o ID numérico do Professor correspondente ao UserId da Identity
            var professor = await _context.Professores
                .FirstOrDefaultAsync(p => p.UserId == professorUserId);

            if (professor == null) return new List<Disciplina>();

            // 2. Agora usamos o professor.Id (int) para comparar com tp.ProfessorId (int)
            return await _context.TurmaProfessores
                .Where(tp => tp.ProfessorId == professor.Id && tp.TurmaId == turmaId)
                .Select(tp => tp.Disciplina)
                .Include(d => d.Modulos)
                .OrderBy(d => d.Nome)
                .ToListAsync();
        }

        public async Task<List<Curso>> GetCursosAsync() =>
            await _context.Cursos.OrderBy(c => c.Nome).ToListAsync();

        public async Task<List<Disciplina>> GetDisciplinasAsync() =>
            await _context.Disciplinas.Include(d => d.Curso).OrderBy(d => d.Nome).ToListAsync();

        public async Task AddDisciplinaAsync(Disciplina disciplina)
        {
            if (disciplina == null || disciplina.CursoId == 0)
                throw new Exception("Selecione um curso válido.");

            try
            {
                disciplina.Curso = null;
                _context.Disciplinas.Add(disciplina);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.InnerException?.Message ?? ex.Message);
            }
        }

        public async Task DeleteDisciplinaAsync(int id)
        {
            try
            {
                // Carrega a disciplina com todos os relacionamentos
                var disciplina = await _context.Disciplinas
                    .Include(d => d.Modulos)
                    .FirstOrDefaultAsync(d => d.Id == id);

                if (disciplina == null)
                    throw new Exception("Disciplina não encontrada.");

                // PASSO 1: Remove TurmaProfessor que referenciam esta disciplina
                var atribuicoesProfessores = await _context.TurmaProfessores
                    .Where(tp => tp.DisciplinaId == id)
                    .ToListAsync();

                if (atribuicoesProfessores.Any())
                {
                    _context.TurmaProfessores.RemoveRange(atribuicoesProfessores);
                    await _context.SaveChangesAsync();
                }

                // PASSO 2: Se houver módulos, remove todos os dependentes
                if (disciplina.Modulos != null && disciplina.Modulos.Any())
                {
                    var moduloIds = disciplina.Modulos.Select(m => m.Id).ToList();

                    // Remove comentários dos trabalhos destes módulos
                    var trabalhoIds = await _context.Trabalhos
                        .Where(t => t.ModuloId.HasValue && moduloIds.Contains(t.ModuloId.Value))
                        .Select(t => t.Id)
                        .ToListAsync();

                    if (trabalhoIds.Any())
                    {
                        var comentarios = await _context.Comentarios
                            .Where(c => trabalhoIds.Contains(c.TrabalhoId))
                            .ToListAsync();

                        if (comentarios.Any())
                            _context.Comentarios.RemoveRange(comentarios);

                        // Remove vertentes dos trabalhos
                        var vertentes = await _context.TrabalhoVertentes
                            .Where(tv => trabalhoIds.Contains(tv.TrabalhoId))
                            .ToListAsync();

                        if (vertentes.Any())
                            _context.TrabalhoVertentes.RemoveRange(vertentes);

                        // Remove os trabalhos
                        var trabalhos = await _context.Trabalhos
                            .Where(t => trabalhoIds.Contains(t.Id))
                            .ToListAsync();

                        if (trabalhos.Any())
                            _context.Trabalhos.RemoveRange(trabalhos);
                    }

                    // Remove TurmaModulo
                    var turmaModulos = await _context.TurmaModulos
                        .Where(tm => moduloIds.Contains(tm.ModuloId))
                        .ToListAsync();

                    if (turmaModulos.Any())
                        _context.TurmaModulos.RemoveRange(turmaModulos);

                    // Remove Modulos
                    _context.Modulos.RemoveRange(disciplina.Modulos);
                }

                // PASSO 3: Remove a disciplina
                _context.Disciplinas.Remove(disciplina);

                // Salva todas as mudanças
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException dbEx)
            {
                var innerMsg = dbEx.InnerException?.InnerException?.Message
                    ?? dbEx.InnerException?.Message
                    ?? dbEx.Message;
                throw new Exception($"Erro de banco de dados ao eliminar: {innerMsg}. Verifique se existem referências ativas a esta disciplina.");
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao eliminar disciplina: {ex.Message}");
            }
        }
    }
}