// Função para converter a chave VAPID para o formato que o navegador entende
function urlBase64ToUint8Array(base64String) {
    const padding = '='.repeat((4 - base64String.length % 4) % 4);
    const base64 = (base64String + padding).replace(/-/g, '+').replace(/_/g, '/');
    const rawData = window.atob(base64);
    const outputArray = new Uint8Array(rawData.length);
    for (let i = 0; i < rawData.length; ++i) {
        outputArray[i] = rawData.charCodeAt(i);
    }
    return outputArray;
}

window.blazorPushNotifications = {
    // Solicita subscrição ao navegador
    requestSubscription: async (publicKey) => {
        try {
            const worker = await navigator.serviceWorker.ready;

            // Tenta obter subscrição existente
            const existingSub = await worker.pushManager.getSubscription();
            if (existingSub) await existingSub.unsubscribe();

            const subscription = await worker.pushManager.subscribe({
                userVisibleOnly: true,
                applicationServerKey: urlBase64ToUint8Array(publicKey)
            });

            return JSON.stringify(subscription);
        } catch (error) {
            console.error("Erro na subscrição JS:", error);
            return null;
        }
    },

    // Verifica se o utilizador já está subscrito (para mudar o botão no Blazor)
    checkSubscription: async () => {
        if (!('serviceWorker' in navigator)) return false;
        const worker = await navigator.serviceWorker.ready;
        const subscription = await worker.pushManager.getSubscription();
        return !!subscription;
    },

    // Cancela a subscrição
    unsubscribe: async () => {
        const worker = await navigator.serviceWorker.ready;
        const subscription = await worker.pushManager.getSubscription();
        if (subscription) {
            return await subscription.unsubscribe();
        }
        return false;
    }
};