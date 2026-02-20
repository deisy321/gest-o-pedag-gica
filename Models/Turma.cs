namespace gestaopedagogica.Models
{
    public class Turma
    {
        public int Id { get; set; }
        public string Nome { get; set; } = "";
        public string Ano { get; set; } = "";

        public int CursoId { get; set; }
        public Curso Curso { get; set; } = null!;

        public List<Aluno> Alunos { get; set; } = new();
        public List<TurmaProfessor> Professores { get; set; } = new();
    }
}
