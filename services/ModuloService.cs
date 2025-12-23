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
            return await _context.Modulos.ToListAsync();
        }

        public async Task AddModuloAsync(Modulo modulo)
        {
            _context.Modulos.Add(modulo);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateModuloAsync(Modulo modulo)
        {
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
