using gestaopedagogica.Data;
using gestaopedagogica.Models;
using System.ComponentModel.DataAnnotations;

namespace gestaopedagogica.Models
{
    public class Trabalho
    {
        public int Id { get; set; }

        [Required]
        public string Titulo { get; set; } = "";

        public bool IsPlanoRecuperacao { get; set; } = false;

        /// <summary>
        /// Define se o trabalho está visível na lista ativa. 
        /// Se false, o trabalho foi "arquivado" para não poluir a lista, 
        /// mas as notas associadas continuam a contar para a média.
        /// </summary>
        public bool Ativo { get; set; } = true;

        public string? Descricao { get; set; }

        public string? AlunoId { get; set; }
        public ApplicationUser? Aluno { get; set; }

        public string? ProfessorId { get; set; }
        public ApplicationUser? Professor { get; set; }

        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
        public DateTime PrazoEntrega { get; set; }
        public DateTime? DataEntrega { get; set; }

        public int HorasAtribuidas { get; set; }

        public int? ModuloId { get; set; }
        public Modulo? Modulo { get; set; }

        public int? DisciplinaId { get; set; }
        public Disciplina? Disciplina { get; set; }

        public int? TurmaId { get; set; }
        public Turma? Turma { get; set; }

        public string ConteudoTexto { get; set; } = "";
        public string? ConteudoTextoAluno { get; set; }

        // ✅ Propriedades de Arquivo
        public byte[]? FicheiroBytes { get; set; }
        public string? FicheiroNome { get; set; }
        public string? FicheiroContentType { get; set; }

        public decimal? Nota { get; set; }
        public DateTime DataEnvio { get; set; } = DateTime.UtcNow;
        public bool VistoPeloProfessor { get; set; } = false;

        public ICollection<TrabalhoVertente> TrabalhoVertentes { get; set; } = new List<TrabalhoVertente>();
    }
}