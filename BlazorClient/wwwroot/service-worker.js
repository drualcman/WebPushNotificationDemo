self.addEventListener('install', async event => {
    console.log("Instalando el Service worker... ...");
    self.skipWaiting;
});

self.addEventListener('fetch', event => {
    return null;
});

self.addEventListener('push', event => {
    console.log('push recibida');
    const payload = event.data.json();
    event.waitUntil(
        self.registration.showNotification('Mensaje importante', {
            body: payload.message,
            icon: 'images/icon-512.png',
            vibrate: [100, 50, 100],
            data: {
                url: payload.url
            }
        })
    );
});
self.addEventListener('notificationclick', event => {
    event.waitUntil(clients.openWindow(event.notification.data.url));
});
