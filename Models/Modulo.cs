namespace gestaopedagogica.Models
{
 public class Modulo
 {
 public int Id { get; set; }
 public string Nome { get; set; } = string.Empty;
 public string? ProfessorId { get; set; }
 public string? ProfessorName { get; set; }
 }
}