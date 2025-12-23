using gestaopedagogica.Data;
using gestaopedagogica.Models;
using Microsoft.EntityFrameworkCore;

namespace gestaopedagogica.Services
{
    public class TrabalhoService
    {
        private readonly ApplicationDbContext _context;

        // Construtor primário
        public TrabalhoService(ApplicationDbContext context) => _context = context;

        // Listar todos os trabalhos de um aluno específico
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
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        // Adicionar novo trabalho
        public async Task AddTrabalhoAsync(Trabalho trabalho)
        {
            trabalho.DataEnvio = DateTime.Now;
            _context.Trabalhos.Add(trabalho);
            await _context.SaveChangesAsync();
        }

        // Atualizar trabalho (status, notas, etc.)
        public async Task AtualizarTrabalhoAsync(Trabalho trabalho)
        {
            _context.Trabalhos.Update(trabalho);
            await _context.SaveChangesAsync();
        }

        // Listar trabalhos por professor
        public async Task<List<Trabalho>> GetTrabalhosDoProfessorAsync(string professorId)
        {
            return await _context.Trabalhos
                .Where(t => t.ProfessorId == professorId)
                .Include(t => t.TrabalhoVertentes) // importante para vertentes
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
        // Buscar todas as vertentes de um trabalho
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
