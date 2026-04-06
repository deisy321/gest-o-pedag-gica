using gestaopedagogica.Data;
using gestaopedagogica.Models;
using System.ComponentModel.DataAnnotations;

namespace gestaopedagogica.Models
{
    public class Trabalho
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = "";
        public bool IsPlanoRecuperacao { get; set; } = false;
        public string? Descricao { get; set; }

        public string? AlunoId { get; set; }
        public ApplicationUser? Aluno { get; set; }

        public string? ProfessorId { get; set; }
        public ApplicationUser? Professor { get; set; }

        public DateTime DataCriacao { get; set; }
        public DateTime PrazoEntrega { get; set; }
        public DateTime? DataEntrega { get; set; } // Adicionado

        public int? ModuloId { get; set; }
        public Modulo? Modulo { get; set; }

        public int? DisciplinaId { get; set; }
        public Disciplina? Disciplina { get; set; }

        public int? TurmaId { get; set; }
        public Turma? Turma { get; set; }

        public string ConteudoTexto { get; set; } = "";
        public string? ConteudoTextoAluno { get; set; }

        // ✅ Propriedades de Arquivo no Trabalho (Cenário sem Vertentes)
        public byte[]? FicheiroBytes { get; set; }
        public string? FicheiroNome { get; set; }
        public string? FicheiroContentType { get; set; }

        public decimal? Nota { get; set; } // Adicionado
        public DateTime DataEnvio { get; set; } = DateTime.UtcNow;
        public bool VistoPeloProfessor { get; set; } = false;

        public ICollection<TrabalhoVertente> TrabalhoVertentes { get; set; } = new List<TrabalhoVertente>();
    }
}