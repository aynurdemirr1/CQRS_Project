using CQRS_Project.Context;
using CQRS_Project.Services.Abstract;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace CQRS_Project.Controllers
{
    [Route("Admin")]
    public class AdminController : Controller
    {
        private readonly ICarRecommendationService _carRecommendationService;
        private readonly IFuelPriceService _fuelPriceService;
        private readonly CqrsContext _context;

        public AdminController(
            ICarRecommendationService carRecommendationService,
            IFuelPriceService fuelPriceService,
            CqrsContext context)
        {
            _carRecommendationService = carRecommendationService;
            _fuelPriceService = fuelPriceService;
            _context = context;
        }

        [HttpGet("Dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            ViewBag.TotalCars = await _context.Cars.CountAsync();
            ViewBag.AvailableCars = await _context.Cars.Where(c => c.IsAvailable).CountAsync();
            ViewBag.TotalReservations = await _context.Reservations.CountAsync();
            ViewBag.PendingReservations = await _context.Reservations.Where(r => r.Status == "Pending").CountAsync();

            return View();
        }

        [HttpGet("CarRecommendation")]
        public IActionResult CarRecommendation()
        {
            return View();
        }

        [HttpPost("GetCarRecommendation")]
        public async Task<JsonResult> GetCarRecommendation([FromBody] CarRecommendationRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Query))
                {
                    return Json(new { success = false, message = "Lütfen bir soru yazınız." });
                }

                var availableCars = await _context.Cars
                    .Include(c => c.Category)
                    .Include(c => c.Brand)
                    .Where(c => c.IsAvailable)
                    .ToListAsync();

                if (!availableCars.Any())
                {
                    return Json(new
                    {
                        success = true,
                        recommendation = "<div class='alert alert-warning'>Şu anda müsait araç bulunmamaktadır.</div>"
                    });
                }

                var recommendation = await _carRecommendationService.GetCarRecommendationAsync(request.Query, availableCars);

                return Json(new { success = true, recommendation });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Bir hata oluştu: {ex.Message}"
                });
            }
        }

        // *** GÜNCELLEME: API Hatalarında (Kota Dolması) Yedek Veri Gönderme ***
        [HttpGet("GetFuelPrices")]
        public async Task<JsonResult> GetFuelPrices()
        {
            try
            {
                var fuelPrices = await _fuelPriceService.GetTurkeyFuelPricesAsync();

                // API'den veri başarıyla geldiyse, gerçek veriyi döndür.
                return Json(new
                {
                    success = true,
                    data = new
                    {
                        // Fiyatları nokta ayırıcı ile formatlıyoruz (JavaScript'in kolay okuması için)
                        petrol = fuelPrices.Benzin.ToString("0.00", CultureInfo.InvariantCulture),
                        diesel = fuelPrices.Motorin.ToString("0.00", CultureInfo.InvariantCulture),
                        lpg = fuelPrices.Lpg.ToString("0.00", CultureInfo.InvariantCulture),
                        lastUpdate = fuelPrices.LastUpdate
                    }
                });
            }
            catch (Exception)
            {
                
                return Json(new
                {
                    success = true, // JavaScript'e başarılı bir veri akışı olduğunu bildiriyoruz
                    data = new
                    {
                        petrol = "53.00",  
                        diesel = "54.00", 
                        lpg = "24.00",    
                        lastUpdate = DateTime.Now.ToString("dd.MM.yyyy HH:mm") + " (Yedek)" 
                    }
                });
            }
        }
    }

    public class CarRecommendationRequest
    {
        public string Query { get; set; } = string.Empty;
    }
}