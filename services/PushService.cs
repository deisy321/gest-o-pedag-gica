using WebPush;
using Microsoft.Extensions.Options;
using gestaopedagogica.Data;
using System.Text.Json;

namespace gestaopedagogica.Services
{
    public class PushService
    {
        private readonly VapidSettings _settings;

        // NOVO: Dicionário para manter a contagem de notificações por Utilizador na memória do servidor
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
                // 1. Incrementar o contador interno antes de enviar
                lock (_contagensPendentes)
                {
                    if (!_contagensPendentes.ContainsKey(targetUserId))
                        _contagensPendentes[targetUserId] = 0;

                    _contagensPendentes[targetUserId]++;
                }

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

                // NOTIFICAR O NAVBAR (Evento em tempo real)
                OnNotificationReceived?.Invoke(targetUserId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao enviar Push: {ex.Message}");
            }
        }

        // NOVO: Método para a Navbar consultar o estado ao carregar
        public int GetNotificacoesCount(string userId)
        {
            lock (_contagensPendentes)
            {
                return _contagensPendentes.TryGetValue(userId, out var count) ? count : 0;
            }
        }

        // NOVO: Método para zerar quando o utilizador clica no sino
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