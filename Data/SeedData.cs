using Microsoft.AspNetCore.Identity;

namespace gestaopedagogica.Data
{
    public static class SeedData
    {
        public static async Task EnsureSeedRolesAndUsersAsync(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            // roles
            string[] roles = new[] { "Admin", "Professor", "Aluno" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // admin user (exemplo)
            var adminEmail = "admin@teste.com";
            var adminPassword = "Admin123!"; // muda depois

            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(admin, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
            }

            // podes criar aqui users de Professor e Aluno se quiseres
        }
    }
}
