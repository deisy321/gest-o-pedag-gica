using gestaopedagogica.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace gestaopedagogica.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Administrador")]
    public class DiagnosticoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DiagnosticoController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetDiagnostico()
        {
            try
            {
                var turmas = await _context.Turmas
                    .Include(t => t.Curso)
                    .Include(t => t.Alunos)
                    .Include(t => t.Professores).ThenInclude(tp => tp.Professor)
                    .Include(t => t.Professores).ThenInclude(tp => tp.Disciplina)
                    .ToListAsync();

                var turmasDto = turmas.Select(t => new
                {
                    t.Id,
                    t.Nome,
                    t.Ano,
                    Curso = t.Curso != null ? t.Curso.Nome : "Sem Curso",
                    CursoId = t.CursoId,
                    TotalAlunos = t.Alunos.Count,
                    TotalProfessores = t.Professores.Count,
                    Professores = t.Professores.Select(tp => new
                    {
                        ProfessorNome = tp.Professor?.Nome,
                        DisciplinaNome = tp.Disciplina?.Nome
                    }).ToList()
                }).ToList();

                var diagnostico = new
                {
                    Timestamp = DateTime.Now,
                    BancoDados = new
                    {
                        TotalTurmas = await _context.Turmas.CountAsync(),
                        TotalProfessores = await _context.Professores.CountAsync(),
                        TotalAlunos = await _context.Alunos.CountAsync(),
                        TotalCursos = await _context.Cursos.CountAsync(),
                        TotalModulos = await _context.Modulos.CountAsync(),
                        TotalDisciplinas = await _context.Disciplinas.CountAsync(),
                    },
                    Turmas = turmasDto,
                    Cursos = await _context.Cursos.Select(c => new { c.Id, c.Nome }).ToListAsync(),
                    Mensagem = "Diagnóstico realizado com sucesso"
                };

                return Ok(diagnostico);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Erro = ex.Message,
                    StackTrace = ex.StackTrace
                });
            }
        }

        [HttpGet("turmas-simples")]
        public async Task<IActionResult> GetTurmasSimples()
        {
            try
            {
                var turmasSimples = await _context.Turmas.ToListAsync();
                Console.WriteLine($"[DiagnosticoController] Turmas simples: {turmasSimples.Count}");

                return Ok(new
                {
                    TotalTurmas = turmasSimples.Count,
                    Turmas = turmasSimples.Select(t => new
                    {
                        t.Id,
                        t.Nome,
                        t.Ano,
                        t.CursoId
                    }).ToList(),
                    Timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Erro = ex.Message, StackTrace = ex.StackTrace });
            }
        }

        [HttpPost("limpar-turmas")]
        public async Task<IActionResult> LimparTurmas()
        {
            try
            {
                var count = await _context.Turmas.CountAsync();
                Console.WriteLine($"[DiagnosticoController] Limpando {count} turmas...");

                _context.Turmas.RemoveRange(await _context.Turmas.ToListAsync());
                await _context.SaveChangesAsync();

                return Ok(new { Mensagem = $"Limpas {count} turmas" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Erro = ex.Message });
            }
        }
    }
}
