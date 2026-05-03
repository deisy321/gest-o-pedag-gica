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
            try
            {
                // 1. Busca a versão atual do banco sem tracking para comparar
                var dbModulo = await _context.Modulos.FirstOrDefaultAsync(m => m.Id == modulo.Id);

                if (dbModulo == null) throw new Exception("Módulo não encontrado no banco de dados.");

                // 2. Atualiza apenas os campos de dados
                dbModulo.Nome = modulo.Nome;
                dbModulo.Numero = modulo.Numero;
                dbModulo.ProfessorId = modulo.ProfessorId;

                // Se o seu modelo tiver carga horária ou critérios na edição, atualize aqui também:
                // dbModulo.TotalHoras = modulo.TotalHoras;

                // 3. Informa ao EF que esta entidade foi alterada
                _context.Modulos.Update(dbModulo);

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao atualizar módulo: {ex.Message}");
            }
        }

        public async Task RemoveModuloAsync(Modulo modulo)
        {
            try
            {
                // 1. Carrega o módulo com todos os relacionamentos
                var moduloCompleto = await _context.Modulos
                                                  .Include(m => m.Turmas)
                                                  .FirstOrDefaultAsync(m => m.Id == modulo.Id);

                if (moduloCompleto == null)
                    throw new Exception("Módulo não encontrado.");

                // 2. Remove trabalhos associados a este módulo
                var trabalhos = await _context.Trabalhos
                                              .Where(t => t.ModuloId == modulo.Id)
                                              .ToListAsync();

                if (trabalhos.Any())
                    _context.Trabalhos.RemoveRange(trabalhos);

                // 3. Remove associações TurmaModulo
                if (moduloCompleto.Turmas != null && moduloCompleto.Turmas.Any())
                    _context.TurmaModulos.RemoveRange(moduloCompleto.Turmas);

                // 4. Remove o módulo
                _context.Modulos.Remove(moduloCompleto);

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException dbEx)
            {
                throw new Exception("Erro ao remover módulo: Verifique se existem trabalhos ou turmas vinculadas. Detalhes: " + dbEx.InnerException?.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao remover módulo: " + ex.Message);
            }
        }
    }
}
