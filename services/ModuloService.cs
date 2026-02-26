using System.Collections.Generic;
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
                modulo.CriterioAptidao ??= "Pendente";
                modulo.CriterioCompetencia ??= "Pendente";
                modulo.CriterioConhecimento ??= "Pendente";

                _context.Modulos.Add(modulo);
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
            // Evita conflito de tracking
            _context.Entry(modulo).State = EntityState.Modified;

            // Garante que o Professor não seja alterado
            if (modulo.Professor != null)
            {
                _context.Entry(modulo.Professor).State = EntityState.Unchanged;
            }

            await _context.SaveChangesAsync();
        }

        public async Task RemoveModuloAsync(Modulo modulo)
        {
            _context.Modulos.Remove(modulo);
            await _context.SaveChangesAsync();
        }
    }
}