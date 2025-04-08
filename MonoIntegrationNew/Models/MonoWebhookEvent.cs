using System.Text.Json.Serialization;

namespace MonoIntegrationNew.Models
{
    public class MonoWebhookEvent
    {
        public string Event { get; set; }
        public MonoWebhookData Data { get; set; }
    }
    public class MonoWebhookData
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("customer")]
        public string Customer { get; set; }
        public MonoMeta Meta { get; set; }
        public MonoAccountData Account { get; set; }
    }
}
