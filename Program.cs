using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using MACS.Models;
using MACS.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MACS.Data;
using MACS.Controllers;
using MACSAPI.Data;
using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();
builder.Services.AddHttpClient<UploadHistoryService>();
builder.Services.AddHttpClient<QRCodeService>();
builder.Services.AddHttpClient<AuthService>();
builder.Services.AddHttpClient<HistoryCarService>();
builder.Services.AddHttpClient<HomeController>();
builder.Services.AddHttpClient<TokenService>();  


builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiBaseUrl"));
builder.Services.Configure<FirebaseConfig>(builder.Configuration.GetSection("FirebaseConfig"));
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<TokenService>();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseInMemoryDatabase("InMemoryDb");
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});


// Session configuration
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

//// Configure Firebase Admin SDK cho local
//var firebaseJson = builder.Configuration["FirebaseConfig:Json"];
//if (!string.IsNullOrEmpty(firebaseJson))
//{
//    FirebaseApp.Create(new AppOptions
//    {
//        Credential = GoogleCredential.FromJson(firebaseJson)
//    });
//}
//else
//{
//    throw new InvalidOperationException("Firebase configuration is missing.");
//}

// Lấy cấu hình Firebase từ biến môi trường cho host
var firebaseJson = Environment.GetEnvironmentVariable("FIREBASE_CONFIG");

// Kiểm tra nếu biến môi trường không tồn tại hoặc rỗng
if (string.IsNullOrEmpty(firebaseJson))
{
    Console.WriteLine("Firebase configuration is missing.");
    throw new InvalidOperationException("Firebase configuration is missing.");
}

try
{
    // In nội dung JSON ra console để kiểm tra (chỉ sử dụng trong môi trường debug)
    Console.WriteLine("Firebase JSON Loaded:");
    Console.WriteLine(firebaseJson);

    // Tạo FirebaseApp với cấu hình
    FirebaseApp.Create(new AppOptions
    {
        Credential = GoogleCredential.FromJson(firebaseJson)
    });

    Console.WriteLine("FirebaseApp initialized successfully.");
}
catch (Exception ex)
{
    // Log lỗi chi tiết
    Console.WriteLine($"Error initializing FirebaseApp: {ex.Message}");
    Console.WriteLine($"Stack Trace: {ex.StackTrace}");
    throw;
}



var app = builder.Build();


// Middleware configuration
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseMiddleware<MACS.Middlewares.UserTokenMiddleware>();

app.UseWhen(context => !context.Request.Path.Value.StartsWith("/firebase-messaging-sw.js"), appBuilder =>
{
    appBuilder.UseAuthentication();
});
app.UseCors("AllowAll");


app.UseSession();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{cardNo?}");

app.Run();
