using CQRS_Project.Context;
using CQRS_Project.CQRS.Handlers.AboutHandlers;
using CQRS_Project.CQRS.Handlers.BrandHandlers;
using CQRS_Project.CQRS.Handlers.CarHandlers;
using CQRS_Project.CQRS.Handlers.CategoryHandlers;
using CQRS_Project.CQRS.Handlers.ContactHandlers;
using CQRS_Project.CQRS.Handlers.CustomerHandlers;
using CQRS_Project.CQRS.Handlers.EmployeeHandlers;
using CQRS_Project.CQRS.Handlers.LocationHandlers;
using CQRS_Project.CQRS.Handlers.ReservationHandlers;
using CQRS_Project.CQRS.Handlers.ReviewHandlers;
using CQRS_Project.CQRS.Handlers.SliderHandlers;
using CQRS_Project.Services;
using CQRS_Project.Services.Abstract;
using CQRS_Project.Settings; // Ayar Modelleri

var builder = WebApplication.CreateBuilder(args);

// ?? APPSETTINGS / SECRET.JSON AYAR SINIFLARINA BAÐLANTI
// Tüm API Key'lerinin doðru okunmasý için gerekli
builder.Services.Configure<RapidAPISettings>(builder.Configuration.GetSection("RapidAPI"));
builder.Services.Configure<GeminiSettings>(builder.Configuration.GetSection("Gemini"));
builder.Services.Configure<HuggingFaceSettings>(builder.Configuration.GetSection("HuggingFace")); // Hugging Face Eklendi
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Email"));

// Add services to the container.
builder.Services.AddDbContext<CqrsContext>();

// ??? HANDLER KAYITLARI (Dependency Injection) - DI HATALARI BURADA ÇÖZÜLÜYOR

// About Handlers
builder.Services.AddScoped<CreateAboutCommandHandler>();
builder.Services.AddScoped<GetAboutByIdQueryHandler>(); // ?? EKSÝK OLAN/HATA VEREN KAYIT DÜZELTÝLDÝ
builder.Services.AddScoped<GetAboutQueryHandler>();
builder.Services.AddScoped<RemoveAboutCommandHandler>();
builder.Services.AddScoped<UpdateAboutCommandHandler>();

// Brand Handlers
builder.Services.AddScoped<CreateBrandCommandHandler>();
builder.Services.AddScoped<GetBrandByIdQueryHandler>();
builder.Services.AddScoped<GetBrandQueryHandler>();
builder.Services.AddScoped<RemoveBrandCommandHandler>();
builder.Services.AddScoped<UpdateBrandCommandHandler>();

// Car Handlers
builder.Services.AddScoped<CreateCarCommandHandler>();
builder.Services.AddScoped<GetCarByIdQueryHandler>();
builder.Services.AddScoped<GetCarQueryHandler>();
builder.Services.AddScoped<RemoveCarCommandHandler>();
builder.Services.AddScoped<UpdateCarCommandHandler>();
builder.Services.AddScoped<GetTotalCarsQueryHandler>();
builder.Services.AddScoped<GetAvailableCarsQueryHandler>();

// Category Handlers
builder.Services.AddScoped<CreateCategoryCommandHandler>();
builder.Services.AddScoped<GetCategoryByIdQueryHandler>();
builder.Services.AddScoped<GetCategoryQueryHandler>();
builder.Services.AddScoped<RemoveCategoryCommandHandler>();
builder.Services.AddScoped<UpdateCategoryCommandHandler>();

// Contact Handlers
builder.Services.AddScoped<CreateContactCommandHandler>();
builder.Services.AddScoped<GetContactByIdQueryHandler>();
builder.Services.AddScoped<GetContactQueryHandler>();
builder.Services.AddScoped<RemoveContactCommandHandler>();
builder.Services.AddScoped<UpdateContactCommandHandler>();

