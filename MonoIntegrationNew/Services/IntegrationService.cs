using MonoIntegrationNew.Interfaces;
using MonoIntegrationNew.Models;
using System.Text.Json;
using System.Text;
using MonoIntegrationNew.Data;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Net.Http;
using Microsoft.IdentityModel.Tokens;

namespace MonoIntegrationNew.Services
{
    public class IntegrationService : IIntegrationServices
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly AppDbContext _dbContext;
        private readonly ILogger<IntegrationService> _logger;
        private readonly string _monoSecretKey;
        private readonly string _redirectUrl;
        private readonly string _monoAccountUrl;

        public IntegrationService(HttpClient httpClient, IConfiguration config, ILogger<IntegrationService> logger, AppDbContext dbContext)
        {
            _httpClient = httpClient;
            _config = config;
            _logger = logger;
            _dbContext = dbContext;
            _monoSecretKey = _config["Mono:SecretKey"]!;
            _redirectUrl = _config["Mono:DefaultRedirectUrl"]!;
            _monoAccountUrl = _config["Mono:MonoAccountUrl"]!;
        }

        public async Task<string> InitiateAccountLinkingAsync(MonoAccountInitiateRequest request)
        {
            
           
            
           
         
            try
            {
                // Validate the input
                if (string.IsNullOrEmpty(request.Customer.Name) ||
                    string.IsNullOrEmpty(request.Customer.Email))
                {
                    throw new Exception("Customer name and email are required");
                }
                // Generate a reference if not provided
                var reference = request.Meta.Ref ?? Guid.NewGuid().ToString();
                var meta = new Meta { Ref = reference };
                var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_monoAccountUrl}/initiate");
                httpRequest.Headers.Add("mono-sec-key", _monoSecretKey);
                var payload = new MonoAccountInitiateRequest
                {
                    Customer = request.Customer,
                    Meta = meta,
                    Scope = "auth",
                    RedirectUrl = _redirectUrl
                };

                var json = JsonSerializer.Serialize(payload);
                httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.SendAsync(httpRequest);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var responseObj = JsonSerializer.Deserialize<MonoAccountLinkingResponse>(
                      content,
                      new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    // Save the linking request to the database
                    var linkingRequest = new MonoLinkingRequest
                    {
                        CustomerName = request.Customer.Name,
                        CustomerEmail = request.Customer.Email,
                        Reference = reference,
                        MonoCustomerId = responseObj.Data.Customer,
                        MonoUrl = responseObj.Data.MonoUrl,
                        CreatedAt = DateTime.UtcNow,
                        Status = "INITIATED"
                    };

                    _dbContext.MonoLinkingRequests.Add(linkingRequest);
                    await _dbContext.SaveChangesAsync();

                    return  responseObj.Data.MonoUrl;

                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Mono API error: {ErrorContent}", errorContent);
                    return $"StausCode: {(int)response.StatusCode}, Message: {errorContent}";

                }
                   



            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initiating account linking");
                return  "An error occurred while processing your request";
            } 
         

           

           
           
            //var result = JsonSerializer.Deserialize<JsonElement>(content);
            //var monoUrl = result.GetProperty("data").GetProperty("mono_url").GetString();

