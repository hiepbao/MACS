// Import Firebase libraries
importScripts("https://www.gstatic.com/firebasejs/9.15.0/firebase-app-compat.js");
importScripts("https://www.gstatic.com/firebasejs/9.15.0/firebase-messaging-compat.js");

// Cấu hình Firebase (không nên để thông tin nhạy cảm trong mã public nếu có thể)
firebase.initializeApp({
    apiKey: "AIzaSyDiBrRn1z-bno2P7_QNs8jmphfC_bmTPRA",
    authDomain: "macs-981ec.firebaseapp.com",
    projectId: "macs-981ec",
    storageBucket: "macs-981ec.appspot.com",
    messagingSenderId: "740480638931",
    appId: "1:740480638931:web:b3e663ef27e7b7ecc89926"
});

const messaging = firebase.messaging();

// Xử lý thông báo nền
messaging.onBackgroundMessage((payload) => {
    console.log("[Service Worker] Nhận thông báo nền:", payload);
    const { title, body, icon } = payload.notification || {};

    self.registration.showNotification(title || "Thông báo", {
        body: body || "Bạn có một thông báo mới!",
        icon: icon || "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcS4fepgMlmqNvoHEYq9sOJ4SSuTwznMOKTq4g&s",
        data: { url: payload.data?.url || "/" }
    });
});

// Xử lý khi người dùng click vào thông báo
self.addEventListener('notificationclick', function (event) {
    const urlToOpen = event.notification.data?.url || "/";
    event.notification.close();
    event.waitUntil(
        clients.matchAll({ type: "window" }).then((windowClients) => {
            for (const client of windowClients) {
                if (client.url === urlToOpen && "focus" in client) {
                    return client.focus();
                }
            }
            return clients.openWindow(urlToOpen);
        })
    );
});