// Customer Handlers
builder.Services.AddScoped<CreateCustomerCommandHandler>();
builder.Services.AddScoped<GetCustomerByIdQueryHandler>();
builder.Services.AddScoped<GetCustomerQueryHandler>();
builder.Services.AddScoped<RemoveCustomerCommandHandler>();
builder.Services.AddScoped<UpdateCustomerCommandHandler>();
builder.Services.AddScoped<GetTotalCustomersCountQueryHandler>();

// Employee Handlers
builder.Services.AddScoped<CreateEmployeeCommandHandler>();
builder.Services.AddScoped<GetEmployeeByIdQueryHandler>();
builder.Services.AddScoped<GetEmployeeQueryHandler>();
builder.Services.AddScoped<RemoveEmployeeCommandHandler>();
builder.Services.AddScoped<UpdateEmployeeCommandHandler>();

// Location Handlers
builder.Services.AddScoped<CreateLocationCommandHandler>();
builder.Services.AddScoped<GetLocationByIdQueryHandler>();
builder.Services.AddScoped<GetLocationQueryHandler>();
builder.Services.AddScoped<RemoveLocationCommandHandler>();
builder.Services.AddScoped<UpdateLocationCommandHandler>();
builder.Services.AddScoped<GetActiveLocationsCountQueryHandler>();

// Reservation Handlers
builder.Services.AddScoped<CreateReservationCommandHandler>();
builder.Services.AddScoped<GetReservationByIdQueryHandler>();
builder.Services.AddScoped<GetReservationQueryHandler>();
builder.Services.AddScoped<RemoveReservationCommandHandler>();
builder.Services.AddScoped<UpdateReservationCommandHandler>();
builder.Services.AddScoped<GetTotalReservationsQueryHandler>();
builder.Services.AddScoped<GetTotalReservationQueryHandler>(); // Tekrar eden isim olabilir, orijinal kodda vardý.

// Review Handlers
builder.Services.AddScoped<CreateReviewCommandHandler>();
builder.Services.AddScoped<GetReviewByIdQueryHandler>();
builder.Services.AddScoped<GetReviewQueryHandler>();
builder.Services.AddScoped<RemoveReviewCommandHandler>();
builder.Services.AddScoped<UpdateReviewCommandHandler>();

// Slider Handlers
builder.Services.AddScoped<CreateSliderCommandHandler>();
builder.Services.AddScoped<GetSliderByIdQueryHandler>();
builder.Services.AddScoped<GetSliderQueryHandler>();
builder.Services.AddScoped<RemoveSliderCommandHandler>();
builder.Services.AddScoped<UpdateSliderCommandHandler>();

// ?? SERVÝS KAYITLARI
builder.Services.AddHttpClient<LocationService>();
builder.Services.AddScoped<LocationService>();
builder.Services.AddScoped<IDistanceCalculationService, DistanceCalculationService>();
builder.Services.AddScoped<IFuelPriceService, FuelPriceService>(); // FuelPriceService'in API ile çalýþmasý gerekiyor.

// YENÝ SERVÝSLER (AI + Email)
builder.Services.AddScoped<SearchLocationQueryHandler>();
builder.Services.AddScoped<SyncTurkishCitiesCommandHandler>();
builder.Services.AddScoped<AddLocationFromApiCommandHandler>();
builder.Services.AddScoped<ICarRecommendationService, CarRecommendationService>();
builder.Services.AddHttpClient<IContactService, ContactService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// Logging
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Bu satýr genellikle eksik olmaz ama kontrol etmekte fayda var.

app.UseRouting();
app.UseAuthorization();

app.MapStaticAssets(); // Özel static asset yönlendirmesi

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Default}/{action=Index}/{id?}")
    .WithStaticAssets();

// TEST AI ENDPOINT
app.MapGet("/test-ai", async (IContactService contactService) =>
{
    // Bu endpoint, Gemini/Hugging Face servisinin çalýþýp çalýþmadýðýný test eder.
    var reply = await contactService.GenerateAutoReplyAsync(
        "Merhaba, sipariþim ne zaman gelir?",
        "Sipariþ Hakkýnda"
    );
    return reply;
});

app.Run();