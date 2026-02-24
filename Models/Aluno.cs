using gestaopedagogica.Data;

namespace gestaopedagogica.Models
{
    public class Aluno
    {
        public int Id { get; set; }

        public string? Nome { get; set; }

        // CORREÇÃO 1: Adiciona o campo Email que estava faltando
        public string? Email { get; set; }

        public string? UserId { get; set; }

        // CORREÇÃO 2: O '?' permite que o valor seja nulo (vazio) inicialmente
        public int? TurmaId { get; set; }

        public Turma? Turma { get; set; }
    }
}
