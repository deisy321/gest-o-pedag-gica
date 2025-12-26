using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace gestaopedagogica.Models
{
    public class TrabalhoVertente
    {
        public int Id { get; set; }

        // ✅ FK explícita
        public int TrabalhoId { get; set; }

        // Navegação
        public Trabalho Trabalho { get; set; } = null!;

        [Required]
        public string Tipo { get; set; } = "";

        public string ConteudoTexto { get; set; } = "";
        public string ConteudoTextoAluno { get; set; } = "";


        public string FicheiroPath { get; set; } = "";

        public DateTime DataEnvio { get; set; } = DateTime.Now;

        public decimal? Nota { get; set; }

        public string Feedback { get; set; } = "";
    }
}
