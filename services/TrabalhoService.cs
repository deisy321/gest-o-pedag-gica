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

        public async Task<int> CriarTrabalhoAsync(Trabalho trabalho)
        {
            if (trabalho == null) throw new ArgumentNullException(nameof(trabalho));
            trabalho.DataCriacao = DateTime.UtcNow;
            trabalho.ConteudoTexto ??= "";
            trabalho.TrabalhoVertentes ??= new List<TrabalhoVertente>();

            if (!trabalho.IsPlanoRecuperacao)
            {
                if (trabalho.ModuloId <= 0) throw new Exception("ModuloId é obrigatório.");
                if (trabalho.DisciplinaId <= 0) throw new Exception("DisciplinaId é obrigatório.");
            }

            if (trabalho.PrazoEntrega == default)
                trabalho.PrazoEntrega = DateTime.UtcNow.AddDays(7);

            if (trabalho.PrazoEntrega.Kind != DateTimeKind.Utc)
                trabalho.PrazoEntrega = DateTime.SpecifyKind(trabalho.PrazoEntrega, DateTimeKind.Utc);

            _context.Trabalhos.Add(trabalho);
            await _context.SaveChangesAsync();
            return trabalho.Id;
        }

        public async Task<int> AddTrabalhoAsync(Trabalho trabalho) => await CriarTrabalhoAsync(trabalho);

        public async Task AddVertenteAsync(TrabalhoVertente vertente)
        {
            vertente.Feedback ??= "";
            vertente.ConteudoTexto ??= "";
            vertente.ConteudoTextoAluno ??= "";
            _context.TrabalhoVertentes.Add(vertente);
            await _context.SaveChangesAsync();
        }

        // --- MÉTODOS DE CONSULTA (ESSENCIAIS PARA O BUILD) ---

        public async Task<TrabalhoVertente?> GetVertentePorIdAsync(int vertenteId)
        {
            return await _context.TrabalhoVertentes
                .Include(v => v.Trabalho)
                .FirstOrDefaultAsync(v => v.Id == vertenteId);
        }

        public async Task<List<TrabalhoVertente>> GetVertentesDoTrabalhoAsync(int trabalhoId)
        {
            return await _context.TrabalhoVertentes
                .Where(v => v.TrabalhoId == trabalhoId)
                .ToListAsync();
        }

        public async Task<List<Trabalho>> GetTrabalhosDoAlunoComVertentesAsync(string alunoUserId)
        {
            return await _context.Trabalhos
                .Include(t => t.TrabalhoVertentes)
                .Include(t => t.Disciplina)
                .Include(t => t.Modulo)
                .Where(t => t.AlunoId == alunoUserId)
                .AsNoTracking()
                .ToListAsync();
        }

        // NOVO: Necessário para RelatóriosGlobais.razor (Erro na imagem 6ccd26)
        public async Task<List<Trabalho>> GetTodosTrabalhosComNotasAsync()
        {
            return await _context.Trabalhos
                .Include(t => t.TrabalhoVertentes)
                .Include(t => t.Disciplina)
                .Include(t => t.Modulo)
                .Include(t => t.Aluno)
                .AsNoTracking()
                .ToListAsync();
        }

        // NOVO: Necessário para RelatoriosProfessor.razor (Erro na imagem 6ccd26)
        public async Task<List<Trabalho>> GetTodosTrabalhosComNotasAsync(string professorUserId)
        {
            return await _context.Trabalhos
                .Include(t => t.TrabalhoVertentes)
                .Include(t => t.Disciplina)
                .Include(t => t.Modulo)
                .Include(t => t.Aluno)
                .Where(t => t.ProfessorId == professorUserId)
                .AsNoTracking()
                .ToListAsync();
        }

        // --- MÉTODOS DE ATUALIZAÇÃO ---

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

        public async Task<List<Trabalho>> GetTrabalhosDisponiveisParaAlunoAsync(string alunoUserId)
        {
            var aluno = await _context.Alunos.AsNoTracking().FirstOrDefaultAsync(a => a.UserId == alunoUserId);
            if (aluno?.TurmaId == null) return new List<Trabalho>();

            return await _context.Trabalhos
                .Include(t => t.TrabalhoVertentes)
                .Include(t => t.Modulo)
                .Include(t => t.Disciplina)
                .Include(t => t.Professor)
                .Where(t => t.TurmaId == aluno.TurmaId || t.AlunoId == alunoUserId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<Trabalho>> GetTrabalhosDoProfessorAsync(string professorUserId)
        {
            return await _context.Trabalhos
                .Include(t => t.TrabalhoVertentes)
                .Include(t => t.Modulo)
                .Include(t => t.Disciplina)
                .Include(t => t.Turma)
                .Where(t => t.ProfessorId == professorUserId)
                .AsNoTracking()
                .ToListAsync();
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
            if (string.IsNullOrWhiteSpace(conteudoAluno) && arquivoBytes == null) return "Conteúdo vazio.";
            if (!int.TryParse(vertenteId, out int vId)) return "ID de vertente inválido.";
            var vertente = await _context.TrabalhoVertentes.FindAsync(vId);
            if (vertente == null) return "Vertente não encontrada.";

            string instrucaoIA = !string.IsNullOrWhiteSpace(vertente.ConteudoTexto)
                ? vertente.ConteudoTexto
                : (await _context.Trabalhos.FindAsync(trabalhoId))?.Descricao ?? "Instrução indisponível";

            return await _iaService.ObterSugestoes(conteudoAluno, instrucaoIA, vertenteId, alunoUserId, trabalhoId.ToString(), arquivoBytes);
        }
    }
}