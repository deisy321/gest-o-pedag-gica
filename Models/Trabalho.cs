using gestaopedagogica.Data;
using gestaopedagogica.Models;

public class Trabalho
{
    public int Id { get; set; }
    public string Titulo { get; set; } = "";

    // Nova propriedade
    public string? Descricao { get; set; }

    public string? AlunoId { get; set; }
    public ApplicationUser? Aluno { get; set; }

    public string? ProfessorId { get; set; }
    public ApplicationUser? Professor { get; set; }

    public DateTime DataCriacao { get; set; }
    public DateTime PrazoEntrega { get; set; }

    public int? ModuloId { get; set; }
    public Modulo? Modulo { get; set; }

    public int? DisciplinaId { get; set; }
    public Disciplina? Disciplina { get; set; }

    public int? TurmaId { get; set; }
    public Turma? Turma { get; set; }

    public string ConteudoTexto { get; set; } = "";

    public DateTime DataEnvio { get; set; } = DateTime.UtcNow;
    public bool VistoPeloProfessor { get; set; } = false;

    public ICollection<TrabalhoVertente> TrabalhoVertentes { get; set; } = new List<TrabalhoVertente>();
}