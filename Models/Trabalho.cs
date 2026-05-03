using gestaopedagogica.Data;
using gestaopedagogica.Models;
using System.ComponentModel.DataAnnotations;

namespace gestaopedagogica.Models
{
    public class Trabalho
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O título é obrigatório")]
        public string Titulo { get; set; } = "";

        // ✅ Propriedade que faltava para corrigir o erro CS0117
        public string Status { get; set; } = "Pendente";

        public bool IsPlanoRecuperacao { get; set; } = false;

        public bool Ativo { get; set; } = true;

        public string? Descricao { get; set; }

        public string? AlunoId { get; set; }
        public virtual ApplicationUser? Aluno { get; set; }

        public string? ProfessorId { get; set; }
        public virtual ApplicationUser? Professor { get; set; }

        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
        public DateTime PrazoEntrega { get; set; }
        // NOVO: Este campo guarda o prazo real para efeitos de Badge
        public DateTime PrazoOriginal { get; set; }
        public DateTime? DataEntrega { get; set; }

        public int HorasAtribuidas { get; set; }

        public int? ModuloId { get; set; }
        public virtual Modulo? Modulo { get; set; }

        public int? DisciplinaId { get; set; }
        public virtual Disciplina? Disciplina { get; set; }

        public int? TurmaId { get; set; }
        public virtual Turma? Turma { get; set; }

        public string ConteudoTexto { get; set; } = "";
        public string? ConteudoTextoAluno { get; set; }

        public byte[]? FicheiroBytes { get; set; }
        public string? FicheiroNome { get; set; }
        public string? FicheiroContentType { get; set; }

        public decimal? Nota { get; set; }
        public DateTime DataEnvio { get; set; } = DateTime.UtcNow;
        public bool VistoPeloProfessor { get; set; } = false;

        public virtual ICollection<TrabalhoVertente> TrabalhoVertentes { get; set; } = new List<TrabalhoVertente>();
    }
}