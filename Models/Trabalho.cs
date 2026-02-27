using gestaopedagogica.Data;
using System;
using System.Collections.Generic;

namespace gestaopedagogica.Models
{
    public class Trabalho
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = "";

        public string? AlunoId { get; set; }
        public ApplicationUser? Aluno { get; set; }

        public string? ProfessorId { get; set; }
        public ApplicationUser? Professor { get; set; }

        public DateTime DataCriacao { get; set; }
        public DateTime PrazoEntrega { get; set; }

        public int? ModuloId { get; set; }
        public Modulo? Modulo { get; set; }

        public int? DisciplinaId { get; set; }
        public Disciplina? Disciplina { get; set; }

        public int? TurmaId { get; set; }
        public Turma? Turma { get; set; }

        // ✅ Campos de texto (NOT NULL)
        public string ConteudoTexto { get; set; } = "";

        // ✅ Campos de data/status (NOT NULL)
        public DateTime DataEnvio { get; set; } = DateTime.UtcNow;
        public bool VistoPeloProfessor { get; set; } = false;

        // ✅ Cada vertente agora guarda suas notas e arquivos
        public ICollection<TrabalhoVertente> TrabalhoVertentes { get; set; } = new List<TrabalhoVertente>();
    }
}