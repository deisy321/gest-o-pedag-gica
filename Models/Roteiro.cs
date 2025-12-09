namespace GestaoPedagogica.Models;

public class Roteiro
{
    public int Id { get; set; }
    public string Titulo { get; set; } = "";
    public List<Tarefa> Tarefas { get; set; } = new();
}

public class Tarefa
{
    public int Id { get; set; }
    public string Descricao { get; set; } = "";
    public DateTime Prazo { get; set; }
    public bool IsSubmitted { get; set; } = false;
    public string? Feedback { get; set; }
    public int Ordem { get; set; }
}
