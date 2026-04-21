using WebPush;
using Microsoft.Extensions.Options;
using gestaopedagogica.Data;
using System.Text.Json;

namespace gestaopedagogica.Services
{
    public class PushService
    {
        private readonly VapidSettings _settings;

        // EVENTO: Permite que outros componentes saibam que algo mudou
        public event Action? OnNotificationReceived;

        public PushService(IOptions<VapidSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task EnviarNotificacaoAsync(string subscriptionJson, string message)
        {
            try
            {
                using var doc = JsonDocument.Parse(subscriptionJson);
                var root = doc.RootElement;

                var endpoint = root.GetProperty("endpoint").GetString();
                var p256dh = root.GetProperty("keys").GetProperty("p256dh").GetString();
                var auth = root.GetProperty("keys").GetProperty("auth").GetString();

                var subscription = new PushSubscription(endpoint, p256dh, auth);

                var vapidDetails = new VapidDetails(
                    _settings.Subject,
                    _settings.PublicKey,
                    _settings.PrivateKey);

                var webPushClient = new WebPushClient();
                await webPushClient.SendNotificationAsync(subscription, message, vapidDetails);

                // NOTIFICAR O NAVBAR: Avisa que uma nova notificação foi processada
                OnNotificationReceived?.Invoke();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao enviar Push: {ex.Message}");
            }
        }
    }
}