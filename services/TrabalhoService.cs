using gestaopedagogica.Data;
using gestaopedagogica.Models;
using Microsoft.EntityFrameworkCore;

namespace gestaopedagogica.Services;

// Aplicação de Construtor Primário para simplificar a classe e limpar avisos do VS
public class TrabalhoService(ApplicationDbContext context, IAService iaService, PushService pushService)
{
    private readonly ApplicationDbContext _context = context;
    private readonly IAService _iaService = iaService;
    private readonly PushService _pushService = pushService;

    // --- NOVA LÓGICA DE GESTÃO DE CARGA HORÁRIA (AJUSTADA PARA TURMAS) ---

    /// <summary>
    /// Calcula as horas já atribuídas a um aluno num módulo específico, mas ISOLADO por Turma.
    /// </summary>
    public async Task<int> GetHorasConsumidasNoModuloAsync(int? moduloId, int? turmaId, string alunoId)
    {
        if (moduloId == null || turmaId == null || string.IsNullOrEmpty(alunoId))
            return 0;

        return await _context.Trabalhos
            .Where(t => t.ModuloId == moduloId
                     && t.TurmaId == turmaId  // O segredo está aqui: isolar pela turma
                     && t.AlunoId == alunoId)
            .SumAsync(t => t.HorasAtribuidas);
    }

    // --- MÉTODOS DE CONSULTA PARA O ALUNO ---

    public async Task<List<Trabalho>> GetTrabalhosByAlunoAsync(string alunoUserId)
    {
        return await GetTrabalhosDoAlunoComVertentesAsync(alunoUserId);
    }

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
        return await _context.Trabalhos
            .Include(t => t.TrabalhoVertentes)
            .Include(t => t.Disciplina)
            .Include(t => t.Modulo)
            .Where(t => t.AlunoId == alunoUserId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<TrabalhoVertente>> GetVertentesDoTrabalhoAsync(int trabalhoId)
    {
        return await _context.TrabalhoVertentes
            .Where(v => v.TrabalhoId == trabalhoId)
            .ToListAsync();
    }

    // --- MÉTODOS DE CONSULTA PARA O PROFESSOR ---

    public async Task<List<Trabalho>> GetTrabalhosDoProfessorAsync(string professorUserId)
    {
        return await _context.Trabalhos
            .Include(t => t.TrabalhoVertentes)
            .Include(t => t.Aluno)
            .Include(t => t.Modulo)
            .Include(t => t.Disciplina)
            .Where(t => t.ProfessorId == professorUserId)
            .OrderByDescending(t => t.DataEntrega)
            .ToListAsync();
    }

    // --- LÓGICA DE AVALIAÇÃO E CRUD ---

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

            if (vertente.Trabalho != null && !string.IsNullOrEmpty(vertente.Trabalho.AlunoId))
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

        if (!string.IsNullOrEmpty(trabalho.AlunoId))
        {
            await EnviarNotificacaoParaUsuario(trabalho.AlunoId,
                $"Novo Trabalho Atribuído: {trabalho.Titulo}. Prazo: {trabalho.PrazoEntrega:dd/MM}");
        }
        return trabalho.Id;
    }

    public async Task AddVertenteAsync(TrabalhoVertente vertente)
    {
        _context.TrabalhoVertentes.Add(vertente);
        await _context.SaveChangesAsync();
    }

