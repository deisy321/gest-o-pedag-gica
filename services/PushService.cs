using WebPush;
using Microsoft.Extensions.Options;
using gestaopedagogica.Data;
using System.Text.Json;

namespace gestaopedagogica.Services
{
    public class PushService
    {
        private readonly VapidSettings _settings;
        private readonly Dictionary<string, int> _contagensPendentes = new();

        public event Action<string>? OnNotificationReceived;

        public PushService(IOptions<VapidSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task EnviarNotificacaoAsync(string subscriptionJson, string message, string targetUserId)
        {
            try
            {
                // 1. Controle de contagem (Mantido)
                lock (_contagensPendentes)
                {
                    if (!_contagensPendentes.ContainsKey(targetUserId))
                        _contagensPendentes[targetUserId] = 0;

                    _contagensPendentes[targetUserId]++;
                }

                // 2. CRIAR O PAYLOAD JSON (Essencial para o seu Service Worker)
                var payload = new
                {
                    title = "TriadeLearn",
                    body = message,
                    url = "/aluno/dashboardaluno" // Link para onde o aluno vai ao clicar
                };
                string jsonPayload = JsonSerializer.Serialize(payload);

                // 3. Extrair dados da subscrição
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

                // 4. ENVIAR O JSON (jsonPayload em vez de message)
                await webPushClient.SendNotificationAsync(subscription, jsonPayload, vapidDetails);

                // Notifica a Navbar em tempo real
                OnNotificationReceived?.Invoke(targetUserId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao enviar Push: {ex.Message}");
            }
        }

        public int GetNotificacoesCount(string userId)
        {
            lock (_contagensPendentes)
            {
                return _contagensPendentes.TryGetValue(userId, out var count) ? count : 0;
            }
        }

        public void LimparNotificacoes(string userId)
        {
            lock (_contagensPendentes)
            {
                if (_contagensPendentes.ContainsKey(userId))
                    _contagensPendentes[userId] = 0;
            }
        }
    }
}