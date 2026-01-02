using gestaopedagogica.Data;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace gestaopedagogica.Controllers
{
    [Route("ficheiro")]
    public class FicheiroController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FicheiroController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("vertente/{id}")]
        public IActionResult DownloadVertente(int id)
        {
            var vert = _context.TrabalhoVertentes.FirstOrDefault(v => v.Id == id);

            if (vert == null || vert.FicheiroBytes == null)
                return NotFound();

            return File(
                vert.FicheiroBytes,
                vert.FicheiroContentType ?? "application/octet-stream",
                vert.FicheiroNome
            );
        }
    }
}
