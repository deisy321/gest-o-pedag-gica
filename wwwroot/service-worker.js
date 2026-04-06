// Este código corre em segundo plano, mesmo com a aba fechada
self.addEventListener('push', function (event) {
    let payload = {
        title: 'Nova Notificação',
        body: 'Tens uma nova atualização no TriadeLearn',
        url: '/'
    };

    if (event.data) {
        try {
            payload = event.data.json();
        } catch (e) {
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

    event.waitUntil(
        self.registration.showNotification(payload.title, options)
    );
});

// Ao clicar na notificação, abre a aplicação
self.addEventListener('notificationclick', function (event) {
    event.notification.close();
    event.waitUntil(
        clients.openWindow(event.notification.data.url || '/')
    );
});