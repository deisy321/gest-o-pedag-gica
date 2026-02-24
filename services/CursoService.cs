using gestaopedagogica.Data;
using gestaopedagogica.Models;
using Microsoft.EntityFrameworkCore;

namespace gestaopedagogica.Services
{
    public class CursoService
    {
        private readonly ApplicationDbContext _context;

        public CursoService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Curso>> GetCursosAsync()
        {
            return await _context.Cursos.AsNoTracking().ToListAsync();
        }

        public async Task AddCursoAsync(Curso curso)
        {
            _context.Cursos.Add(curso);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveCursoAsync(Curso curso)
        {
            _context.Cursos.Remove(curso);
            await _context.SaveChangesAsync();
        }
    }
}