            //return monoUrl;
        }

        public async Task ProcessAccountConnectedEvent(MonoWebhookEvent webhookEvent)
        {
            var accountId = webhookEvent.Data.Id;

            _logger.LogInformation("Account connected: {AccountId}", accountId);

            // Find the most recent linking request for this user
            // This is a simplified approach - I will think about a more reliable method
            // to associate the account with the correct user
            var linkingRequest = await _dbContext.MonoLinkingRequests
                .OrderByDescending(r => r.CreatedAt)
                .FirstOrDefaultAsync(r => r.Status == "INITIATED");

            if (linkingRequest != null)
            {
                // Update the linking request with the account ID
                linkingRequest.MonoAccountId = accountId;
                linkingRequest.Status = "CONNECTED";
                linkingRequest.UpdatedAt = DateTime.UtcNow;

                _dbContext.Entry(linkingRequest).State = EntityState.Detached;
                 
                _dbContext.MonoLinkingRequests.Update(linkingRequest);
                await _dbContext.SaveChangesAsync();

                // Now we need to check the data status before proceeding
                // We could either wait for the account_updated webhook or
                // proactively check the account status
                await CheckAccountDataStatus(accountId);
            }
            else
            {
                _logger.LogWarning("Received account_connected webhook but couldn't find matching linking request");
            }
        }
        public async Task ProcessAccountUpdatedEvent(MonoWebhookEvent webhookEvent)
        {
            var accountData = webhookEvent.Data;
            var dataStatus = accountData.Meta?.DataStatus;

            _logger.LogInformation("Account updated: Status = {DataStatus}", dataStatus);

            if (string.IsNullOrEmpty(dataStatus))
            {
                _logger.LogWarning("Received account_updated webhook without data status");
                return;
            }

            // Find the account in our database
            var accountRecord = await _dbContext.MonoAccounts
                .FirstOrDefaultAsync(a => a.MonoAccountId == accountData.Account._id);

            if (accountRecord != null)
            {
                // Update the account with the latest information
                accountRecord.DataStatus = dataStatus;
                accountRecord.UpdatedAt = DateTime.UtcNow;

                // Also update account details if available
                if (accountData.Account != null)
                {
                    accountRecord.AccountName = accountData.Account.Name;
                    accountRecord.AccountNumber = accountData.Account.AccountNumber;
                    accountRecord.BankName = accountData.Account.Institution?.Name;
                    accountRecord.BankCode = accountData.Account.Institution?.BankCode;
                    accountRecord.AccountType = accountData.Account.Type;
                    accountRecord.Currency = accountData.Account.Currency;
                    accountRecord.Balance = accountData.Account.Balance;
                }

                await _dbContext.SaveChangesAsync();

                // If the data is now available, we could trigger any additional processing
                if (dataStatus.Equals("AVAILABLE", StringComparison.OrdinalIgnoreCase))
                {
                    await ProcessAvailableAccountData(accountRecord.MonoAccountId);
                }
            }
            else
            {
                _logger.LogWarning("Received account_updated webhook but couldn't find matching account");
            }
        }

        private async Task CheckAccountDataStatus(string accountId)
        {
            try
            {
                
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_monoAccountUrl}/{accountId}");
                request.Headers.Add("mono-sec-key", _monoSecretKey);
                

                var response = await _httpClient.SendAsync(request);

                // Get account details
               

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var accountDetails = JsonSerializer.Deserialize<MonoAccountDetailsResponse>(
                        responseString,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    var dataStatus = accountDetails.Data.Meta?.DataStatus;
                    _logger.LogInformation("Account data status: {DataStatus}", dataStatus);

                    // Save the account details
                    var existingAccount = await _dbContext.MonoAccounts
                        .FirstOrDefaultAsync(a => a.MonoAccountId == accountId);

                    if (existingAccount == null)
                    {
                        var account = new MonoAccount
                        {
                            MonoAccountId = accountId,
                            AccountName = accountDetails.Data.Account?.Name,
                            AccountNumber = accountDetails.Data.Account?.AccountNumber,
                            BankName = accountDetails.Data.Account?.Institution?.Name,
                            BankCode = accountDetails.Data.Account?.Institution?.BankCode,
                            AccountType = accountDetails.Data.Account?.Type,
                            Currency = accountDetails.Data.Account?.Currency,
                            Balance = accountDetails.Data.Account?.Balance ?? 0,
                            DataStatus = dataStatus,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        _dbContext.MonoAccounts.Add(account);
                    }
                    else
                    {
                        existingAccount.DataStatus = dataStatus;
                        existingAccount.UpdatedAt = DateTime.UtcNow;

                        if (accountDetails.Data.Account != null)
                        {
                            existingAccount.AccountName = accountDetails.Data.Account.Name;
                            existingAccount.AccountNumber = accountDetails.Data.Account.AccountNumber;
                            existingAccount.BankName = accountDetails.Data.Account.Institution?.Name;
                            existingAccount.BankCode = accountDetails.Data.Account.Institution?.BankCode;
                            existingAccount.AccountType = accountDetails.Data.Account.Type;
                            existingAccount.Currency = accountDetails.Data.Account.Currency;
                            existingAccount.Balance = accountDetails.Data.Account.Balance;
                        }

                        _dbContext.Entry(existingAccount).State = EntityState.Detached;

                        _dbContext.MonoAccounts.Update(existingAccount);
                    }

                    await _dbContext.SaveChangesAsync();

                    // If data is available, we can process it
                    if (dataStatus?.Equals("AVAILABLE", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        await ProcessAvailableAccountData(accountId);
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error getting account details: {ErrorContent}", errorContent);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking account data status");
            }
        }

        private async Task ProcessAvailableAccountData(string accountId)
        {
            // This is where you would fetch and process financial data
            // such as transactions, statements, etc.

            _logger.LogInformation("Processing available data for account: {AccountId}", accountId);

            // Example: Fetch transactions
            await FetchAccountTransactions(accountId);
        }

        private async Task<MonoTransactionsResponse> FetchAccountTransactions(string accountId)
        {
            try
            {
                // Validate input
                if (string.IsNullOrEmpty(accountId))
                {
                  throw new Exception("Account ID is required");
                }

                // Check if we have a record for this account
                var accountRecord = await _dbContext.MonoAccounts
                    .FirstOrDefaultAsync(a => a.MonoAccountId == accountId);

                if (accountRecord == null)
                {
                   throw new Exception ($"Account with ID {accountId} not found");
                }

                // Check if data is available
                if (accountRecord.DataStatus != "AVAILABLE")
                {
                   throw new Exception ($"Account data is not available. Current status: {accountRecord.DataStatus}");
                }

                //Set up request
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_monoAccountUrl}/{accountId}/transactions");

                // Add headers
                request.Headers.Add("mono-sec-key", _monoSecretKey);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // Send request
                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();

                    // Log the raw response for debugging
                    _logger.LogInformation("Transactions response: {Response}", responseString);

                    // Deserialize the response
                    var transactionsResponse = JsonSerializer.Deserialize<MonoTransactionsResponse>(
                        responseString,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    // Store transactions in database (optional)
                    if (transactionsResponse?.Data != null && transactionsResponse.Data.Count > 0)
                    {
                        foreach (var transaction in transactionsResponse.Data)
                        {
                            var existingTransaction = await _dbContext.MonoTransactions
                                .FirstOrDefaultAsync(t => t.TransactionId == transaction.Id);

                            if (existingTransaction == null)
                            {
                                _dbContext.MonoTransactions.Add(new MonoTransaction
                                {
                                    MonoAccountId = accountId,
                                    TransactionId = transaction.Id,
                                    Narration = transaction.Narration,
                                    Amount = transaction.Amount,
                                    Type = transaction.Type,
                                    Balance = transaction.Balance,
                                    Date = transaction.Date,
                                    Category = transaction.Category,
                                    CreatedAt = DateTime.UtcNow
                                });
                            }
                        }

                        await _dbContext.SaveChangesAsync();
                    }

                    return transactionsResponse;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error getting transactions: {ErrorContent}", errorContent);
                    throw new Exception ( $"Status Code: {(int)response.StatusCode}, Error: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transactions");
                throw new Exception("An error occurred while processing your request") ;
            }
        }

        public async Task<StatementResponse> GetAccountStatementAsync(StatementRequest statementRequest)
        {
            var accountId = "";
            if (string.IsNullOrEmpty(statementRequest.CustomerName) || string.IsNullOrEmpty(statementRequest.CustomerEmail))
                throw new NullReferenceException("Customer name and customer email is required");
            if (statementRequest.IsAccountLink == true)
            {
                var linkData = await _dbContext.MonoLinkingRequests.FirstOrDefaultAsync(r=>r.CustomerName == statementRequest.CustomerName && r.CustomerEmail == statementRequest.CustomerEmail);
                accountId = linkData.MonoAccountId;
            }
            if (string.IsNullOrEmpty(accountId))
                throw new NullReferenceException(" AccountId is empty");
            // Validate period
            if (statementRequest.Period < 1 || statementRequest.Period > 12)
                throw new ArgumentOutOfRangeException(nameof(statementRequest.Period), "Period must be between 1 and 12 months");

            // Set up request
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_monoAccountUrl}/{accountId}/statement?period=last{statementRequest.Period}months");

            // Add headers
            request.Headers.Add("mono-sec-key", _monoSecretKey);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // Add realtime header if requested
            //if (statementRequest.Realtime)
            //    request.Headers.Add("x-realtime", "true");

            // Send request
            var response = await _httpClient.SendAsync(request);

            // Ensure success
            response.EnsureSuccessStatusCode();

            // Parse response
            var jsonString = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var statementsResponse = JsonSerializer.Deserialize<StatementResponse>(jsonString, options);

            // Store transactions in database (optional)
            if (statementsResponse?.Data != null && statementsResponse.Data.Count > 0)
            {
                foreach (var transaction in statementsResponse.Data)
                {
                    var existingTransaction = await _dbContext.MonoTransactions
                        .FirstOrDefaultAsync(t => t.TransactionId == transaction.Id);

                    if (existingTransaction == null)
                    {
                        _dbContext.MonoTransactions.Add(new MonoTransaction
                        {
                            MonoAccountId = accountId,
                            TransactionId = transaction.Id,
                            Narration = transaction.Narration,
                            Amount = transaction.Amount,
                            Type = transaction.Type,
                            Balance = transaction.Balance,
                            Date = transaction.Date,
                            Category = transaction.Category,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }

                await _dbContext.SaveChangesAsync();
            }

            return statementsResponse;
        }
    }
}
