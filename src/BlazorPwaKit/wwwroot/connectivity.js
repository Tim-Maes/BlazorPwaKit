let dotNetRef = null;

export function registerConnectivityHandler(dotNetObjectRef) {
    dotNetRef = dotNetObjectRef;
    window.addEventListener('online', notifyOnline);
    window.addEventListener('offline', notifyOffline);
}

export function disposeConnectivityHandler() {
    window.removeEventListener('online', notifyOnline);
    window.removeEventListener('offline', notifyOffline);
    dotNetRef = null;
}

export function getIsOnline() {
    return navigator.onLine;
}

function notifyOnline() {
    if (dotNetRef) dotNetRef.invokeMethodAsync('OnConnectivityChanged', true);
}

function notifyOffline() {
    if (dotNetRef) dotNetRef.invokeMethodAsync('OnConnectivityChanged', false);
}
