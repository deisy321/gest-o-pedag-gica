using System.Collections.Generic;

namespace gestaopedagogica.Models
{
    public class Modulo
    {
        public int Id { get; set; }
        public string Numero { get; set; } = "";
        public string Nome { get; set; } = "";

        // --- A TRÍADE PEDAGÓGICA ---
        public string CriterioConhecimento { get; set; } = "Avaliação teórica e compreensão de conceitos.";
        public string CriterioCompetencia { get; set; } = "Capacidade de organizar e aplicar o saber.";
        public string CriterioAptidao { get; set; } = "Talento prático e execução individual.";

        // CORREÇÃO: O ponto de interrogação '?' permite que o campo seja opcional (NULO)
        public int? ProfessorId { get; set; }

        // CORREÇÃO: O professor também deve ser anulável para evitar erros de validação
        public Professor? Professor { get; set; }

        // Relacionamento muitos-para-muitos com Turma
        public List<TurmaModulo> Turmas { get; set; } = new();
        public int DisciplinaId { get; set; }
        public Disciplina Disciplina { get; set; } = null!;
    }
}
