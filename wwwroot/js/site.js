// Hàm lấy giá trị cookie
const getCookie = (name) => {
    const cookie = document.cookie
        .split(";")
        .map((c) => c.trim())
        .find((c) => c.startsWith(`${name}=`));
    return cookie ? cookie.split("=")[1] : null;
};

// Hàm phân tích JWT
const parseJwt = (token) => {
    try {
        const base64Url = token.split(".")[1];
        const base64 = base64Url.replace(/-/g, "+").replace(/_/g, "/");
        const jsonPayload = decodeURIComponent(
            atob(base64)
                .split("")
                .map((c) => `%${("00" + c.charCodeAt(0).toString(16)).slice(-2)}`)
                .join("")
        );
        return JSON.parse(jsonPayload);
    } catch (error) {
        console.error("Lỗi khi phân tích JWT:", error);
        return null;
    }
};

// Hàm chính
const main = async () => {
    const jwt = getCookie("UserToken");

    if (!jwt) {
        console.error("JWT không tồn tại. Không thể xác minh quyền.");
        return;
    }

    const decodedJwt = parseJwt(jwt);
    if (!decodedJwt || decodedJwt.role !== "store") {
        console.error("Người dùng không có quyền 'store' hoặc JWT không hợp lệ.");
        return;
    }

    console.log("Người dùng được xác minh với quyền 'store'.");

    // Khởi tạo Firebase Messaging
    const messaging = firebase.messaging();

    try {
        const permission = await Notification.requestPermission();
        if (permission !== "granted") {
            console.error("Người dùng không cấp quyền thông báo.");
        }

        console.log("Quyền thông báo đã được cấp.");

        const vapidKey =
            "BBE_KozcLeKHmgrx7KnFYG3V71uGgO3Jh1Bx9nctAJs6NvmgxDY-Hckvm-cw4Z23xo3eZPweX2ZmvP_rcFS8K0U";

        // Lấy FCM token
        const token = await messaging.getToken({ vapidKey });
        if (!token) {
            console.error("Không thể lấy FCM token.");
            return;
        }

        console.log("Token mới nhận được:", token);

        const savedToken = localStorage.getItem("FCMToken");
        if (token === savedToken) {
            console.log("Token đã được lưu trước đó. Không cần gửi lại.");
        }

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
        localStorage.setItem("FCMToken", token);
    } catch (error) {
        console.error("Lỗi trong quá trình xử lý thông báo:", error);
    }

    // Lắng nghe thông báo khi ứng dụng đang mở
    messaging.onMessage((payload) => {
        console.log("Nhận thông báo trong chế độ foreground:", payload);

        const { title, body, icon } = payload.notification || {};

        // Kiểm tra quyền thông báo
        if (Notification.permission !== "granted") {
            console.error("Quyền thông báo chưa được cấp.");
            return;
        }
        // Phát âm thanh thông báo
        const notificationSound = new Audio("https://notificationsounds.com/notification-sounds/ill-make-it-possible-notification/download/mp3"); // Đường dẫn tới file âm thanh
        notificationSound.play().catch((err) => {
            console.error("Lỗi khi phát âm thanh:", err);
        });

        // Hiển thị thông báo
        if (title && body) {
            new Notification(title, {
                body,
                icon: icon || "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcS4fepgMlmqNvoHEYq9sOJ4SSuTwznMOKTq4g&s", // Thêm biểu tượng mặc định nếu không có
                image: "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQPiOEI7-694Ef1ym2Uw7HquUBQbyHUfU2N7Q&s", // Thêm hình ảnh lớn
            });
        } else {
            console.error("Thông báo không đủ thông tin để hiển thị.");
        }
    });

};

// Chạy hàm chính
main();
