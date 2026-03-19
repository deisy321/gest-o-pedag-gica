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

        // Criar trabalho
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

        // Adicionar vertente
        public async Task AddVertenteAsync(TrabalhoVertente vertente)
        {
            vertente.Feedback ??= "";
            vertente.ConteudoTexto ??= "";
            vertente.ConteudoTextoAluno ??= "";

            _context.TrabalhoVertentes.Add(vertente);
            await _context.SaveChangesAsync();
        }

        // Atualizar vertente
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

        // Atualizar nota e feedback do professor
        public async Task<bool> AtualizarNotaEFeedbackAsync(int vertenteId, decimal? nota, string? feedback)
        {
            var vertente = await _context.TrabalhoVertentes.FindAsync(vertenteId);
            if (vertente == null) return false;

            vertente.Nota = nota;
            vertente.Feedback = feedback ?? "";
            await _context.SaveChangesAsync();
            return true;
        }

        // Obter trabalho por id com todas as relações
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

        // Obter trabalhos de um aluno com todas as vertentes (corrigido)
        public async Task<List<Trabalho>> GetTrabalhosDoAlunoComVertentesAsync(string alunoUserId)
        {
            if (string.IsNullOrEmpty(alunoUserId)) return new List<Trabalho>();

            var trabalhos = await _context.Trabalhos
                .Include(t => t.TrabalhoVertentes) // traz todas, mesmo não enviadas
                .Include(t => t.Modulo)
                .Include(t => t.Disciplina)
                .Include(t => t.Professor)
                .Include(t => t.Turma)
                .Where(t => t.AlunoId == alunoUserId)
                .AsNoTracking()
                .ToListAsync();

            return trabalhos;
        }

        // Obter trabalhos disponíveis para aluno (pela turma)
        public async Task<List<Trabalho>> GetTrabalhosDisponiveisParaAlunoAsync(string alunoUserId)
        {
            var aluno = await _context.Alunos.FirstOrDefaultAsync(a => a.UserId == alunoUserId);
            if (aluno?.TurmaId == null) return new List<Trabalho>();

            return await _context.Trabalhos
                .Include(t => t.Disciplina)
                .Include(t => t.Modulo)
                .Include(t => t.Professor)
                .Include(t => t.TrabalhoVertentes)
                .Include(t => t.Turma)
                .Where(t => t.TurmaId == aluno.TurmaId)
                .AsNoTracking()
                .ToListAsync();
        }

        // Obter trabalhos de um professor
        public async Task<List<Trabalho>> GetTrabalhosDoProfessorAsync(string professorUserId)
        {
            if (string.IsNullOrEmpty(professorUserId)) return new List<Trabalho>();

            return await _context.Trabalhos
                .Include(t => t.TrabalhoVertentes)
                .Include(t => t.Disciplina)
                .Include(t => t.Modulo)
                .Include(t => t.Turma)
                .Include(t => t.Aluno)
                .Where(t => t.ProfessorId == professorUserId)
                .AsNoTracking()
                .ToListAsync();
        }

        // Obter trabalhos por turma
        public async Task<List<Trabalho>> GetTrabalhosPorTurmaAsync(int turmaId)
        {
            return await _context.Trabalhos
                .Include(t => t.TrabalhoVertentes)
                .Include(t => t.Modulo)
                .Include(t => t.Disciplina)
                .Include(t => t.Professor)
                .Include(t => t.Turma)
                .AsNoTracking()
                .Where(t => t.TurmaId == turmaId)
                .ToListAsync();
        }

        // Obter todos os trabalhos com notas
        public async Task<List<Trabalho>> GetTodosTrabalhosComNotasAsync()
        {
            return await _context.Trabalhos
                .Include(t => t.TrabalhoVertentes)
                .Include(t => t.Turma)
                .ToListAsync();
        }

        // Apagar trabalho
        public async Task<bool> ApagarTrabalhoAsync(int trabalhoId)
        {
            var trabalho = await _context.Trabalhos.FindAsync(trabalhoId);
            if (trabalho == null) return false;

            _context.Trabalhos.Remove(trabalho);
            await _context.SaveChangesAsync();
            return true;
        }

        // Obter nome do aluno
        public async Task<string> ObterNomeAluno(string alunoUserId)
        {
            var aluno = await _context.Alunos.FirstOrDefaultAsync(a => a.UserId == alunoUserId);
            return aluno?.Nome ?? "Aluno Desconhecido";
        }
    }
}