using CQRS_Project.CQRS.Queries.CarQueries;
using CQRS_Project.CQRS.Handlers.CarHandlers;
using CQRS_Project.Services.Abstract;
using Microsoft.AspNetCore.Mvc;

namespace CQRS_Project.ViewComponents
{
    public class ReservationSummaryViewComponent : ViewComponent
    {
        // View Component'in kullanmasına gerek kalmadığı için bu servisi yoruma alabiliriz
        private readonly IFuelPriceService _fuelPriceService;
        private readonly GetCarByIdQueryHandler _carByIdQueryHandler;

        public ReservationSummaryViewComponent(IFuelPriceService fuelPriceService, GetCarByIdQueryHandler carByIdQueryHandler)
        {
            _fuelPriceService = fuelPriceService; // Inject edildi ama kullanılmayacak
            _carByIdQueryHandler = carByIdQueryHandler;
        }

        public async Task<IViewComponentResult> InvokeAsync(
            int carId,
            string brand,
            string model,
            decimal pricePerDay,
            decimal totalPrice,
            int totalDays,
            int pickUpLocationId,
            int dropOffLocationId,
            DateTime startDate,
            DateTime endDate,
            string pickUpText,
            string dropOffText,
            string startTime,
            string endTime,
            double pickUpLat,
            double pickUpLon,
            double dropOffLat,
            double dropOffLon,
            // Controller'da hesaplanan ve View Component'e parametre olarak gelen değerler:
            double distanceKm,
            double estimatedFuelLiters,
            decimal estimatedFuelCost)
        {
            // *** DİKKAT: Artık Controller'dan gelen parametreleri kullandığımız için
            // bu View Component'in en başındaki statik/varsayılan değer atama bloğunu SİLİP/YORUMA ALIYORUZ.

            // Hata veren harici API çağrısını devre dışı bırakıyoruz (429 Çözümü).
            // var fuelPrices = await _fuelPriceService.GetTurkeyFuelPricesAsync();

            // Eğer View'da FuelPrices listesine ihtiyaç varsa, sabit bir liste gönderilir:
            var staticFuelPrices = new List<object>
            {
                new { fuel_type = "Benzin", price = 42.50M },
                new { fuel_type = "Dizel", price = 44.00M },
                new { fuel_type = "LPG", price = 22.00M }
            };


            // Gerekli servis çağrısı
            var carModel = await _carByIdQueryHandler.Handle(new GetCarByIdQuery(carId));

            if (carModel == null)
            {
                return Content("Araç bilgisi bulunamadı veya CarId geçersiz.");
            }

            // Tüm parametreler Controller'dan geldiği için doğrudan View'a gönderilir.
            ViewBag.CarId = carId;
            ViewBag.Brand = brand;
            ViewBag.Model = model;
            ViewBag.PricePerDay = pricePerDay;
            ViewBag.TotalPrice = totalPrice;
            ViewBag.TotalDays = totalDays;
            ViewBag.PickUpText = pickUpText;
            ViewBag.DropOffText = dropOffText;
            ViewBag.StartDate = startDate; // DateTime
            ViewBag.EndDate = endDate;     // DateTime
            ViewBag.StartTime = startTime;
            ViewBag.EndTime = endTime;

            // Hesaplanan değerler
            ViewBag.DistanceKm = distanceKm;
            ViewBag.EstimatedFuelLiters = estimatedFuelLiters;
            ViewBag.EstimatedFuelCost = estimatedFuelCost;

            ViewBag.CarInfo = carModel;
            ViewBag.FuelPrices = staticFuelPrices;

            return View();
        }
    }
}