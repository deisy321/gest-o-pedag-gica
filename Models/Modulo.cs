namespace gestaopedagogica.Models
{
    public class Modulo
    {
        public int Id { get; set; }
        public string Nome { get; set; } = "";

        public int ProfessorId { get; set; }
        public Professor Professor { get; set; } = null!;
    }
}
