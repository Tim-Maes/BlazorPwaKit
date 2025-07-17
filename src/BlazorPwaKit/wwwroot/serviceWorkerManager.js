// BlazorPwaKit Service Worker Manager JavaScript Module
let dotNetReference = null;
let registrations = new Map();

export function initialize(dotNetObjectReference) {
    dotNetReference = dotNetObjectReference;
    
    // Listen for service worker events if supported
    if ('serviceWorker' in navigator) {
        navigator.serviceWorker.addEventListener('message', (event) => {
            if (dotNetReference) {
                if (event.data && event.data.type === 'UPDATE_AVAILABLE') {
                    dotNetReference.invokeMethodAsync('OnServiceWorkerEvent', 'updateavailable', null);
                } else {
                    dotNetReference.invokeMethodAsync('OnServiceWorkerEvent', 'message', event.data);
                }
            }
        });
    }
}

export function isServiceWorkerSupported() {
    return 'serviceWorker' in navigator;
}

export async function registerServiceWorker(scriptUrl, scope) {
    if (!isServiceWorkerSupported()) {
        throw new Error('Service workers are not supported in this browser');
    }

    try {
        const options = scope ? { scope } : undefined;
        const registration = await navigator.serviceWorker.register(scriptUrl, options);
        
        // Store registration for later reference
        const key = `${scriptUrl}:${scope || '/'}`;
        registrations.set(key, registration);
        
        // Set up event listeners
        setupRegistrationEventListeners(registration);
        
        return true;
    } catch (error) {
        console.error('Service worker registration failed:', error);
        if (dotNetReference) {
            dotNetReference.invokeMethodAsync('OnServiceWorkerEvent', 'error', error.message);
        }
        return false;
    }
}

export async function updateServiceWorker(scriptUrl, scope) {
    if (!isServiceWorkerSupported()) {
        return false;
    }

    try {
        const key = `${scriptUrl}:${scope || '/'}`;
        let registration = registrations.get(key);
        
        if (!registration) {
            // Try to get existing registration
            registration = await navigator.serviceWorker.getRegistration(scope);
            if (registration) {
                registrations.set(key, registration);
            }
        }
        
        if (registration) {
            const updatedRegistration = await registration.update();
            if (dotNetReference) {
                dotNetReference.invokeMethodAsync('OnServiceWorkerEvent', 'updated', null);
            }
            return true;
        }
        
        return false;
    } catch (error) {
        console.error('Service worker update failed:', error);
        if (dotNetReference) {
            dotNetReference.invokeMethodAsync('OnServiceWorkerEvent', 'error', error.message);
        }
        return false;
    }
}

export async function unregisterServiceWorker(scriptUrl, scope) {
    if (!isServiceWorkerSupported()) {
        return false;
    }

    try {
        const key = `${scriptUrl}:${scope || '/'}`;
        let registration = registrations.get(key);
        
        if (!registration) {
            // Try to get existing registration
            registration = await navigator.serviceWorker.getRegistration(scope);
        }
        
        if (registration) {
            const success = await registration.unregister();
            if (success) {
                registrations.delete(key);
            }
            return success;
        }
        
        return false;
    } catch (error) {
        console.error('Service worker unregistration failed:', error);
        if (dotNetReference) {
            dotNetReference.invokeMethodAsync('OnServiceWorkerEvent', 'error', error.message);
        }
        return false;
    }
}

export async function getServiceWorkerState() {
    if (!isServiceWorkerSupported()) {
        return null;
    }

    try {
        const registration = await navigator.serviceWorker.getRegistration();
        if (registration) {
            if (registration.active) {
                return registration.active.state;
            } else if (registration.waiting) {
                return 'waiting';
            } else if (registration.installing) {
                return 'installing';
            }
        }
        return 'not-registered';
    } catch (error) {
        console.error('Failed to get service worker state:', error);
        return null;
    }
}

export async function setCachePolicies(policies) {
    // Send cache policies to the active service worker
    if ('serviceWorker' in navigator && navigator.serviceWorker.controller) {
        navigator.serviceWorker.controller.postMessage({
            type: 'SET_CACHE_POLICIES',
            policies: policies
        });
    } else {
        // Wait for the service worker to become active
        navigator.serviceWorker.ready.then(reg => {
            if (reg.active) {
                reg.active.postMessage({
                    type: 'SET_CACHE_POLICIES',
                    policies: policies
                });
            }
        });
    }
}

export async function setOfflineFallbackPath(path) {
    if ('serviceWorker' in navigator && navigator.serviceWorker.controller) {
        navigator.serviceWorker.controller.postMessage({
            type: 'SET_OFFLINE_FALLBACK_PATH',
            path: path
        });
    } else {
        navigator.serviceWorker.ready.then(reg => {
            if (reg.active) {
                reg.active.postMessage({
                    type: 'SET_OFFLINE_FALLBACK_PATH',
                    path: path
                });
            }
        });
    }
}

function setupRegistrationEventListeners(registration) {
    if (registration.installing) {
        setupServiceWorkerEventListeners(registration.installing);
    }
    
    if (registration.waiting) {
        setupServiceWorkerEventListeners(registration.waiting);
    }
    
    if (registration.active) {
        setupServiceWorkerEventListeners(registration.active);
    }
    
    registration.addEventListener('updatefound', () => {
        const newWorker = registration.installing;
        if (newWorker) {
            setupServiceWorkerEventListeners(newWorker);
        }
    });
}

function setupServiceWorkerEventListeners(serviceWorker) {
    serviceWorker.addEventListener('statechange', () => {
        if (dotNetReference) {
            const eventType = serviceWorker.state === 'activated' ? 'activate' : 
                             serviceWorker.state === 'installed' ? 'install' : 
                             serviceWorker.state;
            dotNetReference.invokeMethodAsync('OnServiceWorkerEvent', eventType, serviceWorker.state);
        }
    });
}

export function dispose() {
    dotNetReference = null;
    registrations.clear();
}