using MACS.Models;
using MACS.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MACS.Data;
using MACS.Controllers;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();
builder.Services.AddHttpClient<UploadHistoryService>();
builder.Services.AddHttpClient<QRCodeService>();
builder.Services.AddHttpClient<AuthService>();
builder.Services.AddHttpClient<HistoryCarService>();
builder.Services.AddHttpClient<HomeController>();

builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiBaseUrl"));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()  
              .AllowAnyMethod()  
              .AllowAnyHeader(); 
    });
});
//session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); 
    options.Cookie.HttpOnly = true;                
    options.Cookie.IsEssential = true;            
});

var app = builder.Build();

//  Middleware
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
