using System;
using System.ComponentModel.DataAnnotations;

namespace gestaopedagogica.Models
{
    public class TrabalhoVertente
    {
        public int Id { get; set; }

        public int TrabalhoId { get; set; }
        public Trabalho Trabalho { get; set; } = null!;

        [Required]
        public string Tipo { get; set; } = "";

        public string ConteudoTexto { get; set; } = "";
        public string ConteudoTextoAluno { get; set; } = "";

        // ✅ ARQUIVO NO BANCO
        public byte[]? FicheiroBytes { get; set; }
        public string? FicheiroNome { get; set; }
        public string? FicheiroContentType { get; set; }

        public DateTime? DataEnvio { get; set; }

        public decimal? Nota { get; set; }
        public string Feedback { get; set; } = "";
    }
}
