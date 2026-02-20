using gestaopedagogica.Data;

namespace gestaopedagogica.Models
{
    public class Professor
    {
        public int Id { get; set; }
        public string Nome { get; set; } = "";

        // Ligação com Identity
        public string UserId { get; set; } = "";
        public ApplicationUser User { get; set; } = null!;

        // Relacionamentos
        public List<TurmaProfessor> Turmas { get; set; } = new();
        public List<Modulo> Modulos { get; set; } = new();
    }
}
