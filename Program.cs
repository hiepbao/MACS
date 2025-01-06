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

builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiBaseUrl"));
builder.Services.Configure<FirebaseConfig>(builder.Configuration.GetSection("FirebaseConfig"));
builder.Services.AddSingleton<NotificationService>();

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

builder.Services.AddScoped<NotificationService>();

// Session configuration
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

//// Configure Firebase Admin SDK
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

// Lấy cấu hình Firebase từ biến môi trường
var firebaseJson2 = Environment.GetEnvironmentVariable("FIREBASE_CONFIG");

if (!string.IsNullOrEmpty(firebaseJson2))
{
    FirebaseApp.Create(new AppOptions
    {
        Credential = GoogleCredential.FromJson(firebaseJson2)
    });
}
else
{
    throw new InvalidOperationException("Firebase configuration is missing.");
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

app.UseCors("AllowAll");

app.UseSession();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{cardNo?}");

app.Run();
