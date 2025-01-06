// Import Firebase SDK
importScripts("https://www.gstatic.com/firebasejs/8.10.1/firebase-app.js");
importScripts("https://www.gstatic.com/firebasejs/8.10.1/firebase-messaging.js");

// Cấu hình Firebase
const firebaseConfig = {
    apiKey: "AIzaSyDiBrRn1z-bno2P7_QNs8jmphfC_bmTPRA",
    authDomain: "macs-981ec.firebaseapp.com",
    projectId: "macs-981ec",
    storageBucket: "macs-981ec.firebasestorage.app",
    messagingSenderId: "740480638931",
    appId: "1:740480638931:web:b3e663ef27e7b7ecc89926",
};

// Khởi tạo Firebase
firebase.initializeApp(firebaseConfig);
const messaging = firebase.messaging();

// Xử lý thông báo khi ứng dụng ở chế độ nền
messaging.onBackgroundMessage(async (payload) => {
    console.log("[Service Worker] Nhận thông báo ở chế độ nền:", payload);

    // Kiểm tra và lấy thông tin từ payload
    const { title, body, icon } = payload.notification || {};

    if (!title || !body) {
        console.error("Thông báo không đủ thông tin để hiển thị (thiếu title hoặc body).");
        return;
    }
    
    // Hiển thị thông báo với biểu tượng mặc định nếu không có
    await self.registration.showNotification(title, {
        body,
        icon: icon || defaultIcon, // Thay "default-icon-url.png" bằng đường dẫn biểu tượng mặc định
        image: "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQPiOEI7-694Ef1ym2Uw7HquUBQbyHUfU2N7Q&s", // Thêm hình ảnh lớn
    });

    console.log("[Service Worker] Hiển thị thông báo thành công.");
});

