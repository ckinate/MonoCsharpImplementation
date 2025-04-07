using System.Text.Json.Serialization;

namespace MonoIntegrationNew.Models
{
    public class MonoAccountInitiateRequest
    {
        [JsonPropertyName("customer")]
        public Customer Customer { get; set; }

        [JsonPropertyName("scope")]
        public string Scope { get; set; } = "auth";

        [JsonPropertyName("meta")]
        public Meta Meta { get; set; }

        [JsonPropertyName("redirect_url")]
        public string RedirectUrl { get; set; }
        public bool IsAccountLink { get; set; }
    }

    public class Meta
    {
        [JsonPropertyName("ref")]
        public string Ref { get; set; }
    }

    public class Customer
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }
    }
}
