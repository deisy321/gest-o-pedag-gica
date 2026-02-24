namespace gestaopedagogica.Models
{
    public class TurmaModulo
    {
        public int TurmaId { get; set; }
        public Turma? Turma { get; set; }

        public int ModuloId { get; set; }
        public Modulo? Modulo { get; set; }
    }
}