using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace gestaopedagogica.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            // String EXATA para o Render
            var connectionString = "Host=dpg-d76ik01aae7s73c5uni0-a.ohio-postgres.render.com;Database=gestaopedagogica;Username=postgre;Password=aHDc5XBa1GpQo2NqbnmLsTE7qgtqBbvl;Port=5432;SSL Mode=Require;Trust Server Certificate=true";

            optionsBuilder.UseNpgsql(connectionString);

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}