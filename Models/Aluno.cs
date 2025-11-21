namespace gestaopedagogica.Models
{
    public class Aluno
    {
        public int Id { get; set; }
        public string Nome { get; set; } = null!;
        public string? NumeroAluno { get; set; }   // opcional
        public DateTime DataNascimento { get; set; }

        // Relação 1:N com Avaliacao
        public ICollection<Avaliacao> Avaliacoes { get; set; } = new List<Avaliacao>();
    }
}
