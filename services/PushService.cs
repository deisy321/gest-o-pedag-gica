using WebPush;
using Microsoft.Extensions.Options;
using gestaopedagogica.Data; // <--- ISTO resolve o erro "VapidSettings não encontrado"
using System.Text.Json;

namespace gestaopedagogica.Services
{
    public class PushService
    {
        private readonly VapidSettings _settings;

        public PushService(IOptions<VapidSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task EnviarNotificacaoAsync(string subscriptionJson, string message)
        {
            try
            {
                // Extrai os dados do JSON que guardaste na Base de Dados
                using var doc = JsonDocument.Parse(subscriptionJson);
                var root = doc.RootElement;

                var endpoint = root.GetProperty("endpoint").GetString();
                var p256dh = root.GetProperty("keys").GetProperty("p256dh").GetString();
                var auth = root.GetProperty("keys").GetProperty("auth").GetString();

                // ISTO resolve o erro CS7036 (falta de argumentos)
                var subscription = new PushSubscription(endpoint, p256dh, auth);

                var vapidDetails = new VapidDetails(
                    _settings.Subject,
                    _settings.PublicKey,
                    _settings.PrivateKey);

                var webPushClient = new WebPushClient();
                await webPushClient.SendNotificationAsync(subscription, message, vapidDetails);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao enviar Push: {ex.Message}");
            }
        }
    }
}