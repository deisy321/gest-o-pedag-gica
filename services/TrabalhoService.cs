using gestaopedagogica.Data;
using gestaopedagogica.Models;
using Microsoft.EntityFrameworkCore;

namespace gestaopedagogica.Services
{
    public class TrabalhoService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAService _iaService;

        public TrabalhoService(ApplicationDbContext context, IAService iaService)
        {
            _context = context;
            _iaService = iaService;
        }

        // =============================
        // CRIAR TRABALHO E VERTENTES
        // =============================
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

        public async Task AddVertenteAsync(TrabalhoVertente vertente)
        {
            vertente.Feedback ??= "";
            vertente.ConteudoTexto ??= "";
            vertente.ConteudoTextoAluno ??= "";

            _context.TrabalhoVertentes.Add(vertente);
            await _context.SaveChangesAsync();
        }

        public async Task AtualizarVertenteAsync(TrabalhoVertente vertente)
        {
            var existente = await _context.TrabalhoVertentes.FindAsync(vertente.Id);
            if (existente == null) throw new Exception("Vertente não encontrada.");

            existente.Tipo = vertente.Tipo;
            existente.ConteudoTexto = vertente.ConteudoTexto ?? "";
            existente.ConteudoTextoAluno = vertente.ConteudoTextoAluno ?? "";
            existente.Feedback = vertente.Feedback ?? "";
            existente.Nota = vertente.Nota;

            if (!string.IsNullOrEmpty(vertente.ConteudoTextoAluno) || vertente.FicheiroBytes != null)
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

        // =============================
        // OBTER TRABALHOS (COM LIMPEZA DE DUPLICADOS)
        // =============================

        public async Task<Trabalho?> GetTrabalhoPorIdAsync(int id)
        {
            return await _context.Trabalhos
                .Include(t => t.TrabalhoVertentes)
                .Include(t => t.Disciplina)
                .Include(t => t.Modulo)
                .Include(t => t.Professor)
                .Include(t => t.Aluno)
                .Include(t => t.Turma)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<List<Trabalho>> GetTrabalhosDoAlunoComVertentesAsync(string alunoUserId)
        {
            if (string.IsNullOrEmpty(alunoUserId)) return new List<Trabalho>();

            var lista = await _context.Trabalhos
                .Include(t => t.TrabalhoVertentes)
                .Include(t => t.Modulo)
                .Include(t => t.Disciplina)
                .Include(t => t.Professor)
                .Include(t => t.Turma)
                .Where(t => t.AlunoId == alunoUserId)
                .AsNoTracking()
                .ToListAsync();

            // Resolve duplicados na memória antes de retornar
            return lista.DistinctBy(t => t.Id).ToList();
        }

        public async Task<List<Trabalho>> GetTrabalhosDisponiveisParaAlunoAsync(string alunoUserId)
        {
            var aluno = await _context.Alunos.AsNoTracking().FirstOrDefaultAsync(a => a.UserId == alunoUserId);
            if (aluno?.TurmaId == null) return new List<Trabalho>();

            var lista = await _context.Trabalhos
                .Include(t => t.TrabalhoVertentes)
                .Include(t => t.Modulo)
                .Include(t => t.Disciplina)
                .Include(t => t.Professor)
                .Where(t => t.TurmaId == aluno.TurmaId)
                .AsNoTracking()
                .ToListAsync();

            // Resolve duplicados na memória antes de retornar
            return lista.DistinctBy(t => t.Id).ToList();
        }

        public async Task<List<Trabalho>> GetTrabalhosDoProfessorAsync(string professorUserId)
        {
            var lista = await _context.Trabalhos
                .Include(t => t.TrabalhoVertentes)
                .Include(t => t.Modulo)
                .Include(t => t.Disciplina)
                .Include(t => t.Turma)
                .Where(t => t.ProfessorId == professorUserId)
                .AsNoTracking()
                .ToListAsync();

            return lista.DistinctBy(t => t.Id).ToList();
        }

        public async Task<List<Trabalho>> GetTrabalhosPorTurmaAsync(int turmaId)
        {
            var lista = await _context.Trabalhos
                .Include(t => t.TrabalhoVertentes)
                .Include(t => t.Modulo)
                .Include(t => t.Disciplina)
                .Include(t => t.Professor)
                .Where(t => t.TurmaId == turmaId)
                .AsNoTracking()
                .ToListAsync();

            return lista.DistinctBy(t => t.Id).ToList();
        }

        public async Task<List<Trabalho>> GetTodosTrabalhosComNotasAsync()
        {
            var lista = await _context.Trabalhos
                .Include(t => t.TrabalhoVertentes)
                .Include(t => t.Turma)
                .AsNoTracking()
                .ToListAsync();

            return lista.DistinctBy(t => t.Id).ToList();
        }

        // =============================
        // AUXILIARES E FEEDBACK
        // =============================

        public async Task<List<TrabalhoVertente>> GetVertentesDoTrabalhoAsync(int trabalhoId)
        {
            return await _context.TrabalhoVertentes
                .Where(v => v.TrabalhoId == trabalhoId)
                .ToListAsync();
        }

        public async Task<TrabalhoVertente?> GetVertentePorIdAsync(int vertenteId)
        {
            return await _context.TrabalhoVertentes
                .Include(v => v.Trabalho)
                .FirstOrDefaultAsync(v => v.Id == vertenteId);
        }

        public async Task<string> ObterNomeAluno(string alunoUserId)
        {
            var aluno = await _context.Alunos.FirstOrDefaultAsync(a => a.UserId == alunoUserId);
            return aluno?.Nome ?? "Aluno Desconhecido";
        }

        public async Task<bool> ApagarTrabalhoAsync(int trabalhoId)
        {
            var trabalho = await _context.Trabalhos.FindAsync(trabalhoId);
            if (trabalho == null) return false;

            _context.Trabalhos.Remove(trabalho);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<string> GerarFeedbackIAAsync(string alunoUserId, string conteudoAluno, byte[]? arquivoBytes, int trabalhoId, string vertenteId)
        {
            if (string.IsNullOrWhiteSpace(conteudoAluno) && arquivoBytes == null)
                return "Conteúdo vazio.";

            var trabalho = await GetTrabalhoPorIdAsync(trabalhoId);
            if (trabalho == null) return "Trabalho não encontrado.";

            var descricaoTrabalho = trabalho.Descricao ?? trabalho.Titulo ?? "Descrição indisponível";

            return await _iaService.ObterSugestoes(conteudoAluno, descricaoTrabalho, vertenteId, alunoUserId, trabalhoId.ToString(), arquivoBytes);
        }

        public async Task<Dictionary<int, string>> GerarFeedbackIAPorVertenteAsync(int trabalhoId)
        {
            var trabalho = await GetTrabalhoPorIdAsync(trabalhoId);
            var feedbacks = new Dictionary<int, string>();
            if (trabalho?.TrabalhoVertentes == null) return feedbacks;

            foreach (var vertente in trabalho.TrabalhoVertentes)
            {
                if (string.IsNullOrWhiteSpace(vertente.ConteudoTextoAluno) && vertente.FicheiroBytes == null)
                {
                    feedbacks[vertente.Id] = "Vazio.";
                    continue;
                }
                var desc = trabalho.Descricao ?? trabalho.Titulo ?? "";
                feedbacks[vertente.Id] = await _iaService.ObterSugestoes(vertente.ConteudoTextoAluno ?? "", desc, vertente.Id.ToString(), trabalho.AlunoId ?? "", trabalhoId.ToString(), vertente.FicheiroBytes);
            }
            return feedbacks;
        }
    }
}