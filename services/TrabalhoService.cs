using gestaopedagogica.Data;
using gestaopedagogica.Models;
using Microsoft.EntityFrameworkCore;

namespace gestaopedagogica.Services
{
    public class TrabalhoService
    {
        private readonly ApplicationDbContext _context;
        public TrabalhoService(ApplicationDbContext context) => _context = context;

        // Adicionar trabalho e vertentes padrão
        public async Task AddTrabalhoAsync(Trabalho trabalho)
        {
            trabalho.DataCriacao = DateTime.UtcNow;
            trabalho.DataEnvio = DateTime.UtcNow;

            if (trabalho.PrazoEntrega.Kind != DateTimeKind.Utc)
                trabalho.PrazoEntrega = DateTime.SpecifyKind(trabalho.PrazoEntrega, DateTimeKind.Utc);

            _context.Trabalhos.Add(trabalho);
            await _context.SaveChangesAsync();

            // Criar vertentes padrão
            var vertentes = new List<TrabalhoVertente>
            {
                new TrabalhoVertente { TrabalhoId = trabalho.Id, Tipo = "Competência", FicheiroPath = "", DataEnvio = DateTime.UtcNow, Feedback = "", ConteudoTexto = "", ConteudoTextoAluno = "" },
                new TrabalhoVertente { TrabalhoId = trabalho.Id, Tipo = "Aptidão", FicheiroPath = "", DataEnvio = DateTime.UtcNow, Feedback = "", ConteudoTexto = "", ConteudoTextoAluno = "" },
                new TrabalhoVertente { TrabalhoId = trabalho.Id, Tipo = "Conhecimento", FicheiroPath = "", DataEnvio = DateTime.UtcNow, Feedback = "", ConteudoTexto = "", ConteudoTextoAluno = "" }
            };

            _context.TrabalhoVertentes.AddRange(vertentes);
            await _context.SaveChangesAsync();
        }

        // Adicionar uma vertente individual
        public async Task AddVertenteAsync(TrabalhoVertente vertente)
        {
            vertente.FicheiroPath ??= "";
            vertente.Feedback ??= "";
            vertente.DataEnvio = DateTime.UtcNow;
            vertente.ConteudoTexto ??= "";
            vertente.ConteudoTextoAluno ??= "";

            _context.TrabalhoVertentes.Add(vertente);
            await _context.SaveChangesAsync();
        }

        // Listar todos os trabalhos disponíveis para um aluno
        public async Task<List<Trabalho>> GetTrabalhosDisponiveisParaAlunoAsync(string alunoId)
        {
            return await _context.Trabalhos
                .Include(t => t.TrabalhoVertentes)
                .OrderByDescending(t => t.PrazoEntrega)
                .ToListAsync();
        }

        // Listar trabalhos de um professor
        public async Task<List<Trabalho>> GetTrabalhosDoProfessorAsync(string professorId)
        {
            return await _context.Trabalhos
                .Include(t => t.TrabalhoVertentes)
                .Where(t => t.ProfessorId == professorId)
                .OrderByDescending(t => t.DataEnvio)
                .ToListAsync();
        }

        // Buscar trabalho por ID
        public async Task<Trabalho?> GetTrabalhoPorIdAsync(int id)
        {
            return await _context.Trabalhos
                .Include(t => t.TrabalhoVertentes)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        // Buscar vertentes de um trabalho
        public async Task<List<TrabalhoVertente>> GetVertentesDoTrabalhoAsync(int trabalhoId)
        {
            return await _context.TrabalhoVertentes
                .Where(v => v.TrabalhoId == trabalhoId)
                .ToListAsync();
        }

        // Buscar vertente por ID
        public async Task<TrabalhoVertente?> GetVertentePorIdAsync(int id)
        {
            return await _context.TrabalhoVertentes.FindAsync(id);
        }

        // Atualizar vertente
        public async Task AtualizarVertenteAsync(TrabalhoVertente vertente)
        {
            _context.TrabalhoVertentes.Update(vertente);
            await _context.SaveChangesAsync();
        }

        // Atualizar trabalho
        public async Task AtualizarTrabalhoAsync(Trabalho trabalho)
        {
            _context.Trabalhos.Update(trabalho);
            await _context.SaveChangesAsync();
        }
    }
}
