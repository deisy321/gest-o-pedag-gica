using gestaopedagogica.Data;
using gestaopedagogica.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace gestaopedagogica.Services
{
    public class UserService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<List<UsuarioModel>> GetAllUsuariosAsync()
        {
            var users = await _context.Users.ToListAsync();
            var lista = new List<UsuarioModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                lista.Add(new UsuarioModel
                {
                    Id = user.Id,
                    Nome = user.UserName ?? "",
                    Email = user.Email ?? "",
                    Role = roles.FirstOrDefault() ?? ""
                });
            }
            return lista;
        }

        public async Task ExcluirUsuarioAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return;

            // -----------------------
            // REMOVER PROFESSOR E RELAÇÕES
            // -----------------------
            var professor = await _context.Professores.FirstOrDefaultAsync(p => p.UserId == userId);
            if (professor != null)
            {
                // Removendo relações usando o ID do objeto carregado (evita erro de string/int no Where)
                var turmas = await _context.TurmaProfessores
                    .Where(tp => tp.ProfessorId == professor.Id)
                    .ToListAsync();
                if (turmas.Any()) _context.TurmaProfessores.RemoveRange(turmas);

                var trabalhos = await _context.Trabalhos
                    .Where(t => t.ProfessorId == professor.UserId)
                    .ToListAsync();
                if (trabalhos.Any()) _context.Trabalhos.RemoveRange(trabalhos);

                var modulos = await _context.Modulos
                    .Where(m => m.ProfessorId == professor.Id)
                    .ToListAsync();
                if (modulos.Any()) _context.Modulos.RemoveRange(modulos);

                _context.Professores.Remove(professor);
            }

            // -----------------------
            // REMOVER ALUNO E RELAÇÕES
            // -----------------------
            var aluno = await _context.Alunos.FirstOrDefaultAsync(a => a.UserId == userId);
            if (aluno != null)
            {
                var trabalhosAluno = await _context.Trabalhos
                    .Where(t => t.AlunoId == aluno.UserId)
                    .ToListAsync();
                if (trabalhosAluno.Any()) _context.Trabalhos.RemoveRange(trabalhosAluno);

                _context.Alunos.Remove(aluno);
            }

            // Salva todas as remoções de tabelas relacionadas primeiro
            await _context.SaveChangesAsync();

            // Por fim, remove o usuário do Identity
            await _userManager.DeleteAsync(user);
        }
    }

    public class UsuarioModel
    {
        public string Id { get; set; } = "";
        public string Nome { get; set; } = "";
        public string Email { get; set; } = "";
        public string Role { get; set; } = "";
    }
}
