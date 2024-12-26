using MACS.Models;
using MACS.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net.Http.Headers;

namespace MACS.Controllers
{
    public class HomeController : Controller
    {

        private readonly QRCodeService _qrCodeService;

        public HomeController(QRCodeService qrCodeService)
        {
            _qrCodeService = qrCodeService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var model = new VehicleLog();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ScanQR(IFormFile qrImage)
        {
            if (qrImage == null || qrImage.Length == 0)
            {
                ViewBag.Message = "No image uploaded. Please capture an image.";
                return View("Index");
            }

            try
            {
                var result = await _qrCodeService.ScanQRCodeAsync(qrImage);

                ViewBag.Message = result.Message ?? "QR Scan Successful!";
                ViewBag.Result = result;

                return View("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Error: {ex.Message}";
                return View("Index");
            }
        }



        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
