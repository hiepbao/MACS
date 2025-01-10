// Cấu hình Firebase
const firebaseConfig = {
    apiKey: "AIzaSyDiBrRn1z-bno2P7_QNs8jmphfC_bmTPRA",
    authDomain: "macs-981ec.firebaseapp.com",
    projectId: "macs-981ec",
    storageBucket: "macs-981ec.appspot.com",
    messagingSenderId: "740480638931",
    appId: "1:740480638931:web:b3e663ef27e7b7ecc89926",
    vapidKey: "BBE_KozcLeKHmgrx7KnFYG3V71uGgO3Jh1Bx9nctAJs6NvmgxDY-Hckvm-cw4Z23xo3eZPweX2ZmvP_rcFS8K0U"
};

// Khởi tạo Firebase app
const app = firebase.initializeApp(firebaseConfig);
const messaging = firebase.messaging();

// Đăng ký Service Worker
if ('serviceWorker' in navigator) {
    navigator.serviceWorker.register('/firebase-messaging-sw.js')
        .then((registration) => {
            console.log('Service Worker đăng ký thành công:', registration.scope);
            messaging.useServiceWorker(registration);
        })
        .catch((error) => {
            console.error('Lỗi đăng ký Service Worker:', error);
        });
} else {
    console.error('Trình duyệt không hỗ trợ Service Worker.');
}

// Kiểm tra nền tảng iOS hoặc Safari
const isIos = /iPad|iPhone|iPod/.test(navigator.userAgent) && !window.MSStream;
const isSafari = /^((?!chrome|android).)*safari/i.test(navigator.userAgent);

// Hàm chính
const main = async () => {
    try {
        // Yêu cầu quyền thông báo
        const permission = await Notification.requestPermission();
        if (permission !== "granted") {
            console.warn("Người dùng không cấp quyền thông báo.");
            return;
        }

        // Lấy FCM token
        const token = await messaging.getToken({
            vapidKey: firebaseConfig.vapidKey
        });
        if (!token) {
            console.error("Không thể lấy FCM token.");
            return;
        }

        console.log("FCM Token:", token);

        // Gửi token lên server
        const response = await fetch("/Home/SaveToken", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ token }),
        });

        if (!response.ok) {
            const errorText = await response.text();
            throw new Error(`Lỗi HTTP: ${response.status}. Chi tiết: ${errorText}`);
        }

        console.log("Token đã được gửi lên server thành công.");
    } catch (error) {
        console.error("Lỗi trong quá trình xử lý thông báo:", error);
    }

    // Lắng nghe thông báo khi ứng dụng đang mở
    messaging.onMessage((payload) => {
        console.log("Nhận thông báo trong chế độ foreground:", payload);

        const { title, body, icon } = payload.notification || {};

        if (Notification.permission !== "granted") {
            console.warn("Quyền thông báo chưa được cấp.");
            return;
        }

        // Hiển thị thông báo với âm thanh
        const notificationOptions = {
            body,
            icon: icon || "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcS4fepgMlmqNvoHEYq9sOJ4SSuTwznMOKTq4g&s",
            image: "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQPiOEI7-694Ef1ym2Uw7HquUBQbyHUfU2N7Q&s",
            //requireInteraction: true // Thông báo sẽ giữ lại cho đến khi người dùng tương tác
        };

        const notification = new Notification(title || "Thông báo", notificationOptions);

        // Xử lý khi người dùng nhấp vào thông báo
        notification.onclick = () => {
            window.open(payload.data?.url || "/", "_blank");
        };
    });
};

main();
