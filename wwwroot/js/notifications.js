window.blazorPushNotifications = {
    // ... tua função requestSubscription existente ...

    // NOVA: Verifica se já existe subscrição ativa
    checkSubscription: async () => {
        const worker = await navigator.serviceWorker.ready;
        const subscription = await worker.pushManager.getSubscription();
        return !!subscription;
    },

    // NOVA: Cancela a subscrição no navegador
    unsubscribe: async () => {
        const worker = await navigator.serviceWorker.ready;
        const subscription = await worker.pushManager.getSubscription();
        if (subscription) {
            return await subscription.unsubscribe();
        }
        return false;
    }
};