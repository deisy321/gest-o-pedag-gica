using System;
using System.ComponentModel.DataAnnotations;

namespace gestaopedagogica.Models
{
    public class TrabalhoVertente
    {
        public int Id { get; set; }

        [Required]
        public int TrabalhoId { get; set; }
        public Trabalho Trabalho { get; set; }

        [Required]
        public string Tipo { get; set; } // "Competencia", "Aptidao", "Conhecimento"

        public string ConteudoTexto { get; set; }

        public string FicheiroPath { get; set; }

        public DateTime DataEnvio { get; set; } = DateTime.Now;

        // AVALIAÇÃO
        public decimal? Nota { get; set; }
        public string Feedback { get; set; }
    }
}
