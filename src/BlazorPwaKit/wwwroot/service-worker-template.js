// BlazorPwaKit Advanced Service Worker Template
// Supports per-resource caching strategies via postMessage from Blazor

const CACHE_NAME = 'blazorpwakit-cache-v1';
let cachePolicies = {};
let offlineFallbackPath = '/offline';

// Listen for offline fallback path from Blazor
self.addEventListener('message', event => {
    if (event.data && event.data.type === 'SET_OFFLINE_FALLBACK_PATH') {
        offlineFallbackPath = event.data.path || '/offline';
    }
    if (event.data && event.data.type === 'SET_CACHE_POLICIES') {
        cachePolicies = event.data.policies || {};
        console.log('BlazorPwaKit SW: Received cache policies', cachePolicies);
    }
    if (event.data && event.data.type === 'SKIP_WAITING') {
        self.skipWaiting();
    }
});

const strategies = {
    CacheFirst: async (event) => {
        const cache = await caches.open(CACHE_NAME);
        const cached = await cache.match(event.request);
        if (cached) return cached;
        const response = await fetch(event.request);
        if (response && response.ok) cache.put(event.request, response.clone());
        return response;
    },
    NetworkFirst: async (event) => {
        try {
            const response = await fetch(event.request);
            if (response && response.ok) {
                const cache = await caches.open(CACHE_NAME);
                cache.put(event.request, response.clone());
                return response;
            }
        } catch {}
        const cache = await caches.open(CACHE_NAME);
        return cache.match(event.request);
    },
    StaleWhileRevalidate: async (event) => {
        const cache = await caches.open(CACHE_NAME);
        const cached = await cache.match(event.request);
        const fetchPromise = fetch(event.request).then(response => {
            if (response && response.ok) cache.put(event.request, response.clone());
            return response;
        });
        return cached || fetchPromise;
    },
    NetworkOnly: async (event) => fetch(event.request),
    CacheOnly: async (event) => {
        const cache = await caches.open(CACHE_NAME);
        return cache.match(event.request);
    }
};

self.addEventListener('install', event => {
    self.skipWaiting();
});

self.addEventListener('activate', event => {
    event.waitUntil(
        caches.keys().then(cacheNames => {
            return Promise.all(
                cacheNames.map(cacheName => {
                    if (cacheName !== CACHE_NAME) {
                        return caches.delete(cacheName);
                    }
                })
            );
        })
    );
    self.clients.claim();
});

self.addEventListener('fetch', event => {
    const url = event.request.url;
    const policy = getPolicyForUrl(url);
    if (event.request.mode === 'navigate') {
        event.respondWith(
            fetch(event.request)
                .catch(async () => {
                    const cache = await caches.open(CACHE_NAME);
                    const fallback = await cache.match(offlineFallbackPath);
                    return fallback || Response.error();
                })
        );
        return;
    }
    if (policy && strategies[policy]) {
        event.respondWith(strategies[policy](event));
    } else {
        // Default: NetworkFirst
        event.respondWith(strategies.NetworkFirst(event));
    }
});

function getPolicyForUrl(url) {
    for (const pattern in cachePolicies) {
        if (url.includes(pattern)) {
            return cachePolicies[pattern];
        }
    }
    return null;
}

// Notify clients when a new service worker is waiting (update available)
function notifyClientsAboutUpdate() {
    self.clients.matchAll({ type: 'window' }).then(clients => {
        for (const client of clients) {
            client.postMessage({ type: 'UPDATE_AVAILABLE' });
        }
    });
}
self.addEventListener('updatefound', () => {
    if (self.registration.installing) {
        self.registration.installing.addEventListener('statechange', event => {
            if (event.target.state === 'installed' && self.registration.waiting) {
                notifyClientsAboutUpdate();
            }
        });
    }
});