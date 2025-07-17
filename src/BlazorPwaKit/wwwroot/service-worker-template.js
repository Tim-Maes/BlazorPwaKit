// BlazorPwaKit Default Service Worker
// This is a basic service worker that can be used as a starting point
// For production use, customize this file based on your caching strategy

const CACHE_NAME = 'blazorpwakit-cache-v1';
const urlsToCache = [
    '/',
    '/index.html',
    '/css/app.css',
    '/js/app.js',
    '/_framework/blazor.webassembly.js'
];

self.addEventListener('install', event => {
    console.log('BlazorPwaKit Service Worker: Install');
    event.waitUntil(
        caches.open(CACHE_NAME)
            .then(cache => {
                console.log('BlazorPwaKit Service Worker: Caching files');
                return cache.addAll(urlsToCache);
            })
    );
});

self.addEventListener('activate', event => {
    console.log('BlazorPwaKit Service Worker: Activate');
    event.waitUntil(
        caches.keys().then(cacheNames => {
            return Promise.all(
                cacheNames.map(cacheName => {
                    if (cacheName !== CACHE_NAME) {
                        console.log('BlazorPwaKit Service Worker: Deleting old cache', cacheName);
                        return caches.delete(cacheName);
                    }
                })
            );
        })
    );
});

self.addEventListener('fetch', event => {
    event.respondWith(
        caches.match(event.request)
            .then(response => {
                // Cache hit - return response
                if (response) {
                    return response;
                }
                return fetch(event.request);
            })
    );
});

// Send message to client when service worker is updated
self.addEventListener('message', event => {
    if (event.data && event.data.type === 'SKIP_WAITING') {
        self.skipWaiting();
    }
});