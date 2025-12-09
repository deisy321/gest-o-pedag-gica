using System.Collections.Generic;

namespace gestaopedagogica.Models
{
    public class Roteiro
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = ""; // inicializado
        public List<Tarefa> Tarefas { get; set; } = new(); // inicializado
    }
}
