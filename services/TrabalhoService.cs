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

        // Listar trabalhos de um aluno
        public async Task<List<Trabalho>> GetTrabalhosDoAlunoAsync(string alunoId)
        {
            return await _context.Trabalhos
                .Where(t => t.AlunoId == alunoId)
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

        // Adicionar novo trabalho (UTC garantido)
        public async Task AddTrabalhoAsync(Trabalho trabalho)
        {
            // Garantindo que todas as datas estejam em UTC
            trabalho.DataEnvio = DateTime.UtcNow;
            trabalho.DataCriacao = DateTime.UtcNow;

            if (trabalho.PrazoEntrega.Kind != DateTimeKind.Utc)
                trabalho.PrazoEntrega = DateTime.SpecifyKind(trabalho.PrazoEntrega, DateTimeKind.Utc);

            _context.Trabalhos.Add(trabalho);
            await _context.SaveChangesAsync();
        }

        // Atualizar trabalho
        public async Task AtualizarTrabalhoAsync(Trabalho trabalho)
        {
            // Garantir UTC no PrazoEntrega caso precise atualizar
            if (trabalho.PrazoEntrega.Kind != DateTimeKind.Utc)
                trabalho.PrazoEntrega = DateTime.SpecifyKind(trabalho.PrazoEntrega, DateTimeKind.Utc);

            _context.Trabalhos.Update(trabalho);
            await _context.SaveChangesAsync();
        }

        // Listar trabalhos de um professor
        public async Task<List<Trabalho>> GetTrabalhosDoProfessorAsync(string professorId)
        {
            return await _context.Trabalhos
                .Where(t => t.ProfessorId == professorId)
                .Include(t => t.TrabalhoVertentes)
                .OrderByDescending(t => t.DataEnvio)
                .ToListAsync();
        }

        // Adicionar vertente
        public async Task AddVertenteAsync(TrabalhoVertente vertente)
        {
            _context.TrabalhoVertentes.Add(vertente);
            await _context.SaveChangesAsync();
        }

        // Buscar vertente por ID
        public async Task<TrabalhoVertente?> GetVertentePorIdAsync(int id)
        {
            return await _context.TrabalhoVertentes.FindAsync(id);
        }

        // Listar vertentes de um trabalho
        public async Task<List<TrabalhoVertente>> GetVertentesDoTrabalhoAsync(int trabalhoId)
        {
            return await _context.TrabalhoVertentes
                .Where(v => v.TrabalhoId == trabalhoId)
                .ToListAsync();
        }

        // Atualizar vertente
        public async Task AtualizarVertenteAsync(TrabalhoVertente vertente)
        {
            _context.TrabalhoVertentes.Update(vertente);
            await _context.SaveChangesAsync();
        }
    }
}
