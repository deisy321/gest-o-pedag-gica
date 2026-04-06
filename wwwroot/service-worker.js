// Este código corre em segundo plano, mesmo com a aba fechada
self.addEventListener('push', function (event) {
    // Payload padrão caso a mensagem venha vazia
    let payload = {
        title: 'Nova Notificação',
        body: 'Tens uma nova atualização no TriadeLearn',
        url: '/'
    };

    if (event.data) {
        try {
            // Tenta ler o JSON enviado pelo teu TrabalhoService.cs
            payload = event.data.json();
        } catch (e) {
            // Se não for JSON, lê como texto simples
            payload.body = event.data.text();
        }
    }

    const options = {
        body: payload.body,
        icon: '/images/logo_triadelearn.png',
        badge: '/images/logo_triadelearn.png',
        vibrate: [100, 50, 100],
        data: { url: payload.url }
    };

    // Exibe a notificação nativa do sistema operativo
    event.waitUntil(
        self.registration.showNotification(payload.title, options)
    );
});

// Ao clicar na notificação, redireciona o utilizador para a app
self.addEventListener('notificationclick', function (event) {
    event.notification.close();
    event.waitUntil(
        clients.openWindow(event.notification.data.url || '/')
    );
});