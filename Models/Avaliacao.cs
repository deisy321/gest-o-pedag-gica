namespace gestaopedagogica.Models
{
    public class Avaliacao
    {
        public int Id { get; set; }

        // FK para Aluno
        public int AlunoId { get; set; }
        public Aluno Aluno { get; set; } = null!;

        // Três vertentes
        public double Conhecimento { get; set; }
        public double Aptidao { get; set; }
        public double Competencia { get; set; }

        public DateTime Data { get; set; } = DateTime.UtcNow;

        // Nota final calculada (poderás guardar ou calcular em runtime)
        public double NotaFinal
        {
            get
            {
                // Exemplo: média simples das três vertentes
                return Math.Round((Conhecimento + Aptidao + Competencia) / 3.0, 2);
            }
        }
    }
}
