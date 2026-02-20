using gestaopedagogica.Models;

public class SubmissaoTrabalho
{
    public int Id { get; set; }

    public int TrabalhoId { get; set; }
    public Trabalho Trabalho { get; set; } = null!;

    public int AlunoId { get; set; }
    public Aluno Aluno { get; set; } = null!;

    public DateTime DataEnvio { get; set; }

    public List<TrabalhoVertente> Vertentes { get; set; } = new();
}
