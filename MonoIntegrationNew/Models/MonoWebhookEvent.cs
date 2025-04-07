namespace MonoIntegrationNew.Models
{
    public class MonoWebhookEvent
    {
        public string Event { get; set; }
        public MonoWebhookData Data { get; set; }
    }
    public class MonoWebhookData
    {
        public string Id { get; set; }
        public MonoMeta Meta { get; set; }
        public MonoAccountData Account { get; set; }
    }
}
