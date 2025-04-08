using System.Text.Json.Serialization;

namespace MonoIntegrationNew.Models
{
    public class MonoAccountData
    {
        public string _id { get; set; }
        public string Name { get; set; }
        public string AccountNumber { get; set; }
        public string Currency { get; set; }
        public decimal Balance { get; set; }
        public string Type { get; set; }
        public string Bvn { get; set; }
        public string AuthMethod { get; set; }
        public MonoInstitution Institution { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
    public class MonoInstitution
    {
        public string Name { get; set; }
        public string BankCode { get; set; }
        public string Type { get; set; }
    }

    public class MonoAccountDetailsResponse
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
        public MonoAccountDetailsData Data { get; set; }
    }
    public class MonoAccountDetailsData
    {
        public MonoAccountInfo Account { get; set; }
        public MonoMetaInfo Meta { get; set; }
    }
    public class MonoAccountInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Currency { get; set; }
        public string Type { get; set; }
        public string AccountNumber { get; set; }
        public decimal Balance { get; set; }
        public string Bvn { get; set; }
        public MonoInstitutionInfo Institution { get; set; }
    }

    public class MonoInstitutionInfo
    {
        public string Name { get; set; }
        public string BankCode { get; set; }
        public string Type { get; set; }
    }
    public class MonoMetaInfo
    {
        [JsonPropertyName("data_status")]
        public string DataStatus { get; set; }

        [JsonPropertyName("auth_method")]
        public string AuthMethod { get; set; }
    }

    public class MonoTransactionsResponse
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
        public List<MonoTransactionInfo> Data { get; set; }
        public MonoTransactionsPaginationInfo Meta { get; set; }
    }

    public class MonoTransactionInfo
    {
        public string Id { get; set; }
        public string Narration { get; set; }
        public decimal Amount { get; set; }
        public string Type { get; set; }
        public decimal Balance { get; set; }
        public DateTime Date { get; set; }
        public string Category { get; set; }
    }

    public class MonoTransactionsPaginationInfo
    {
        public int Total { get; set; }
        public int Page { get; set; }
        public string Previous { get; set; }
        public string Next { get; set; }
    }

    public class StatementMetadata
    {
        public int Count { get; set; }
    }

    public class StatementResponse
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
        public List<MonoTransactionInfo> Data { get; set; }
        public StatementMetadata Meta { get; set; }
    }
    public class StatementRequest
    {
       // public string AccountId { get; set; }
        public int Period { get; set; }
      //  public bool Realtime { get; set; } = false;
        public bool IsAccountLink { get; set; } = false;
      
        public string AccountNumber { get; set; }
    }
}
