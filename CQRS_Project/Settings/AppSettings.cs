namespace CQRS_Project.Settings
{
    public class AppSettings
    {
        public RapidAPISettings RapidAPI { get; set; }
        public HuggingFaceSettings HuggingFace { get; set; }
        public GeminiSettings Gemini { get; set; }
        public OpenAISettings OpenAI { get; set; }
        public EmailSettings Email { get; set; }
    }

    public class RapidAPISettings
    {
        public string Key { get; set; }
    }

    public class HuggingFaceSettings
    {
        public string ApiToken { get; set; }
    }

    public class GeminiSettings
    {
        public string ApiKey { get; set; }
    }

    public class OpenAISettings
    {
        public string ApiKey { get; set; }
    }

    public class EmailSettings
    {
        public string SenderPassword { get; set; }
    }
}
