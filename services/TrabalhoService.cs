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
        // Criar trabalho e vertentes padrão
        // ------------------------------
        public async Task AddTrabalhoAsync(Trabalho trabalho)
        {
            trabalho.DataCriacao = DateTime.UtcNow;

            if (trabalho.PrazoEntrega.Kind != DateTimeKind.Utc)
                trabalho.PrazoEntrega = DateTime.SpecifyKind(trabalho.PrazoEntrega, DateTimeKind.Utc);

            _context.Trabalhos.Add(trabalho);
            await _context.SaveChangesAsync();

            var vertentes = new List<TrabalhoVertente>
            {
                new TrabalhoVertente { TrabalhoId = trabalho.Id, Tipo = "Competência" },
                new TrabalhoVertente { TrabalhoId = trabalho.Id, Tipo = "Aptidão" },
                new TrabalhoVertente { TrabalhoId = trabalho.Id, Tipo = "Conhecimento" }
            };

            _context.TrabalhoVertentes.AddRange(vertentes);
            await _context.SaveChangesAsync();
        }

        // ------------------------------
        // Listagens
        // ------------------------------
        public async Task<List<Trabalho>> GetTrabalhosDoProfessorAsync(string professorId)
        {
            return await _context.Trabalhos
                .Include(t => t.TrabalhoVertentes)
                .Where(t => t.ProfessorId == professorId)
                .OrderByDescending(t => t.DataCriacao)
                .ToListAsync();
        }

        public async Task<List<Trabalho>> GetTrabalhosDisponiveisParaAlunoAsync(string alunoId)
        {
            return await _context.Trabalhos
                .Include(t => t.TrabalhoVertentes)
                .OrderByDescending(t => t.DataCriacao)
                .ToListAsync();
        }

        public async Task<Trabalho?> GetTrabalhoPorIdAsync(int id)
        {
            return await _context.Trabalhos
                .Include(t => t.TrabalhoVertentes)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<TrabalhoVertente?> GetVertentePorIdAsync(int id)
        {
            return await _context.TrabalhoVertentes
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task<List<TrabalhoVertente>> GetVertentesDoTrabalhoAsync(int trabalhoId)
        {
            return await _context.TrabalhoVertentes
                .Where(v => v.TrabalhoId == trabalhoId)
                .ToListAsync();
        }

        // ------------------------------
        // Atualizar vertente (professor ou aluno)
        // ------------------------------
        public async Task AtualizarVertenteAsync(TrabalhoVertente vertente)
        {
            // Marca como enviado automaticamente se houver conteúdo
            if ((vertente.FicheiroBytes != null && vertente.FicheiroBytes.Length > 0)
                || !string.IsNullOrEmpty(vertente.ConteudoTextoAluno))
            {
                vertente.DataEnvio ??= DateTime.UtcNow;
            }

            _context.TrabalhoVertentes.Update(vertente);
            await _context.SaveChangesAsync();
        }

        // ------------------------------
        // Envio de vertente (aluno)
        // ------------------------------
        public async Task EnviarVertenteAsync(int vertenteId, byte[]? arquivo = null, string? conteudoTexto = null)
        {
            var vertente = await _context.TrabalhoVertentes.FirstOrDefaultAsync(v => v.Id == vertenteId);
            if (vertente == null) return;

            if (arquivo != null && arquivo.Length > 0)
            {
                vertente.FicheiroBytes = arquivo;
                vertente.FicheiroContentType ??= "application/octet-stream";
                vertente.FicheiroNome ??= "arquivo_enviado";
            }

            if (!string.IsNullOrEmpty(conteudoTexto))
            {
                vertente.ConteudoTextoAluno = conteudoTexto;
            }

            // Marca como enviado
            vertente.DataEnvio ??= DateTime.UtcNow;

            _context.TrabalhoVertentes.Update(vertente);
            await _context.SaveChangesAsync();
        }

        // ------------------------------
        // Adicionar vertente manualmente
        // ------------------------------
        public async Task AddVertenteAsync(TrabalhoVertente vertente)
        {
            vertente.Feedback ??= "";
            vertente.ConteudoTexto ??= "";
            vertente.ConteudoTextoAluno ??= "";
            vertente.FicheiroBytes ??= null;
            vertente.FicheiroNome ??= null;
            vertente.FicheiroContentType ??= null;

            _context.TrabalhoVertentes.Add(vertente);
            await _context.SaveChangesAsync();
        }

        // ------------------------------
        // Apagar trabalho e vertentes
        // ------------------------------
        public async Task ApagarTrabalhoAsync(int trabalhoId)
        {
            var trabalho = await _context.Trabalhos
                .Include(t => t.TrabalhoVertentes)
                .FirstOrDefaultAsync(t => t.Id == trabalhoId);

            if (trabalho == null) return;

            _context.TrabalhoVertentes.RemoveRange(trabalho.TrabalhoVertentes);
            _context.Trabalhos.Remove(trabalho);
            await _context.SaveChangesAsync();
        }

        // ------------------------------
        // Buscar arquivo de vertente
        // ------------------------------
        public async Task<byte[]?> GetArquivoVertenteAsync(int vertenteId)
        {
            var vertente = await _context.TrabalhoVertentes
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Id == vertenteId);

            return vertente?.FicheiroBytes;
        }
    }
}
