
// Kiểm tra nền tảng iOS
const isIos = /iPad|iPhone|iPod/.test(navigator.userAgent) && !window.MSStream;
const isSafari = /^((?!chrome|android).)*safari/i.test(navigator.userAgent);

// Hàm chính
const main = async () => {

    // Khởi tạo Firebase Messaging
    const messaging = firebase.messaging();

    try {
        const permission = await Notification.requestPermission();
        if (permission !== "granted") {
            console.error("Người dùng không cấp quyền thông báo.");
        }

        const vapidKey =
            "BBE_KozcLeKHmgrx7KnFYG3V71uGgO3Jh1Bx9nctAJs6NvmgxDY-Hckvm-cw4Z23xo3eZPweX2ZmvP_rcFS8K0U";

        // Lấy FCM token
        const token = await messaging.getToken({ vapidKey });
        if (!token) {
            console.error("Không thể lấy FCM token.");
            return;
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
        }
        // Phát âm thanh thông báo
        //const notificationSound = new Audio("https://notificationsounds.com/notification-sounds/ill-make-it-possible-notification/download/mp3"); // Đường dẫn tới file âm thanh
        //notificationSound.play().catch((err) => {
        //    console.error("Lỗi khi phát âm thanh:", err);
        //});

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

main();
