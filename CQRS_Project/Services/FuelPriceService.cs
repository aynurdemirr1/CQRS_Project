using CQRS_Project.Models;
using CQRS_Project.Services.Abstract;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace CQRS_Project.Services
{
    public class FuelPriceService : IFuelPriceService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _cache;

        public FuelPriceService(HttpClient httpClient, IConfiguration configuration, IMemoryCache cache)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _cache = cache;
        }

        public async Task<FuelPriceResponse> GetTurkeyFuelPricesAsync()
        {
            // Önce cache kontrolü
            if (_cache.TryGetValue("TurkeyFuelPrices", out FuelPriceResponse cached))
            {
                Console.WriteLine("FuelPrice cache'den alındı.");
                return cached;
            }

            try
            {
                var apiKey = _configuration["RapidAPI:Key"];

                // İstek oluşturuluyor: Tüm Avrupa ülkeleri listesini çeker
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    // Bu endpoint, bir dizi (Array) içinde ülkeleri döndürür.
                    RequestUri = new Uri("https://gas-price.p.rapidapi.com/europeanCountries"),
                    Headers =
                    {
                        { "x-rapidapi-key", apiKey },
                        { "x-rapidapi-host", "gas-price.p.rapidapi.com" }
                    }
                };

                using var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var body = await response.Content.ReadAsStringAsync();
                var json = JObject.Parse(body);

                // *** KRİTİK DÜZELTME: JSON Dizisinden Türkiye nesnesini bulma ***
                // "result" bir dizi (JArray) olduğu için, önce diziyi alıp sonra içinden "Turkey" nesnesini bulmalıyız.
                var resultArray = json["result"]?.ToArray();
                var turkey = resultArray?.FirstOrDefault(x => x["country"]?.ToString() == "Turkey");

                if (turkey == null)
                    throw new Exception("API yanıtında Türkiye verisi bulunamadı veya JSON formatı değişti.");

                // Bu API Euro fiyatı döndürüyorsa kur çevrimi gereklidir.
                // Eğer API direkt TL döndürüyorsa aşağıdaki GetEuroToTLRate çağrısını kaldırıp, ParseFuel metodunu basitleştirmelisiniz.
                decimal euroToTL = await GetEuroToTLRate();

                decimal ParseFuel(string? value)
                {
                    if (string.IsNullOrEmpty(value) || value == "-" || value == "0,000")
                        return 0;

                    // Gelen Euro değerini (nokta/virgül ayrımı varsa) düzeltip TL'ye çevirir.
                    value = value.Replace(",", ".");
                    decimal eur = decimal.Parse(value, CultureInfo.InvariantCulture);

                    return Math.Round(eur * euroToTL, 2);
                }

                var priceResponse = new FuelPriceResponse
                {
                    // Fiyatlar, bulunan 'turkey' nesnesinin altındaki anahtarlardan alınıyor
                    Benzin = ParseFuel(turkey["gasoline"]?.ToString()),
                    Motorin = ParseFuel(turkey["diesel"]?.ToString()),
                    Lpg = ParseFuel(turkey["lpg"]?.ToString()),
                    LastUpdate = DateTime.Now.ToString("dd.MM.yyyy HH:mm")
                };

                // Cache'e 30 dakikalığına kaydet
                _cache.Set("TurkeyFuelPrices", priceResponse, TimeSpan.FromMinutes(30));

                Console.WriteLine("FuelPrice API'den başarıyla alındı.");
                return priceResponse;
            }
            catch (Exception ex)
            {
                // *** KRİTİK DÜZELTME: Sabit fiyatları döndüren (fallback) kısmı kaldırdık ***
                Console.WriteLine($"GetTurkeyFuelPricesAsync API hatası: {ex.Message}");
                // Hatayı olduğu gibi fırlatırız ki, sorunun kaynağını (403 Forbidden gibi) görelim.
                throw;
            }
        }

        // --- Kur Çekme Metodu ---

        private async Task<decimal> GetEuroToTLRate()
        {
            if (_cache.TryGetValue("EuroToTRY", out decimal cachedRate))
                return cachedRate;

            try
            {
                var apiKey = _configuration["RapidAPI:Key"];

                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri("https://exchange-rates7.p.rapidapi.com/convert?base=EUR&target=TRY"),
                    Headers =
                    {
                        { "x-rapidapi-key", apiKey },
                        { "x-rapidapi-host", "exchange-rates7.p.rapidapi.com" }
                    }
                };

                using var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var body = await response.Content.ReadAsStringAsync();
                var json = JObject.Parse(body);

                decimal rate = json["result"]?.Value<decimal>() ??
                               json["conversion_result"]?.Value<decimal>() ??
                               json["rate"]?.Value<decimal>() ?? 0;

                if (rate <= 0)
                    throw new Exception("Kur alınamadı");

                _cache.Set("EuroToTRY", rate, TimeSpan.FromMinutes(60));

                Console.WriteLine($"EUR/TRY kuru: {rate}");
                return rate;
            }
            catch
            {
                // Kur API'si nadiren kullanıldığı için sabit kur değeri fallback olarak kalabilir.
                return 48.5m;
            }
        }

        public async Task<FuelPriceResponse> GetCurrentFuelPriceAsync()
        {
            return await GetTurkeyFuelPricesAsync();
        }

        public Task<decimal> GetFuelPriceAsync(string fuelType)
        {
            throw new NotImplementedException();
        }
    }
}