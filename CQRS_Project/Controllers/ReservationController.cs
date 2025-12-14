using CQRS_Project.Context;
using CQRS_Project.CQRS.Commands.ReservationCommands;
using CQRS_Project.CQRS.Handlers.CarHandlers;
using CQRS_Project.CQRS.Handlers.ReservationHandlers;
using CQRS_Project.CQRS.Queries.CarQueries;
using CQRS_Project.CQRS.Queries.ReservationQueries;
using CQRS_Project.Services.Abstract;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CQRS_Project.Controllers
{
    public class ReservationController : Controller
    {
        private readonly CqrsContext _context;
        private readonly GetReservationByIdQueryHandler _getReservationByIdQueryHandler;
        private readonly GetReservationQueryHandler _getReservationQueryHandler;
        private readonly CreateReservationCommandHandler _createReservationCommandHandler;
        private readonly UpdateReservationCommandHandler _updateReservationCommandHandler;
        private readonly RemoveReservationCommandHandler _removeReservationCommandHandler;
        private readonly GetAvailableCarsQueryHandler _getAvailableCarsHandler;
        private readonly IFuelPriceService _fuelPriceService;

        public ReservationController(
            GetReservationByIdQueryHandler getReservationByIdQueryHandler,
            GetReservationQueryHandler getReservationQueryHandler,
            CreateReservationCommandHandler createReservationCommandHandler,
            UpdateReservationCommandHandler updateReservationCommandHandler,
            RemoveReservationCommandHandler removeReservationCommandHandler,
            GetAvailableCarsQueryHandler getAvailableCarsHandler,
            CqrsContext context,
            IFuelPriceService fuelPriceService)
        {
            _getReservationByIdQueryHandler = getReservationByIdQueryHandler;
            _getReservationQueryHandler = getReservationQueryHandler;
            _createReservationCommandHandler = createReservationCommandHandler;
            _updateReservationCommandHandler = updateReservationCommandHandler;
            _removeReservationCommandHandler = removeReservationCommandHandler;
            _getAvailableCarsHandler = getAvailableCarsHandler;
            _context = context;
            _fuelPriceService = fuelPriceService;
        }

        public async Task<IActionResult> Index()
        {
            var values = await _getReservationQueryHandler.Handle();
            return View(values);
        }

        [HttpGet]
        public IActionResult CreateReservation()
        {
            ViewBag.Customers = new SelectList(_context.Customers, "CustomerId", "FullName");
            ViewBag.Cars = new SelectList(_context.Cars, "CarId", "Model");
            ViewBag.Locations = new SelectList(_context.Locations, "LocationId", "City");
            return View();
        }

        // Bu metot, formdan direkt post edildiğinde kullanılır
        [HttpPost]
        public async Task<IActionResult> CreateReservation(CreateReservationCommand command)
        {
            try
            {
                // Güvenlik Önlemi: CustomerId ve Status atamaları
                if (command.CustomerId == 0) command.CustomerId = 1;
                if (string.IsNullOrEmpty(command.Status)) command.Status = "Pending";

                if (!ModelState.IsValid)
                {
                    TempData["ErrorMessage"] = "Formda eksik veya hatalı bilgi var. Lütfen kontrol edin.";
                    return RedirectToAction("BookNow");
                }

                await _createReservationCommandHandler.Handle(command, CancellationToken.None);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Kritik Rezervasyon Oluşturma Hatası (Form Post): {ex.Message}");
                TempData["ErrorMessage"] = "REZERVASYON HATASI: " + ex.Message.Split('\n')[0];
                return RedirectToAction("BookNow");
            }
        }

        // KRİTİK DÜZELTME: Frontend'den gelen AJAX isteği için bu metot eklendi (405 hatası çözümü)
        [HttpPost]
        public async Task<JsonResult> CreateReservationFromFrontend([FromBody] CreateReservationCommand command)
        {
            try
            {
                // Güvenlik Önlemi: CustomerId ve Status atamaları
                if (command.CustomerId == 0) command.CustomerId = 1;
                if (string.IsNullOrEmpty(command.Status)) command.Status = "Pending";

                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Form verisi eksik veya geçersiz." });
                }

                await _createReservationCommandHandler.Handle(command, CancellationToken.None);

                // Başarılı olursa, JSON yanıtı döndürün
                return Json(new { success = true, message = "Rezervasyon başarıyla kaydedildi." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Frontend Rezervasyon Hatası: {ex.Message}");
                // Hata durumunda success: false döndürün
                return Json(new { success = false, message = "Kayıt sırasında hata oluştu: " + ex.Message });
            }
        }


        public async Task<IActionResult> DeleteReservation(int id)
        {
            await _removeReservationCommandHandler.Handle(new RemoveReservationCommand(id));
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> UpdateReservation(int id)
        {
            var value = await _getReservationByIdQueryHandler.Handle(new GetReservationByIdQuery(id));
            ViewBag.Customers = new SelectList(_context.Customers, "CustomerId", "FullName");
            ViewBag.Cars = new SelectList(_context.Cars, "CarId", "Model");
            ViewBag.Locations = new SelectList(_context.Locations, "LocationId", "City");
            return View(value);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateReservation(UpdateReservationCommand command)
        {
            await _updateReservationCommandHandler.Handle(command);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> BookNow()
        {
            ViewBag.Locations = new SelectList(
                await _context.Locations.Where(l => l.IsActive)
                    .Select(l => new { l.LocationId, Text = l.City + " - " + l.District })
                    .ToListAsync(),
                "LocationId", "Text");

            ViewBag.Categories = new SelectList(
                await _context.Categories.ToListAsync(),
                "CategoryId", "CategoryName");

            if (TempData["ErrorMessage"] != null)
            {
                ViewBag.ErrorMessage = TempData["ErrorMessage"];
            }

            return View();
        }

        [HttpPost]
        public async Task<JsonResult> GetAvailableCars([FromBody] GetAvailableCarsQuery query)
        {
            try
            {
                // KRİTİK KONTROL: Tarihlerin geçerli olduğundan emin olun
                if (query.StartDate == default || query.EndDate == default || query.StartDate >= query.EndDate)
                {
                    return Json(new { success = false, message = "Lütfen geçerli bir başlangıç ve bitiş tarihi seçiniz." });
                }

                var availableCars = await _getAvailableCarsHandler.Handle(query);
                var totalDays = (query.EndDate - query.StartDate).Days;
                if (totalDays <= 0) totalDays = 1;

                var result = availableCars.Select(c => new
                {
                    carId = c.CarId,
                    brand = c.Brand,
                    model = c.Model,
                    category = c.Category,
                    pricePerDay = c.PricePerDay,
                    totalPrice = c.PricePerDay * totalDays,
                    totalDays = totalDays,
                    imageUrl = c.ImageUrl ?? "/images/default-car.jpg",
                    year = c.ModelYear,
                    fuelType = c.FuelType,
                    transmission = c.Transmission,
                    seatCount = c.SeatCount
                }).ToList();

                return Json(new
                {
                    success = true,
                    data = result
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Araç Listeleme Hatası: {ex.Message}");
                return Json(new
                {
                    success = false,
                    message = $"Araç listelenirken bir sunucu hatası oluştu: {ex.Message}"
                });
            }
        }

        // KRİTİK DÜZELTME: Yakıt hesaplaması için bu metot eklendi (405 hatası çözümü)
        [HttpPost]
        public async Task<JsonResult> CalculateFuelCost([FromBody] FuelCalculationRequest request)
        {
            try
            {
                // Lokasyon ve araç verilerini çekin
                var car = await _context.Cars.FirstOrDefaultAsync(c => c.CarId == request.CarId);
                var pickUp = await _context.Locations.FindAsync(request.PickUpLocationId);
                var dropOff = await _context.Locations.FindAsync(request.DropOffLocationId);

                if (car == null || pickUp == null || dropOff == null)
                {
                    return Json(new { success = false, message = "Araç veya lokasyon bilgileri eksik." });
                }

                // Hesaplamalar (Details metodundan kopyalandı)
                double pickUpLatVal = pickUp.Latitude == 0 ? 41.0082 : pickUp.Latitude;
                double pickUpLonVal = pickUp.Longitude == 0 ? 28.9784 : pickUp.Longitude;
                double dropOffLatVal = dropOff.Latitude == 0 ? 39.9334 : dropOff.Latitude;
                double dropOffLonVal = dropOff.Longitude == 0 ? 32.8597 : dropOff.Longitude;

                double distanceKm = GetDistance(pickUpLatVal, pickUpLonVal, dropOffLatVal, dropOffLonVal);
                if (distanceKm < 5) distanceKm = 100;

                decimal fuelPricePerLiter = 40.00M; // Sabit yakıt fiyatı

                // Null kontrolü eklendi
                double fuelConsumptionPer100Km = (car.FuelType != null && car.FuelType.ToLower().Contains("elektrik")) ? 15.0 : 7.0;
                double estimatedFuelLiters = (distanceKm / 100) * fuelConsumptionPer100Km;
                decimal estimatedFuelCost = Math.Round((decimal)estimatedFuelLiters * fuelPricePerLiter, 2);

                return Json(new
                {
                    success = true,
                    distanceKm = distanceKm,
                    estimatedFuelCost = estimatedFuelCost,
                    estimatedFuelLiters = estimatedFuelLiters,
                    fuelType = car.FuelType // Frontend'de görüntülemek için ekledik
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fuel Cost Calculation Error: {ex.Message}");
                return Json(new { success = false, message = "Yakıt hesaplama hatası: " + ex.Message });
            }
        }

        // --- DETAILS METODU (Daha önceki versiyonundan kopyalandı) ---
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var reservation = await _getReservationByIdQueryHandler.Handle(new GetReservationByIdQuery(id));

            if (reservation == null)
                return NotFound("Rezervasyon bulunamadı.");

            var car = await _context.Cars
                .Include(c => c.Brand)
                .FirstOrDefaultAsync(c => c.CarId == reservation.CarId);

            if (car == null)
                return NotFound("Rezervasyon ile ilişkili araç bulunamadı.");

            var pickUp = await _context.Locations.FindAsync(reservation.PickUpLocationId);
            var dropOff = await _context.Locations.FindAsync(reservation.DropOffLocationId);

            if (pickUp == null || dropOff == null)
                return BadRequest("Rezervasyon ile ilişkili alış/teslim lokasyonu bulunamadı.");

            // Hesaplamalar
            double pickUpLatVal = pickUp.Latitude == 0 ? 41.0082 : pickUp.Latitude;
            double pickUpLonVal = pickUp.Longitude == 0 ? 28.9784 : pickUp.Longitude;
            double dropOffLatVal = dropOff.Latitude == 0 ? 39.9334 : dropOff.Latitude;
            double dropOffLonVal = dropOff.Longitude == 0 ? 32.8597 : dropOff.Longitude;

            double distanceKm = GetDistance(pickUpLatVal, pickUpLonVal, dropOffLatVal, dropOffLonVal);
            if (distanceKm < 5) distanceKm = 100;

            decimal fuelPricePerLiter = 40.00M; // Sabit yakıt fiyatı

            double fuelConsumptionPer100Km = car.FuelType != null && car.FuelType.ToLower().Contains("elektrik") ? 15.0 : 7.0;
            double estimatedFuelLiters = (distanceKm / 100) * fuelConsumptionPer100Km;
            decimal estimatedFuelCost = Math.Round((decimal)estimatedFuelLiters * fuelPricePerLiter, 2);

            int totalDays = (reservation.EndDate - reservation.StartDate).Days;
            if (totalDays <= 0) totalDays = 1;
            decimal totalPrice = car.PricePerDay * totalDays;


            // Verileri ViewBag'e Atama 
            ViewBag.ReservationId = reservation.ReservationId;
            ViewBag.CarId = car.CarId;
            ViewBag.Brand = car.Brand?.BrandName;
            ViewBag.Model = car.Model;
            ViewBag.FuelType = car.FuelType;
            ViewBag.PricePerDay = car.PricePerDay;
            ViewBag.TotalDays = totalDays;
            ViewBag.TotalPrice = totalPrice;

            ViewBag.StartDate = reservation.StartDate;
            ViewBag.EndDate = reservation.EndDate;
            ViewBag.StartTime = reservation.StartDate.ToString("HH:mm");
            ViewBag.EndTime = reservation.EndDate.ToString("HH:mm");

            ViewBag.PickUpText = $"{pickUp.City} - {pickUp.District}";
            ViewBag.DropOffText = $"{dropOff.City} - {dropOff.District}";
            ViewBag.PickUpLocationId = reservation.PickUpLocationId;
            ViewBag.DropOffLocationId = reservation.DropOffLocationId;

            ViewBag.DistanceKm = distanceKm;
            ViewBag.EstimatedFuelLiters = estimatedFuelLiters;
            ViewBag.EstimatedFuelCost = estimatedFuelCost;
            ViewBag.PickUpLat = pickUpLatVal;
            ViewBag.PickUpLon = pickUpLonVal;
            ViewBag.DropOffLat = dropOffLatVal;
            ViewBag.DropOffLon = dropOffLonVal;

            return View();
        }
        // --- DETAILS METODU SONU ---


        private double GetDistance(double lat1, double lon1, double lat2, double lon2)
        {
            if (lat1 == 0 || lon1 == 0 || lat2 == 0 || lon2 == 0) return 0;
            double R = 6371;
            double dLat = (lat2 - lat1) * (Math.PI / 180);
            double dLon = (lon2 - lon1) * (Math.PI / 180);
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(lat1 * (Math.PI / 180)) * Math.Cos(lat2 * (Math.PI / 180)) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        public class FuelCalculationRequest
        {
            public int PickUpLocationId { get; set; }
            public int DropOffLocationId { get; set; }
            public int CarId { get; set; }
        }
    }
}