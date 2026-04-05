public class NotificationSubscription
{
    public int Id { get; set; }
    public string UserId { get; set; } // ID do Aluno ou Professor
    public string Payload { get; set; } // O JSON gigante que o JS devolve
}