    public async Task<Trabalho?> GetTrabalhoPorIdAsync(int id)
    {
        return await _context.Trabalhos
            .Include(t => t.TrabalhoVertentes)
            .Include(t => t.Aluno)
            .Include(t => t.Professor)
            .Include(t => t.Modulo)
            .Include(t => t.Disciplina)
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
            dbObj.ConteudoTexto = trabalho.ConteudoTexto;
            dbObj.ConteudoTextoAluno = trabalho.ConteudoTextoAluno;
            dbObj.FicheiroNome = trabalho.FicheiroNome;
            dbObj.FicheiroBytes = trabalho.FicheiroBytes;
            dbObj.FicheiroContentType = trabalho.FicheiroContentType;
            dbObj.HorasAtribuidas = trabalho.HorasAtribuidas;

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

    // --- NOVA LÓGICA DE IA TURBINADA ---

    public async Task<string> GerarFeedbackIAAsync(string alunoUserId, string conteudoAluno, byte[]? arquivoBytes, int trabalhoId, string vertenteId)
    {
        var trabalho = await _context.Trabalhos
            .Include(t => t.Modulo)
            .Include(t => t.Disciplina)
            .Include(t => t.TrabalhoVertentes)
            .Include(t => t.Aluno)
            .Include(t => t.Professor)
            .FirstOrDefaultAsync(t => t.Id == trabalhoId);

        if (trabalho == null) return "Erro: Trabalho não localizado no sistema.";

        var dadosAluno = await _context.Set<Aluno>().FirstOrDefaultAsync(a => a.UserId == alunoUserId);

        string nomeExibicaoAluno = dadosAluno?.Nome ?? trabalho.Aluno?.UserName ?? "Estudante";
        string nomeExibicaoProfessor = trabalho.Professor?.UserName ?? "Docente";

        string instrucaoParaIA = trabalho.ConteudoTexto ?? "Realizar a tarefa proposta.";
        string tipoDeAnalise = "Geral";

        if (int.TryParse(vertenteId, out int vId) && vId > 0)
        {
            var vertente = trabalho.TrabalhoVertentes.FirstOrDefault(v => v.Id == vId);
            if (vertente != null)
            {
                instrucaoParaIA = vertente.ConteudoTexto ?? instrucaoParaIA;
                tipoDeAnalise = vertente.Tipo ?? "Tríade";
            }
        }

        string promptSistema = $@"
Tu és o Core Pedagógico da TriadeLearn. 
CONTEXTO ATUAL:
- Aluno: {nomeExibicaoAluno}
- Professor: {nomeExibicaoProfessor}
- Unidade: {trabalho.Modulo?.Nome ?? "Módulo Técnico"}
- Disciplina: {trabalho.Disciplina?.Nome ?? "Área Técnico"}
- Foco da Tríade: {tipoDeAnalise}

MISSÃO: 
Avalia a entrega do aluno com base na instrução do professor. 
- Se for 'Conhecimento', foca no rigor teórico.
- Se for 'Aptidão', foca na execução técnica.
- Se for 'Competência', foca na atitude profissional.
Fala diretamente para o aluno de forma construtiva.";

        return await _iaService.ObterSugestoes(
            promptSistemaOriginal: promptSistema,
            instrucaoPrincipal: instrucaoParaIA,
            conteudoParaAnalisar: conteudoAluno,
            alunoId: alunoUserId,
            trabalhoId: trabalhoId.ToString(),
            vertenteId: vertenteId,
            arquivoBytes: arquivoBytes
        );
    }

    // --- MÉTODOS DE NOTIFICAÇÃO E RELATÓRIO ---

    public async Task<List<NotificationSubscription>> GetSubscriptionsByUserIdAsync(string userId)
    {
        return await _context.PushSubscriptions
            .Where(s => s.UserId == userId)
            .ToListAsync();
    }

    public async Task EnviarNotificacaoParaUsuario(string userId, string mensagem)
    {
        try
        {
            var subs = await GetSubscriptionsByUserIdAsync(userId);
            foreach (var sub in subs)
            {
                if (!string.IsNullOrEmpty(sub.Payload))
                {
                    await _pushService.EnviarNotificacaoAsync(sub.Payload, message: mensagem, userId);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro push: {ex.Message}");
        }
    }

    public async Task<List<Trabalho>> GetTodosTrabalhosComNotasAsync()
    {
        try
        {
            return await _context.Trabalhos
                .Include(t => t.Aluno)
                .Include(t => t.Modulo)
                .Include(t => t.Disciplina)
                .Include(t => t.TrabalhoVertentes)
                .AsNoTracking()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao obter trabalhos para relatório: {ex.Message}");
            return []; // Simplificação de coleção: [] em vez de new List<Trabalho>()
        }
    }
}