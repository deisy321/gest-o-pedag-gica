using gestaopedagogica.Data;
using gestaopedagogica.Models;
using Microsoft.EntityFrameworkCore;

namespace gestaopedagogica.Services
{
    public class TrabalhoService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAService _iaService;
        private readonly PushService _pushService;

        public TrabalhoService(ApplicationDbContext context, IAService iaService, PushService pushService)
        {
            _context = context;
            _iaService = iaService;
            _pushService = pushService;
        }

        // --- MÉTODOS PARA O ALUNO ---

        public async Task<List<Trabalho>> GetTrabalhosDisponiveisParaAlunoAsync(string alunoUserId)
        {
            return await _context.Trabalhos
                .Include(t => t.TrabalhoVertentes)
                .Include(t => t.Modulo)
                .Include(t => t.Professor)
                .Include(t => t.Disciplina)
                .Where(t => t.AlunoId == alunoUserId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<Trabalho>> GetTrabalhosDoAlunoComVertentesAsync(string alunoUserId)
        {
            return await GetTrabalhosDisponiveisParaAlunoAsync(alunoUserId);
        }

        public async Task<List<TrabalhoVertente>> GetVertentesDoTrabalhoAsync(int trabalhoId)
        {
            return await _context.TrabalhoVertentes
                .Where(v => v.TrabalhoId == trabalhoId)
                .ToListAsync();
        }

        // --- MÉTODOS PARA O PROFESSOR ---

        public async Task<List<Trabalho>> GetTrabalhosDoProfessorAsync(string professorUserId)
        {
            return await _context.Trabalhos
                .Include(t => t.TrabalhoVertentes)
                .Include(t => t.Aluno)
                .Include(t => t.Modulo)
                .Where(t => t.ProfessorId == professorUserId)
                .OrderByDescending(t => t.DataEntrega)
                .ToListAsync();
        }

        public async Task AtualizarNotaEFeedbackAsync(int vertenteId, decimal? nota, string feedback)
        {
            var vertente = await _context.TrabalhoVertentes
                .Include(v => v.Trabalho)
                .FirstOrDefaultAsync(v => v.Id == vertenteId);

            if (vertente != null)
            {
                vertente.Nota = nota;
                vertente.Feedback = feedback;
                await _context.SaveChangesAsync();

                if (vertente.Trabalho != null)
                {
                    await EnviarNotificacaoParaUsuario(vertente.Trabalho.AlunoId,
                        $"Nota Lançada! Recebeste {nota} no trabalho: {vertente.Trabalho.Titulo}");
                }
            }
        }

        public async Task<bool> ApagarTrabalhoAsync(int trabalhoId)
        {
            try
            {
                var trabalho = await _context.Trabalhos
                    .Include(t => t.TrabalhoVertentes)
                    .FirstOrDefaultAsync(t => t.Id == trabalhoId);

                if (trabalho == null) return false;

                if (trabalho.TrabalhoVertentes != null && trabalho.TrabalhoVertentes.Count > 0)
                {
                    _context.TrabalhoVertentes.RemoveRange(trabalho.TrabalhoVertentes);
                }

                _context.Trabalhos.Remove(trabalho);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao apagar trabalho: {ex.Message}");
                return false;
            }
        }

        public async Task<int> AddTrabalhoAsync(Trabalho trabalho) => await CriarTrabalhoAsync(trabalho);

        public async Task<int> CriarTrabalhoAsync(Trabalho trabalho)
        {
            ArgumentNullException.ThrowIfNull(trabalho);

            trabalho.DataCriacao = DateTime.UtcNow;
            trabalho.PrazoEntrega = DateTime.SpecifyKind(trabalho.PrazoEntrega, DateTimeKind.Utc);

            _context.Trabalhos.Add(trabalho);
            await _context.SaveChangesAsync();

            await EnviarNotificacaoParaUsuario(trabalho.AlunoId,
                $"Novo Trabalho Atribuído: {trabalho.Titulo}. Prazo: {trabalho.PrazoEntrega:dd/MM}");

            return trabalho.Id;
        }

        public async Task AddVertenteAsync(TrabalhoVertente vertente)
        {
            _context.TrabalhoVertentes.Add(vertente);
            await _context.SaveChangesAsync();
        }

        // --- MÉTODOS DE APOIO / GENÉRICOS ---

        public async Task<Trabalho?> GetTrabalhoPorIdAsync(int id)
        {
            return await _context.Trabalhos
                .Include(t => t.TrabalhoVertentes)
                .Include(t => t.Aluno)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<TrabalhoVertente?> GetVertentePorIdAsync(int id) => await _context.TrabalhoVertentes.FindAsync(id);

        public async Task AtualizarVertenteAsync(TrabalhoVertente vertente)
        {
            if (vertente.DataEnvio.HasValue)
                vertente.DataEnvio = DateTime.SpecifyKind(vertente.DataEnvio.Value, DateTimeKind.Utc);

            _context.Entry(vertente).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            var trabalho = await _context.Trabalhos.FindAsync(vertente.TrabalhoId);
            if (trabalho != null)
            {
                trabalho.DataEntrega = vertente.DataEnvio;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> AtualizarTrabalhoAsync(Trabalho trabalho)
        {
            try
            {
                var dbObj = await _context.Trabalhos.FindAsync(trabalho.Id);
                if (dbObj == null) return false;

                dbObj.Titulo = trabalho.Titulo;
                dbObj.Descricao = trabalho.Descricao;
                dbObj.ConteudoTextoAluno = trabalho.ConteudoTextoAluno;
                dbObj.FicheiroNome = trabalho.FicheiroNome;
                dbObj.FicheiroBytes = trabalho.FicheiroBytes;
                dbObj.FicheiroContentType = trabalho.FicheiroContentType;

                if (trabalho.DataEntrega.HasValue)
                    dbObj.DataEntrega = DateTime.SpecifyKind(trabalho.DataEntrega.Value, DateTimeKind.Utc);

                dbObj.PrazoEntrega = DateTime.SpecifyKind(trabalho.PrazoEntrega, DateTimeKind.Utc);

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao atualizar: {ex.Message}");
                return false;
            }
        }

        public async Task<string> GerarFeedbackIAAsync(string alunoUserId, string conteudoAluno, byte[]? arquivoBytes, int trabalhoId, string vertenteId)
        {
            // Ajustado para bater com os parâmetros enviados pela Razor Page
            return await _iaService.ObterSugestoes(conteudoAluno, "", vertenteId, alunoUserId, trabalhoId.ToString(), arquivoBytes);
        }

        public async Task<string?> GetProfessorPushSubscriptionAsync(string professorId)
        {
            return await _context.PushSubscriptions
                .Where(s => s.UserId == professorId)
                .Select(s => s.Payload)
                .FirstOrDefaultAsync();
        }

        private async Task EnviarNotificacaoParaUsuario(string userId, string mensagem)
        {
            try
            {
                var subs = await _context.PushSubscriptions.Where(s => s.UserId == userId).ToListAsync();
                foreach (var sub in subs)
                {
                    await _pushService.EnviarNotificacaoAsync(sub.Payload, mensagem);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro push: {ex.Message}");
            }
        }

        public async Task<List<Trabalho>> GetTodosTrabalhosComNotasAsync()
        {
            return await _context.Trabalhos
                .Include(t => t.Aluno)
                .Include(t => t.Modulo)
                .Include(t => t.TrabalhoVertentes)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}