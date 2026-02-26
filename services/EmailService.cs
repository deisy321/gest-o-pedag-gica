using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace gestaopedagogica.Services
{
    public interface IEmailService
    {
        Task EnviarTrabalhoConfirmacaoAsync(string emailAluno, string nomeAluno, string titulo, DateTime prazo);
        Task EnviarAvaliacaoAsync(string emailAluno, string nomeAluno, string titulo, decimal nota, string feedback);
   Task EnviarLembreteAsync(string emailAluno, string nomeAluno, string titulo, TimeSpan tempoRestante);
    }

  public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
     private readonly string _senderEmail;
   private readonly string _senderPassword;
        private readonly string _smtpServer;
        private readonly int _smtpPort;

        public EmailService(IConfiguration configuration)
 {
            _configuration = configuration;
  _senderEmail = _configuration["EmailSettings:SenderEmail"] ?? "triadelearn@gmail.com";
            _senderPassword = _configuration["EmailSettings:SenderPassword"] ?? "";
     _smtpServer = _configuration["EmailSettings:SmtpServer"] ?? "smtp.gmail.com";
       _smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
        }

    public async Task EnviarTrabalhoConfirmacaoAsync(string emailAluno, string nomeAluno, string titulo, DateTime prazo)
        {
            try
        {
   var subject = "?? Novo Trabalho Disponível - TriadeLearn";
        var body = $@"
<html>
    <head>
     <style>
     body {{ font-family: 'Segoe UI', Roboto, sans-serif; background-color: #f5f5f5; }}
    .container {{ max-width: 600px; margin: 0 auto; background-color: white; padding: 20px; border-radius: 10px; box-shadow: 0 2px 5px rgba(0,0,0,0.1); }}
  .header {{ background-color: #1e3a8a; color: white; padding: 20px; text-align: center; border-radius: 10px; margin-bottom: 20px; }}
            .content {{ color: #333; line-height: 1.6; }}
     .info {{ background-color: #f0f9ff; padding: 15px; border-left: 4px solid #0d9488; margin: 15px 0; border-radius: 5px; }}
   .button {{ display: inline-block; background-color: #0d9488; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; margin-top: 15px; }}
     .footer {{ text-align: center; color: #888; font-size: 12px; margin-top: 30px; border-top: 1px solid #eee; padding-top: 15px; }}
        </style>
    </head>
    <body>
        <div class='container'>
          <div class='header'>
    <h1>?? Novo Trabalho Disponível</h1>
     </div>
      <div class='content'>
         <p>Olá <strong>{nomeAluno}</strong>,</p>
       <p>Um novo trabalho foi criado para você pelo seu professor!</p>
           
  <div class='info'>
              <p><strong>?? Título:</strong> {titulo}</p>
    <p><strong>?? Prazo de Entrega:</strong> {prazo:dd/MM/yyyy HH:mm}</p>
      <p><strong>? Dias Restantes:</strong> {(prazo - DateTime.Now).Days} dias</p>
            </div>
        
        <p>Acesse sua dashboard para visualizar os detalhes do trabalho e começar a resolver.</p>
   
           <a href='http://localhost:5001/aluno/DashboardAluno' class='button'>Ver Meu Trabalho</a>
                
      <p>Boa sorte! ??</p>
            </div>
     <div class='footer'>
   <p>TriadeLearn - Sistema de Gestão Pedagógica</p>
          <p>Esta é uma mensagem automática. Não responda diretamente.</p>
            </div>
        </div>
    </body>
</html>";

      await EnviarEmailAsync(emailAluno, subject, body);
            }
     catch (Exception ex)
{
         Console.WriteLine($"? Erro ao enviar email de confirmação: {ex.Message}");
 }
        }

  public async Task EnviarAvaliacaoAsync(string emailAluno, string nomeAluno, string titulo, decimal nota, string feedback)
        {
  try
      {
       var subject = "? Seu Trabalho foi Avaliado - TriadeLearn";
          var body = $@"
<html>
    <head>
 <style>
      body {{ font-family: 'Segoe UI', Roboto, sans-serif; background-color: #f5f5f5; }}
   .container {{ max-width: 600px; margin: 0 auto; background-color: white; padding: 20px; border-radius: 10px; box-shadow: 0 2px 5px rgba(0,0,0,0.1); }}
  .header {{ background-color: #059669; color: white; padding: 20px; text-align: center; border-radius: 10px; margin-bottom: 20px; }}
     .nota {{ font-size: 32px; font-weight: bold; color: #059669; text-align: center; margin: 20px 0; }}
   .feedback {{ background-color: #f0fdf4; padding: 15px; border-left: 4px solid #059669; margin: 15px 0; border-radius: 5px; }}
          .button {{ display: inline-block; background-color: #059669; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; margin-top: 15px; }}
            .footer {{ text-align: center; color: #888; font-size: 12px; margin-top: 30px; border-top: 1px solid #eee; padding-top: 15px; }}
        </style>
 </head>
    <body>
        <div class='container'>
<div class='header'>
      <h1>? Sua Avaliação Está Pronta!</h1>
    </div>
            <div>
       <p>Olá <strong>{nomeAluno}</strong>,</p>
     <p>Seu professor avaliou o trabalho: <strong>{titulo}</strong></p>
         
      <div class='nota'>{nota:F1} / 20</div>
   
       <div class='feedback'>
          <p><strong>?? Feedback do Professor:</strong></p>
<p>{feedback}</p>
      </div>
           
       <p>Acesse sua dashboard para ver os detalhes completos da avaliação.</p>
     
      <a href='http://localhost:5001/aluno/DashboardAluno' class='button'>Ver Avaliação Completa</a>
      
          <p>Continue com o ótimo trabalho! ??</p>
  </div>
     <div class='footer'>
 <p>TriadeLearn - Sistema de Gestão Pedagógica</p>
   </div>
        </div>
    </body>
</html>";

 await EnviarEmailAsync(emailAluno, subject, body);
    }
 catch (Exception ex)
     {
           Console.WriteLine($"? Erro ao enviar email de avaliação: {ex.Message}");
}
        }

        public async Task EnviarLembreteAsync(string emailAluno, string nomeAluno, string titulo, TimeSpan tempoRestante)
        {
    try
      {
          var subject = "? Lembrete: Seu Trabalho Vence em Breve - TriadeLearn";
          var diasRestantes = tempoRestante.Days;
              var horasRestantes = tempoRestante.Hours;

          var body = $@"
<html>
    <head>
 <style>
     body {{ font-family: 'Segoe UI', Roboto, sans-serif; background-color: #f5f5f5; }}
            .container {{ max-width: 600px; margin: 0 auto; background-color: white; padding: 20px; border-radius: 10px; box-shadow: 0 2px 5px rgba(0,0,0,0.1); }}
      .header {{ background-color: #ea580c; color: white; padding: 20px; text-align: center; border-radius: 10px; margin-bottom: 20px; }}
      .warning {{ background-color: #fef3c7; padding: 15px; border-left: 4px solid #ea580c; margin: 15px 0; border-radius: 5px; }}
 .button {{ display: inline-block; background-color: #ea580c; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; margin-top: 15px; }}
     .footer {{ text-align: center; color: #888; font-size: 12px; margin-top: 30px; border-top: 1px solid #eee; padding-top: 15px; }}
        </style>
    </head>
    <body>
        <div class='container'>
            <div class='header'>
    <h1>? Lembrete de Prazo!</h1>
            </div>
      <div>
       <p>Olá <strong>{nomeAluno}</strong>,</p>
                <p>Este é um lembrete de que seu trabalho vence em breve!</p>
                
                <div class='warning'>
   <p><strong>?? Trabalho:</strong> {titulo}</p>
    <p><strong>?? Tempo Restante:</strong> {diasRestantes} dias e {horasRestantes} horas</p>
      </div>
      
     <p>Não se esqueça de enviar sua resposta antes do prazo!</p>
        
         <a href='http://localhost:5001/aluno/DashboardAluno' class='button'>Ir para Meu Trabalho</a>
 
    <p>Bom trabalho! ??</p>
     </div>
            <div class='footer'>
     <p>TriadeLearn - Sistema de Gestão Pedagógica</p>
            </div>
</div>
    </body>
</html>";

      await EnviarEmailAsync(emailAluno, subject, body);
     }
            catch (Exception ex)
   {
  Console.WriteLine($"? Erro ao enviar lembrete: {ex.Message}");
    }
      }

        private async Task EnviarEmailAsync(string destinatario, string assunto, string corpo)
        {
 try
     {
                using (var client = new SmtpClient(_smtpServer, _smtpPort))
             {
     client.EnableSsl = true;
  client.Credentials = new NetworkCredential(_senderEmail, _senderPassword);

      var mailMessage = new MailMessage
           {
         From = new MailAddress(_senderEmail),
       Subject = assunto,
      Body = corpo,
        IsBodyHtml = true
  };

           mailMessage.To.Add(destinatario);

            await client.SendMailAsync(mailMessage);
    Console.WriteLine($"? Email enviado para {destinatario}");
           }
 }
            catch (Exception ex)
    {
        Console.WriteLine($"? Erro ao enviar email: {ex.Message}");
  throw;
            }
        }
    }
}
