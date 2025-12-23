using gestaopedagogica.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace gestaopedagogica.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Tabelas existentes
        public DbSet<Aluno> Alunos { get; set; }
        public DbSet<Avaliacao> Avaliacoes { get; set; }
        public DbSet<Modulo> Modulos { get; set; }
        public DbSet<Turma> Turmas { get; set; }
        public DbSet<Trabalho> Trabalhos { get; set; }
        public DbSet<Curso> Cursos { get; set; }

        // DbSet para TrabalhoVertente
        public DbSet<TrabalhoVertente> TrabalhoVertentes { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Relação 1:N Trabalho -> TrabalhoVertente
            builder.Entity<TrabalhoVertente>()
                .HasOne(tv => tv.Trabalho)
                .WithMany(t => t.TrabalhoVertentes)
                .HasForeignKey(tv => tv.TrabalhoId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relação 1:N Turma -> Alunos
            builder.Entity<Turma>()
                .HasMany(t => t.Alunos)
                .WithOne(a => a.Turma)
                .HasForeignKey(a => a.TurmaId)
                .OnDelete(DeleteBehavior.Cascade);
        }

    }
}
