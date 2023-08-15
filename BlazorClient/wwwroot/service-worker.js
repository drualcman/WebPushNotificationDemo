self.addEventListener('install', async event => {
    console.log("Instalando el Service worker... ...");
    self.skipWaiting;
});

self.addEventListener('fetch', event => {
    return null;
});
