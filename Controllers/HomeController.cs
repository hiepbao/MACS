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

                if (result != null && result.Data != null && result.Data is List<string> dataList)
                {
                    // Convert data list to a dictionary
                    var dataDict = dataList
                        .Select(item => item.Split(":"))
                        .Where(parts => parts.Length > 1)
                        .ToDictionary(parts => parts[0].Trim().ToLower(), parts => parts[1].Trim());

                    if (dataDict.ContainsKey("message"))
                    {
                        var message = dataDict["message"].ToLower();

                        if (message == "xuất kho")
                        {
                            TempData["Result"] = Newtonsoft.Json.JsonConvert.SerializeObject(result);
                            return RedirectToAction("Warehouse");
                        }
                        else if (message == "xe ra")
                        {
                            TempData["Result"] = Newtonsoft.Json.JsonConvert.SerializeObject(result); 
                            return RedirectToAction("CheckOut");
                        }
                        else
                        {
                            ViewBag.Message = "Lỗi: QR không hợp lệ.";
                            return View("Index");
                        }
                    }
                    else
                    {
                        ViewBag.Message = "Lỗi: Không tìm thấy trường 'message' trong QR.";
                        return View("Index");
                    }
                }
                else
                {
                    ViewBag.Message = "Lỗi: Không thể đọc dữ liệu từ QR.";
                    return View("Index");
                }
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Đã xảy ra lỗi trong quá trình quét QR: " + ex.Message;
                return View("Index");
            }


        }


        [HttpGet]
        public IActionResult WareHouse()
        {
            if (TempData["Result"] != null)
            {
                var jsonString = TempData["Result"] as string;
                if (!string.IsNullOrEmpty(jsonString))
                {
                    try
                    {
                        // Deserialize JSON thành QrCodeResponse
                        var resultObject = Newtonsoft.Json.JsonConvert.DeserializeObject<QrCodeResponse>(jsonString);
                        return View(resultObject); // Truyền đúng model vào view
                    }
                    catch (Exception ex)
                    {
                        // Log lỗi nếu cần
                        TempData["Error"] = "Lỗi khi xử lý JSON: " + ex.Message;
                    }
                }
            }

            // Truyền một model mặc định nếu không có dữ liệu
            return View(new QrCodeResponse());
        }

        [HttpGet]
        public IActionResult CheckOut()
        {
            if (TempData["Result"] != null)
            {
                var jsonString = TempData["Result"] as string;
                if (!string.IsNullOrEmpty(jsonString))
                {
                    try
                    {
                        // Deserialize JSON thành QrCodeResponse
                        var resultObject = Newtonsoft.Json.JsonConvert.DeserializeObject<QrCodeResponse>(jsonString);
                        return View(resultObject); // Truyền đúng model vào view
                    }
                    catch (Exception ex)
                    {
                        // Log lỗi nếu cần
                        TempData["Error"] = "Lỗi khi xử lý JSON: " + ex.Message;
                    }
                }
            }

            // Truyền một model mặc định nếu không có dữ liệu
            return View(new QrCodeResponse());
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
