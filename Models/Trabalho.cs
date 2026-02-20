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

        public ICollection<TrabalhoVertente> TrabalhoVertentes { get; set; } = new List<TrabalhoVertente>();
    }
}
