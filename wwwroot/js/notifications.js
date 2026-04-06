// Função essencial para converter a Chave Pública VAPID
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

// Objeto que o Blazor invoca (JSRuntime.InvokeAsync)
window.blazorPushNotifications = {
    requestSubscription: async (publicKey) => {
        try {
            // Aguarda que o Service Worker esteja pronto e ativo
            const worker = await navigator.serviceWorker.ready;

            // Solicita a subscrição ao serviço de Push do Browser (Google/Mozilla/etc)
            const subscription = await worker.pushManager.subscribe({
                userVisibleOnly: true,
                applicationServerKey: urlBase64ToUint8Array(publicKey)
            });

            // Retorna o objeto de subscrição como JSON para o C# guardar na BD
            return JSON.stringify(subscription);
        } catch (error) {
            console.error("Erro detalhado na subscrição:", error);
            return null;
        }
    }
};