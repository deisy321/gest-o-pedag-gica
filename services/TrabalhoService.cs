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
        // Criar trabalho (SEM criar vertentes automaticamente)
        // ------------------------------
        public async Task AddTrabalhoAsync(Trabalho trabalho)
        {
            try
            {
                trabalho.DataCriacao = DateTime.UtcNow;

                if (trabalho.PrazoEntrega.Kind != DateTimeKind.Utc)
                    trabalho.PrazoEntrega = DateTime.SpecifyKind(trabalho.PrazoEntrega, DateTimeKind.Utc);

                if (trabalho.ModuloId <= 0)
                    throw new Exception("ModuloId é obrigatório e deve ser > 0");

                if (trabalho.DisciplinaId <= 0)
                    throw new Exception("DisciplinaId é obrigatório e deve ser > 0");

                if (string.IsNullOrEmpty(trabalho.ProfessorId))
                    throw new Exception("ProfessorId é obrigatório");

                if (string.IsNullOrEmpty(trabalho.AlunoId))
                    throw new Exception("AlunoId é obrigatório");

                // ✅ Garantir que ConteudoTexto nunca seja nulo
                trabalho.ConteudoTexto ??= "";

                _context.Trabalhos.Add(trabalho);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException dbEx)
            {
                Console.WriteLine("========== ERRO DE BANCO DE DADOS ==========");
                Console.WriteLine($"Mensagem: {dbEx.Message}");
                Console.WriteLine($"Inner: {dbEx.InnerException?.Message}");
                Console.WriteLine($"Stack: {dbEx.StackTrace}");
                throw new Exception($"Erro ao salvar trabalho no banco: {dbEx.InnerException?.Message ?? dbEx.Message}", dbEx);
            }
            catch (Exception ex)
            {
                Console.WriteLine("========== ERRO COMPLETO ==========");
                Console.WriteLine(ex.ToString());

                if (ex.InnerException != null)
                {
                    Console.WriteLine("========== INNER ==========");
                    Console.WriteLine(ex.InnerException.ToString());
                }

                throw;
            }
        }

        // ------------------------------
        // Listagens
        // ------------------------------
        public async Task<List<Trabalho>> GetTrabalhosDoProfessorAsync(string professorId)
        {
            return await _context.Trabalhos
                .Include(t => t.TrabalhoVertentes)
                .Include(t => t.Disciplina)
                .Include(t => t.Turma)
                .Where(t => t.ProfessorId == professorId)
                .OrderByDescending(t => t.DataCriacao)
                .ToListAsync();
        }

        public async Task<List<Trabalho>> GetTrabalhosDisponiveisParaAlunoAsync(string alunoId)
        {
            return await _context.Trabalhos
                .Include(t => t.TrabalhoVertentes)
                .Include(t => t.Disciplina)
                .Where(t => t.AlunoId == alunoId)
                .OrderByDescending(t => t.DataCriacao)
                .ToListAsync();
        }

        public async Task<Trabalho?> GetTrabalhoPorIdAsync(int id)
        {
            return await _context.Trabalhos
                .Include(t => t.TrabalhoVertentes)
                .Include(t => t.Disciplina)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        // ------------------------------
        // Obter vertente por Id
        // ------------------------------
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
        // Atualizar vertente existente
        // ------------------------------
        public async Task AtualizarVertenteAsync(TrabalhoVertente vertente)
        {
            var existente = await _context.TrabalhoVertentes
                .FirstOrDefaultAsync(v => v.Id == vertente.Id);

            if (existente == null)
                throw new Exception("Vertente não encontrada.");

            existente.Tipo = vertente.Tipo;
            existente.ConteudoTexto = vertente.ConteudoTexto;
            existente.ConteudoTextoAluno = vertente.ConteudoTextoAluno;
            existente.Feedback = vertente.Feedback;

            if ((vertente.FicheiroBytes != null && vertente.FicheiroBytes.Length > 0)
                || !string.IsNullOrEmpty(vertente.ConteudoTextoAluno))
            {
                existente.DataEnvio ??= DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        // ------------------------------
        // Criar nova vertente
        // ------------------------------
        public async Task AddVertenteAsync(TrabalhoVertente vertente)
        {
            try
            {
                if (vertente.TrabalhoId <= 0)
                    throw new Exception("TrabalhoId é obrigatório e deve ser > 0");

                if (string.IsNullOrEmpty(vertente.Tipo))
                    throw new Exception("Tipo de vertente é obrigatório");

                vertente.Feedback ??= "";
                vertente.ConteudoTexto ??= "";
                vertente.ConteudoTextoAluno ??= "";

                _context.TrabalhoVertentes.Add(vertente);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException dbEx)
            {
                Console.WriteLine("========== ERRO AO ADICIONAR VERTENTE ==========");
                Console.WriteLine($"Mensagem: {dbEx.Message}");
                Console.WriteLine($"Inner: {dbEx.InnerException?.Message}");
                throw new Exception($"Erro ao salvar vertente: {dbEx.InnerException?.Message ?? dbEx.Message}", dbEx);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERRO ao adicionar vertente: {ex.Message}");
                throw;
            }
        }

        // ------------------------------
        // Envio de vertente (aluno)
        // ------------------------------
        public async Task EnviarVertenteAsync(int vertenteId, byte[]? arquivo = null, string? conteudoTexto = null)
        {
            var vertente = await _context.TrabalhoVertentes
                .FirstOrDefaultAsync(v => v.Id == vertenteId);

            if (vertente == null)
                throw new Exception("Vertente não encontrada.");

            if (arquivo != null && arquivo.Length > 0)
            {
                vertente.FicheiroBytes = arquivo;
                vertente.FicheiroContentType = "application/octet-stream";
                vertente.FicheiroNome = "arquivo_enviado";
            }

            if (!string.IsNullOrEmpty(conteudoTexto))
                vertente.ConteudoTextoAluno = conteudoTexto;

            vertente.DataEnvio ??= DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        // ------------------------------
        // Apagar trabalho
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
        // Buscar arquivo
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