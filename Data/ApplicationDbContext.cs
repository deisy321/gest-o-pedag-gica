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

        // ===== DbSets =====
        public DbSet<Aluno> Alunos { get; set; }
        public DbSet<Avaliacao> Avaliacoes { get; set; }
        public DbSet<Curso> Cursos { get; set; }
        public DbSet<Modulo> Modulos { get; set; }

        public DbSet<Turma> Turmas { get; set; }
        public DbSet<Professor> Professores { get; set; }
        public DbSet<TurmaProfessor> TurmaProfessores { get; set; }

        public DbSet<Trabalho> Trabalhos { get; set; }
        public DbSet<TrabalhoVertente> TrabalhoVertentes { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ===== Turma -> Alunos (1:N) =====
            builder.Entity<Turma>()
                .HasMany(t => t.Alunos)
                .WithOne(a => a.Turma)
                .HasForeignKey(a => a.TurmaId)
                .OnDelete(DeleteBehavior.Cascade);

            // ===== Trabalho -> Vertentes (1:N) =====
            builder.Entity<TrabalhoVertente>()
                .HasOne(tv => tv.Trabalho)
                .WithMany(t => t.TrabalhoVertentes)
                .HasForeignKey(tv => tv.TrabalhoId)
                .OnDelete(DeleteBehavior.Cascade);

            // ===== Professor <-> Turma (N:N) =====
            builder.Entity<TurmaProfessor>()
                .HasKey(tp => new { tp.TurmaId, tp.ProfessorId });

            builder.Entity<TurmaProfessor>()
                .HasOne(tp => tp.Turma)
                .WithMany(t => t.Professores)
                .HasForeignKey(tp => tp.TurmaId);

            builder.Entity<TurmaProfessor>()
                .HasOne(tp => tp.Professor)
                .WithMany(p => p.Turmas)
                .HasForeignKey(tp => tp.ProfessorId);
        }
    }
}
