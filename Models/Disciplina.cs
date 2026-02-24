using gestaopedagogica.Models;

public class Disciplina
{
    public int Id { get; set; }
    public string Nome { get; set; } = "";

    public List<Modulo> Modulos { get; set; } = new();

    // O "?" torna o CursoId opcional. O "= null" evita erros de validação.
    public int? CursoId { get; set; }
    public Curso? Curso { get; set; }
}
