using gestaopedagogica.Data;
using gestaopedagogica.Models;
using Microsoft.EntityFrameworkCore;

namespace gestaopedagogica.Services
{
    public class TrabalhoService
    {
        private readonly ApplicationDbContext _context;

        public TrabalhoService(ApplicationDbContext context)
        {
            _context = context;
        }

        // ------------------------------
        // Criar trabalho único
        // ------------------------------
        public async Task<int> AddTrabalhoAsync(Trabalho trabalho)
        {
            if (trabalho == null) throw new ArgumentNullException(nameof(trabalho));

            trabalho.DataCriacao = DateTime.UtcNow;
            trabalho.ConteudoTexto ??= "";
            trabalho.TrabalhoVertentes ??= new List<TrabalhoVertente>();

            if (trabalho.ModuloId <= 0) throw new Exception("ModuloId é obrigatório.");
            if (trabalho.DisciplinaId <= 0) throw new Exception("DisciplinaId é obrigatório.");
            if (trabalho.PrazoEntrega == default) throw new Exception("PrazoEntrega é obrigatório.");

            if (trabalho.PrazoEntrega.Kind != DateTimeKind.Utc)
                trabalho.PrazoEntrega = DateTime.SpecifyKind(trabalho.PrazoEntrega, DateTimeKind.Utc);

            _context.Trabalhos.Add(trabalho);
            await _context.SaveChangesAsync();
            return trabalho.Id;
        }

        // ------------------------------
        // Adicionar vertente ao trabalho
        // ------------------------------
        public async Task AddVertenteAsync(TrabalhoVertente vertente)
        {
            vertente.Feedback ??= "";
            vertente.ConteudoTexto ??= "";
            vertente.ConteudoTextoAluno ??= "";

            _context.TrabalhoVertentes.Add(vertente);
            await _context.SaveChangesAsync();
        }

        // ------------------------------
        // Consultas e Listagens
        // ------------------------------

        public async Task<Trabalho?> GetTrabalhoPorIdAsync(int id)
        {
            return await _context.Trabalhos
                .Include(t => t.TrabalhoVertentes)
                .Include(t => t.Disciplina)
                .Include(t => t.Modulo)
                .Include(t => t.Aluno)
                .Include(t => t.Turma)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        // RESOLVE O ERRO: GetVertentePorIdAsync
        public async Task<TrabalhoVertente?> GetVertentePorIdAsync(int vertenteId)
        {
            return await _context.TrabalhoVertentes
                .Include(v => v.Trabalho)
                .FirstOrDefaultAsync(v => v.Id == vertenteId);
        }

        // RESOLVE O ERRO: GetVertentesDoTrabalhoAsync
        public async Task<List<TrabalhoVertente>> GetVertentesDoTrabalhoAsync(int trabalhoId)
        {
            return await _context.TrabalhoVertentes
                .Where(v => v.TrabalhoId == trabalhoId)
                .ToListAsync();
        }

        public async Task<List<Trabalho>> GetTrabalhosPorTurmaAsync(int turmaId)
        {
            return await _context.Trabalhos
                .Include(t => t.TrabalhoVertentes)
                .Include(t => t.Aluno)
                .Include(t => t.Disciplina)
                .Include(t => t.Modulo)
                .Where(t => t.TurmaId == turmaId)
                .AsNoTracking()
                .OrderByDescending(t => t.DataCriacao)
                .ToListAsync();
        }

        public async Task<List<Trabalho>> GetTrabalhosDisponiveisParaAlunoAsync(string alunoId)
        {
            var agora = DateTime.UtcNow;

            return await _context.Trabalhos
                .Include(t => t.TrabalhoVertentes)
                .Include(t => t.Aluno)
                .Include(t => t.Turma)
                .Where(t => (t.AlunoId == null || t.AlunoId == alunoId) && t.PrazoEntrega >= agora)
                .OrderBy(t => t.PrazoEntrega)
                .ToListAsync();
        }

        public async Task<List<Trabalho>> GetTrabalhosDoProfessorAsync(string professorId)
        {
            return await _context.Trabalhos
                .Include(t => t.TrabalhoVertentes)
                .Include(t => t.Disciplina)
                .Include(t => t.Modulo)
                .Include(t => t.Turma)
                .Include(t => t.Aluno)
                .Where(t => t.ProfessorId == professorId)
                .OrderByDescending(t => t.DataCriacao)
                .ToListAsync();
        }

        // ------------------------------
        // Atualizações
        // ------------------------------
        public async Task AtualizarVertenteAsync(TrabalhoVertente vertente)
        {
            var existente = await _context.TrabalhoVertentes.FindAsync(vertente.Id);
            if (existente == null) throw new Exception("Vertente não encontrada.");

            existente.Tipo = vertente.Tipo;
            existente.ConteudoTexto = vertente.ConteudoTexto ?? "";
            existente.ConteudoTextoAluno = vertente.ConteudoTextoAluno ?? "";
            existente.Feedback = vertente.Feedback ?? "";
            existente.Nota = vertente.Nota;

            if (vertente.FicheiroBytes != null || !string.IsNullOrEmpty(vertente.ConteudoTextoAluno))
                existente.DataEnvio ??= DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task<bool> AtualizarNotaEFeedbackAsync(int vertenteId, decimal? nota, string? feedback)
        {
            var vertente = await _context.TrabalhoVertentes.FindAsync(vertenteId);
            if (vertente == null) return false;

            vertente.Nota = nota;
            vertente.Feedback = feedback ?? "";

            await _context.SaveChangesAsync();
            return true;
        }

        // ------------------------------
        // Eliminação
        // ------------------------------
        public async Task ApagarTrabalhoAsync(int trabalhoId)
        {
            var trabalho = await _context.Trabalhos
                .Include(t => t.TrabalhoVertentes)
                .FirstOrDefaultAsync(t => t.Id == trabalhoId);

            if (trabalho != null)
            {
                _context.TrabalhoVertentes.RemoveRange(trabalho.TrabalhoVertentes);
                _context.Trabalhos.Remove(trabalho);
                await _context.SaveChangesAsync();
            }
        }

        // ------------------------------
        // Helpers
        // ------------------------------
        public string ObterNomeAluno(ApplicationUser? aluno)
        {
            if (aluno == null) return "Desconhecido";
            return !string.IsNullOrEmpty(aluno.UserName) ? aluno.UserName : aluno.Email ?? "Sem Identificador";
        }
    }
}
