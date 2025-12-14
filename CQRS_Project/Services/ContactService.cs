// ContactService.cs - SON GÜNCELLEME (Token 1024'e Yükseltildi)
using CQRS_Project.Services.Abstract;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace CQRS_Project.Services
{
    public class ContactService : IContactService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        private const string GEMINI_MODEL = "gemini-2.5-flash";
        private const string GEMINI_BASE_URL = "https://generativelanguage.googleapis.com/v1/models/";


        public ContactService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["Gemini:ApiKey"] ?? "";
        }

        public async Task<string> GenerateAutoReplyAsync(string message, string subject)
        {
            if (string.IsNullOrEmpty(_apiKey))
            {
                return "API anahtarı bulunamadı.";
            }

            int retryCount = 0;
            int maxRetries = 3;

            string requestUri = $"{GEMINI_BASE_URL}{GEMINI_MODEL}:generateContent?key={_apiKey}";

            while (retryCount < maxRetries)
            {
                try
                {
                    var prompt = $@"Sen profesyonel bir Türk müşteri hizmetleri temsilcisisin. 
Müşteri bilgileri:
Konu: {subject}
Mesaj: {message}

Görevlerin:
1. Müşterinin hangi dilde yazdığını tespit et (Türkçe/İngilizce/diğer)
2. Aynı dilde yanıt ver
3. Konusuna uygun, profesyonel ve yardımsever **4-5 cümlelik** bir cevap yaz.
4. Müşteri hizmetleri tonu kullan.

Çok önemlidir: Yanıtın **HİÇBİR ZAMAN KESİLMEMESİNİ** ve **TAMAMLANMASINI** garanti et.

SADECE YANIT METNİNİ (SELAMLAMA VE KAPANIŞ OLMADAN) ve başka hiçbir açıklama **yapmadan** döndür:";

                    var requestBody = new
                    {
                        contents = new[]
                        {
                            new
                            {
                                parts = new[]
                                {
                                    new { text = prompt }
                                }
                            }
                        },
                        generationConfig = new
                        {
                            temperature = 0.7,
                            topK = 40,
                            topP = 0.95,
                            // 💥 Token limitini 1024'e yükselttik
                            maxOutputTokens = 1024,
                            stopSequences = new string[] { }
                        },
                        safetySettings = new[]
                        {
                            new
                            {
                                category = "HARM_CATEGORY_HARASSMENT",
                                threshold = "BLOCK_MEDIUM_AND_ABOVE"
                            },
                            new
                            {
                                category = "HARM_CATEGORY_HATE_SPEECH",
                                threshold = "BLOCK_MEDIUM_AND_ABOVE"
                            }
                        }
                    };

                    var response = await _httpClient.PostAsJsonAsync(
                        requestUri,
                        requestBody
                    );

                    if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests || response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                    {
                        retryCount++;
                        if (retryCount >= maxRetries)
                        {
                            Console.WriteLine($"API yoğunluğu nedeniyle {maxRetries} deneme başarısız oldu.");
                            return "Sistem yoğunluğu nedeniyle şu anda yanıt veremiyoruz. Talebiniz alınmıştır, size manuel olarak geri dönüş yapılacaktır.";
                        }

                        var delayMs = (int)Math.Pow(2, retryCount) * 1000;
                        Console.WriteLine($"API yoğunluğu - {retryCount}. deneme, {delayMs / 1000}s bekleniyor...");
                        await Task.Delay(delayMs);
                        continue;
                    }

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"HTTP Error: {response.StatusCode}");
                        Console.WriteLine($"Response: {errorContent}");

                        return response.StatusCode switch
                        {
                            System.Net.HttpStatusCode.Unauthorized => "API anahtarı geçersiz. Lütfen Gemini API key'ini kontrol edin.",
                            System.Net.HttpStatusCode.BadRequest => $"Geçersiz istek: {errorContent}",
                            System.Net.HttpStatusCode.Forbidden => "API erişimi engellendi. API key'i kontrol edin.",
                            System.Net.HttpStatusCode.NotFound => "API adresi güncel değil veya model bulunamadı. (Hatalı URL veya model adı/Key)",
                            _ => $"Mesajınız için teşekkür ederiz. Konu: {subject} ile ilgili teknik bir sorun nedeniyle size manuel geri dönüş yapılacaktır."
                        };
                    }

                    var resultContent = await response.Content.ReadAsStringAsync();

                    Console.WriteLine($"[DEBUG] API RAW Content: {resultContent}");

                    try
                    {
                        var result = JsonSerializer.Deserialize<JsonElement>(resultContent);

                        if (result.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
                        {
                            var firstCandidate = candidates[0];
                            if (firstCandidate.TryGetProperty("content", out var content))
                            {
                                if (content.TryGetProperty("parts", out var parts) && parts.GetArrayLength() > 0)
                                {
                                    var firstPart = parts[0];
                                    if (firstPart.TryGetProperty("text", out var textElement))
                                    {
                                        var generatedText = textElement.GetString();
                                        if (!string.IsNullOrEmpty(generatedText))
                                        {
                                            Console.WriteLine($"Gemini AI Yanıtı (Ham): {generatedText}");
                                            return generatedText;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (JsonException jsonEx)
                    {
                        Console.WriteLine($"JSON parse hatası: {jsonEx.Message}");
                        Console.WriteLine($"Raw content: {resultContent}");
                    }

                    // Eğer AI yanıtı gelmezse (Fallback 1)
                    return $"Sayın müşterimiz, Mesajınız için teşekkür ederiz. Konu: {subject} ile ilgili talebiniz alınmıştır. Uzman ekibimiz detaylı inceleme sonrası size en kısa sürede geri dönüş yapacaktır. İyi günler dileriz.";
                }
                catch (HttpRequestException ex)
                {
                    retryCount++;
                    Console.WriteLine($"HTTP bağlantı hatası ({retryCount}/{maxRetries}): {ex.Message}");

                    if (retryCount >= maxRetries)
                    {
                        return "Bağlantı hatası oluştu. Lütfen internet bağlantınızı kontrol edip tekrar deneyin veya farklı bir zamanda tekrar iletişim kurun.";
                    }
                    await Task.Delay(2000 * retryCount);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Genel hata: {ex.Message}");
                    return $"Mesajınız için teşekkür ederiz. Teknik bir sorun nedeniyle size manuel geri dönüş yapılacaktır. Konu: {subject}";
                }
            }

            Console.WriteLine("Maksimum deneme sayısı aşıldı");
            return $"Mesajınız için teşekkür ederiz. Sistem yoğunluğu nedeniyle size manuel geri dönüş yapılacaktır. Konu: {subject}";
        }
    }
}