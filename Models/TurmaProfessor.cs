namespace gestaopedagogica.Models
{
    public class TurmaProfessor
    {
        public int TurmaId { get; set; }
        public Turma Turma { get; set; } = null!;

        public int ProfessorId { get; set; }
        public Professor Professor { get; set; } = null!;
        public int? DisciplinaId { get; set; }
        public Disciplina Disciplina { get; set; } = null!;

        // ADICIONE ESTA PROPRIEDADE:
        // O "= string.Empty" resolve o erro de "propriedade não anulável"
        public string Modulo { get; set; } = string.Empty;
    }
}
