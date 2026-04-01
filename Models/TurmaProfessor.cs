namespace gestaopedagogica.Models
{
    public class TurmaProfessor
    {
        public int TurmaId { get; set; }
        public Turma Turma { get; set; } = null!;

        public int ProfessorId { get; set; }
        public Professor Professor { get; set; } = null!;

        // DisciplinaId é OBRIGATÓRIO
        public int DisciplinaId { get; set; }
        public Disciplina Disciplina { get; set; } = null!;

        // Modulo é opcional (string)
        public string Modulo { get; set; } = string.Empty;
    }
}
