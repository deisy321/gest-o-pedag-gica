using gestaopedagogica.Data;
using gestaopedagogica.Models;
using Microsoft.AspNetCore.Identity;

namespace gestaopedagogica.Services
{
    public class AdminService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        // Retorna o total de alunos
        public async Task<int> GetTotalAlunosAsync()
        {
            var alunos = await _userManager.GetUsersInRoleAsync("Aluno");
            return alunos.Count;
        }

        // Retorna o total de professores
        public async Task<int> GetTotalProfessoresAsync()
        {
            var professores = await _userManager.GetUsersInRoleAsync("Professor");
            return professores.Count;
        }

        // Retorna todos os usuários
        public List<ApplicationUser> GetTodosUsuarios()
        {
            return _userManager.Users.ToList();
        }

        // Retorna usuários filtrando por role (ex.: "Aluno" ou "Professor")
        public async Task<List<ApplicationUser>> GetUsuariosPorRoleAsync(string role)
        {
            var users = await _userManager.GetUsersInRoleAsync(role);
            return users.ToList();
        }
    }
}
