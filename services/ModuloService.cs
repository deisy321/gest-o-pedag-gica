using gestaopedagogica.Data;
using gestaopedagogica.Models;
using Microsoft.EntityFrameworkCore;

namespace gestaopedagogica.Services
{
    public class ModuloService
    {
        private readonly ApplicationDbContext _context;
        public ModuloService(ApplicationDbContext context) => _context = context;

        public async Task<List<Modulo>> GetModulosAsync()
        {
            return await _context.Modulos.AsNoTracking().ToListAsync();
        }

        public async Task<List<Modulo>> GetModulosComProfessorAsync()
        {
            return await _context.Modulos
                                 .Include(m => m.Professor)
                                 .AsNoTracking()
                                 .ToListAsync();
        }

        public async Task<Modulo?> GetModuloByIdAsync(int id)
        {
            return await _context.Modulos
                                 .Include(m => m.Professor)
                                 .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task AddModuloAsync(Modulo modulo)
        {
            try
            {
                // Como os critérios são obrigatórios no banco (Migration), 
                // inicializamos aqui com um valor padrão para não dar erro de gravação.
                // O Professor irá editar estes valores depois na Dashboard dele.
                modulo.CriterioAptidao ??= "Pendente";
                modulo.CriterioCompetencia ??= "Pendente";
                modulo.CriterioConhecimento ??= "Pendente";

                _context.Modulos.Add(modulo);

                // O SaveChangesAsync gera o ID necessário para a tabela de ligação (turmamodulos)
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro crítico no ModuloService: {ex.InnerException?.Message ?? ex.Message}");
                throw;
            }
        }

        public async Task UpdateModuloAsync(Modulo modulo)
        {
            // Este método será usado pelo Professor para preencher os critérios reais
            _context.Modulos.Update(modulo);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveModuloAsync(Modulo modulo)
        {
            _context.Modulos.Remove(modulo);
            await _context.SaveChangesAsync();
        }
    }
}
