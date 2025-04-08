namespace MonoIntegrationNew.Data
{
    public class MonoLinkingRequest
    {
        public int Id { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string? Reference { get; set; }
        public string? MonoCustomerId { get; set; }
        public string? MonoAccountId { get; set; }
        public string MonoUrl { get; set; }
        public string Status { get; set; } // INITIATED, CONNECTED, AVAILABLE, FAILED
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation property
        public MonoAccount? Account { get; set; }
    }

    public class MonoAccount
    {
        public int Id { get; set; }
        public string? MonoAccountId { get; set; }
        public string? AccountName { get; set; }
        public string? AccountNumber { get; set; }
        public string? BankName { get; set; }
        public string? BankCode { get; set; }
        public string? AccountType { get; set; }
        public string? Currency { get; set; }
        public decimal Balance { get; set; }
        public string? DataStatus { get; set; } // AVAILABLE, PROCESSING, FAILED
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation property
        public int? LinkingRequestId { get; set; }
        public MonoLinkingRequest LinkingRequest { get; set; }

        // Added newly
        public ICollection<MonoTransaction>? Transactions { get; set; }
    }

    public class MonoTransaction
    {
        public int Id { get; set; }
        public string? MonoAccountId { get; set; }
        public string? TransactionId { get; set; }
        public string? Narration { get; set; }
        public decimal Amount { get; set; }
        public string? Type { get; set; } // debit or credit
        public decimal Balance { get; set; }
        public DateTime Date { get; set; }
        public string? Category { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation property
        public int AccountId { get; set; }
        public MonoAccount? Account { get; set; }
    }
}
