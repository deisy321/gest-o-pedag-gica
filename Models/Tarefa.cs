namespace gestaopedagogica.Models
{
    public class Tarefa
    {
        public int Id { get; set; }
        public string Descricao { get; set; } = ""; // inicializado
        public DateTime Prazo { get; set; }
        public bool IsSubmitted { get; set; } = false; // envia automaticamente desbloqueia próximo
        public string? Feedback { get; set; } = null;  // feedback pode ser nulo
        public int Ordem { get; set; }
    }
}
