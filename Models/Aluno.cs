namespace gestaopedagogica.Models
{
    public class Aluno
    {
        public int Id { get; set; }
        public string Nome { get; set; } = "";

        // Relação com a turma
        public int TurmaId { get; set; }
        public Turma Turma { get; set; } = null!;
    }
}
