using System.Text.Json.Serialization;

namespace MonoIntegrationNew.Models
{
    public class MonoAccountLinkingResponse
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
        public MonoAccountLinkingData Data { get; set; }
    }
    public class MonoAccountLinkingData
    {
        [JsonPropertyName("mono_url")]
        public string MonoUrl { get; set; }
        public string Customer { get; set; }
        public MonoMeta Meta { get; set; }
        public string Scope { get; set; }

        [JsonPropertyName("redirect_url")]
        public string RedirectUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class MonoMeta
    {
        public string Ref { get; set; }
        public string DataStatus { get; set; }
        public string AuthMethod { get; set; }
    }
}
