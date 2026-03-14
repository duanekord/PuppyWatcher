// Minimal service worker for PWA installability.
// This app uses Blazor InteractiveServer (SignalR), so offline caching is not applicable.

self.addEventListener('install', event => {
    self.skipWaiting();
});

self.addEventListener('activate', event => {
    event.waitUntil(clients.claim());
});

self.addEventListener('fetch', event => {
    // Let all requests pass through to the network
});
