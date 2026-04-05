using System;

namespace gestaopedagogica.Models
{
  public class Comentario
    {
        public int Id { get; set; }

     // Relacionamento com Trabalho
        public int TrabalhoId { get; set; }
 public Trabalho Trabalho { get; set; } = null!;

      // Autor do comentário (pode ser Professor ou Aluno)
 public string AutorId { get; set; } = "";
   public virtual Data.ApplicationUser? Autor { get; set; }

        // Conteúdo
        public string Conteudo { get; set; } = "";

        // Timestamps
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
        public DateTime? DataAtualizacao { get; set; }

        // Type de comentário
        public enum TipoComentario
        {
          Pergunta = 0,
    Resposta = 1,
          Feedback = 2,
            Observacao = 3
     }

        public TipoComentario Tipo { get; set; } = TipoComentario.Observacao;

        // Se é resposta a outro comentário
        public int? ComentarioPaiId { get; set; }
        public virtual Comentario? ComentarioPai { get; set; }

  // Respostas a este comentário
        public virtual ICollection<Comentario> Respostas { get; set; } = new List<Comentario>();
    }
}
