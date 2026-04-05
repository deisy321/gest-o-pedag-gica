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

        // Definido como PushSubscriptions para manter consistência com o Service e a tabela física
        public DbSet<NotificationSubscription> PushSubscriptions { get; set; }
        public DbSet<Aluno> Alunos { get; set; }
        public DbSet<Avaliacao> Avaliacoes { get; set; }
        public DbSet<Curso> Cursos { get; set; }
        public DbSet<Modulo> Modulos { get; set; }
        public DbSet<Turma> Turmas { get; set; }
        public DbSet<Professor> Professores { get; set; }
        public DbSet<TurmaProfessor> TurmaProfessores { get; set; }
        public DbSet<TurmaModulo> TurmaModulos { get; set; }
        public DbSet<Trabalho> Trabalhos { get; set; }
        public DbSet<TrabalhoVertente> TrabalhoVertentes { get; set; }
        public DbSet<Disciplina> Disciplinas { get; set; }
        public DbSet<Comentario> Comentarios { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ===== Professor <-> Turma <-> Disciplina (N:N:1) =====
            builder.Entity<TurmaProfessor>(entity =>
            {
                entity.ToTable("TurmaProfessores");
                entity.HasKey(tp => new { tp.TurmaId, tp.ProfessorId, tp.DisciplinaId });

                entity.HasOne(tp => tp.Turma)
                    .WithMany(t => t.Professores)
                    .HasForeignKey(tp => tp.TurmaId);

                entity.HasOne(tp => tp.Professor)
                    .WithMany(p => p.Turmas)
                    .HasForeignKey(tp => tp.ProfessorId);

                entity.HasOne(tp => tp.Disciplina)
                    .WithMany()
                    .HasForeignKey(tp => tp.DisciplinaId);
            });

            // ===== Módulo <-> Turma (Muitos-para-Muitos Real) =====
            builder.Entity<TurmaModulo>(entity =>
            {
                entity.ToTable("TurmaModulos");
                entity.HasKey(tm => new { tm.TurmaId, tm.ModuloId });

                entity.HasOne(tm => tm.Turma)
                    .WithMany(t => t.Modulos)
                    .HasForeignKey(tm => tm.TurmaId);

                entity.HasOne(tm => tm.Modulo)
                    .WithMany(m => m.Turmas)
                    .HasForeignKey(tm => tm.ModuloId);
            });

            // ===== Ajuste para evitar o Erro de Coluna Inexistente =====
            builder.Entity<Modulo>(entity =>
            {
                entity.ToTable("Modulos");
                entity.Ignore("TurmaId");
            });

            // ===== Disciplina =====
            builder.Entity<Disciplina>(entity =>
            {
                entity.ToTable("Disciplinas");
                entity.HasOne(d => d.Curso).WithMany().HasForeignKey(d => d.CursoId).IsRequired(false);
            });

            // ===== Trabalho =====
            builder.Entity<Trabalho>(entity =>
            {
                entity.ToTable("Trabalhos");
                entity.HasKey(t => t.Id);

                entity.HasOne(t => t.Modulo).WithMany().HasForeignKey(t => t.ModuloId).IsRequired(false);
                entity.HasOne(t => t.Disciplina).WithMany().HasForeignKey(t => t.DisciplinaId).IsRequired(false);
                entity.HasOne(t => t.Turma).WithMany().HasForeignKey(t => t.TurmaId).IsRequired(false);
                entity.HasOne(t => t.Aluno).WithMany().HasForeignKey(t => t.AlunoId).IsRequired(false);
                entity.HasOne(t => t.Professor).WithMany().HasForeignKey(t => t.ProfessorId).IsRequired(false);
            });

            // ===== TrabalhoVertente =====
            builder.Entity<TrabalhoVertente>(entity =>
            {
                entity.ToTable("TrabalhoVertentes");
                entity.HasKey(tv => tv.Id);
                entity.HasOne(tv => tv.Trabalho)
                    .WithMany(t => t.TrabalhoVertentes)
                    .HasForeignKey(tv => tv.TrabalhoId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ===== Comentario =====
            builder.Entity<Comentario>(entity =>
            {
                entity.ToTable("Comentarios");
                entity.HasKey(c => c.Id);
                entity.HasOne(c => c.Trabalho).WithMany().HasForeignKey(c => c.TrabalhoId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(c => c.Autor).WithMany().HasForeignKey(c => c.AutorId).IsRequired(false);
                entity.HasOne(c => c.ComentarioPai).WithMany(c => c.Respostas).HasForeignKey(c => c.ComentarioPaiId).IsRequired(false).OnDelete(DeleteBehavior.Cascade);
            });

            // ===== Mapeamento de Tabelas Físicas (Case Sensitive Postgres) =====
            builder.Entity<Professor>().ToTable("Professores");
            builder.Entity<Turma>().ToTable("Turmas");
            builder.Entity<Aluno>().ToTable("Alunos");
            builder.Entity<Curso>().ToTable("Cursos");
            builder.Entity<Avaliacao>().ToTable("Avaliacoes");
            builder.Entity<Trabalho>().ToTable("Trabalhos");
            builder.Entity<TrabalhoVertente>().ToTable("TrabalhoVertentes");
            builder.Entity<Comentario>().ToTable("Comentarios");
            builder.Entity<NotificationSubscription>().ToTable("PushSubscriptions");
        }
    }
}