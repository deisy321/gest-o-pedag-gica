using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace gestaopedagogica.Models
{
    public class Trabalho
    {
        public int Id { get; set; }

        [Required]
        public string AlunoId { get; set; } = "";

        [Required]
        public string ProfessorId { get; set; } = "";

        [Required]
        public string Titulo { get; set; } = "";

        public string ConteudoTexto { get; set; } = "";

        public string FicheiroCompetencia { get; set; } = "";
        public string FicheiroAptidao { get; set; } = "";
        public string FicheiroConhecimento { get; set; } = "";

      
        public DateTime DataEnvio { get; set; }
        public DateTime PrazoEntrega { get; set; }
        public DateTime DataCriacao { get; set; }

        public bool VistoPeloProfessor { get; set; } = false;

        public string NotaCompetencia { get; set; } = "";
        public string NotaAptidao { get; set; } = "";
        public string NotaConhecimento { get; set; } = "";

        public List<TrabalhoVertente> TrabalhoVertentes { get; set; } = new();
    }
}
