using System;
using System.Collections.Generic;

namespace gestaopedagogica.Models
{
    // ViewModel só para exibição no dashboard
    public class TrabalhoAluno
    {
        public int TrabalhoId { get; set; }
        public string Titulo { get; set; } = "";

        public string AlunoNome { get; set; } = "Aluno desconhecido";
        public string AlunoEmail { get; set; } = "";

        public string ModuloNome { get; set; } = "Módulo não definido";
        public string DisciplinaNome { get; set; } = "Disciplina não definida";

        public List<VertenteStatus> Vertentes { get; set; } = new();
    }

    public class VertenteStatus
    {
        public string Tipo { get; set; } = "";
        public bool Enviado { get; set; } = false;
        public string Conteudo { get; set; } = "";
    }
}
