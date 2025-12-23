using System.Collections.Generic;

namespace gestaopedagogica.Models
{
    public class Turma
    {
        public int Id { get; set; }
        public string Nome { get; set; } = "";
        public string Ano { get; set; } = "";
        public string ProfessorResponsavel { get; set; } = "";

        // Lista de alunos da turma
        public List<Aluno> Alunos { get; set; } = new();
    }
}
