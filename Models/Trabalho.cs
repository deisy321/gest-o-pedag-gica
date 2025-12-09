using System;
using System.ComponentModel.DataAnnotations;

namespace gestaopedagogica.Models
{
    public class Trabalho
    {
        public int Id { get; set; }

        [Required]
        public string AlunoId { get; set; } = "";  // inicializado

        [Required]
        public string ProfessorId { get; set; } = "";  // inicializado

        [Required]
        public string Titulo { get; set; } = "";  // inicializado

        public string ConteudoTexto { get; set; } = "";

        // Um ficheiro por vertente
        public string FicheiroCompetencia { get; set; } = "";
        public string FicheiroAptidao { get; set; } = "";
        public string FicheiroConhecimento { get; set; } = "";

        public DateTime DataEnvio { get; set; } = DateTime.Now;

        public bool VistoPeloProfessor { get; set; } = false;

        // Avaliação separada
        public string NotaCompetencia { get; set; } = "";
        public string NotaAptidao { get; set; } = "";
        public string NotaConhecimento { get; set; } = "";
    }
}
