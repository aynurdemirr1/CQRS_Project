using CQRS_Project.Context;
using CQRS_Project.CQRS.Queries.CarQueries;
using CQRS_Project.CQRS.Results.CarResults;
using Microsoft.EntityFrameworkCore;

namespace CQRS_Project.CQRS.Handlers.CarHandlers
{
    public class GetCarByIdQueryHandler
    {
        private readonly CqrsContext _context;

        public GetCarByIdQueryHandler(CqrsContext context)
        {
            _context = context;
        }

        public async Task<GetCarByIdQueryResult> Handle(GetCarByIdQuery query)
        {
            // Arabayı Brand ve Category bilgileriyle birlikte çekiyoruz.
            var values = await _context.Cars
                .Include(c => c.Brand)
                .Include(c => c.Category)
                .FirstOrDefaultAsync(c => c.CarId == query.CarId);

            // KRİTİK: Null Kontrolü (Araç bulunamazsa)
            if (values == null)
            {
                return new GetCarByIdQueryResult
                {
                    CarId = query.CarId,
                    Brand = "Bilinmiyor",
                    Model = "Bilinmiyor",
                    Category = "Bilinmiyor",
                    PricePerDay = 0,
                    IsAvailable = false,
                };
            }

            // Araç bulunduğunda:
            return new GetCarByIdQueryResult
            {
                CarId = values.CarId,
                Brand = values.Brand?.BrandName ?? "Marka Yok",
                Model = values.Model,
                Category = values.Category?.CategoryName ?? "Kategori Yok",
                PricePerDay = values.PricePerDay,
                ImageUrl = values.ImageUrl,
                IsAvailable = true,
                SeatCount = values.SeatCount,
                FuelType = values.FuelType,
                ModelYear = values.ModelYear,
                Transmission = values.Transmission,
                Stars = values.Stars,
            };
        }
    }
}