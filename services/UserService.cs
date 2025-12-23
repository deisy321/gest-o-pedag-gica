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
                    Role = roles.FirstOrDefault() ?? "" // pega a primeira role, se existir
                });
            }

            return lista;
        }

        public async Task ExcluirUsuarioAsync(string userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
            }
        }

        public async Task AddUserAsync(ApplicationUser user)
        {
            await _userManager.CreateAsync(user);
        }

        public async Task UpdateUserAsync(ApplicationUser user)
        {
            await _userManager.UpdateAsync(user);
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
