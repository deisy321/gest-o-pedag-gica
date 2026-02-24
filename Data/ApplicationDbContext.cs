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

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ===== Professor <-> Turma <-> Disciplina (N:N) =====
            builder.Entity<TurmaProfessor>(entity =>
            {
                entity.ToTable("TurmaProfessores");

                // Chave primária composta
                entity.HasKey(tp => new { tp.TurmaId, tp.ProfessorId, tp.DisciplinaId });

                // Mapeamento de colunas (Maiúsculas conforme a BD)
                entity.Property(tp => tp.TurmaId).HasColumnName("TurmaId");
                entity.Property(tp => tp.ProfessorId).HasColumnName("ProfessorId");
                entity.Property(tp => tp.DisciplinaId).HasColumnName("DisciplinaId");
                entity.Property(tp => tp.Modulo).HasColumnName("Modulo");

                // Configuração das Relações
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

            // ===== Módulo <-> Turma (N:N) =====
            builder.Entity<TurmaModulo>(entity =>
            {
                entity.ToTable("TurmaModulos");
                entity.HasKey(tm => new { tm.TurmaId, tm.ModuloId });

                // Ajustado para Maiúsculas para manter consistência
                entity.Property(tm => tm.TurmaId).HasColumnName("TurmaId");
                entity.Property(tm => tm.ModuloId).HasColumnName("ModuloId");

                entity.HasOne(tm => tm.Turma).WithMany(t => t.Modulos).HasForeignKey(tm => tm.TurmaId);
                entity.HasOne(tm => tm.Modulo).WithMany(m => m.Turmas).HasForeignKey(tm => tm.ModuloId);
            });

            // ===== Disciplina =====
            builder.Entity<Disciplina>(entity =>
            {
                entity.ToTable("Disciplinas");

                entity.Property(d => d.Id).HasColumnName("Id");
                entity.Property(d => d.Nome).HasColumnName("Nome");
                entity.Property(d => d.CursoId).HasColumnName("CursoId");

                // Relação obrigatória com Curso
                entity.HasOne(d => d.Curso)
                      .WithMany()
                      .HasForeignKey(d => d.CursoId);
            });

            // Tabelas principais - Mapeamento para tabelas físicas
            builder.Entity<Professor>().ToTable("Professores");
            builder.Entity<Turma>().ToTable("Turmas");
            builder.Entity<Modulo>().ToTable("Modulos");
            builder.Entity<Aluno>().ToTable("Alunos");
            builder.Entity<Curso>().ToTable("Cursos");
            builder.Entity<Avaliacao>().ToTable("Avaliacoes");
            builder.Entity<Trabalho>().ToTable("Trabalhos");
            builder.Entity<TrabalhoVertente>().ToTable("TrabalhoVertentes");
        }
    }
}
