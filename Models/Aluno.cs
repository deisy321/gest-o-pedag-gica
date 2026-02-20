using gestaopedagogica.Data;

namespace gestaopedagogica.Models
{
    public class Aluno
    {
        public int Id { get; set; }
        public string UserId { get; set; } = "";
        public ApplicationUser User { get; set; } = null!;
        public string Nome { get; set; } = "";
        public int TurmaId { get; set; }
        public Turma Turma { get; set; } = null!;
    }
}
