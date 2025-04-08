using MonoIntegrationNew.Data;
using MonoIntegrationNew.Models;
using System.Threading.Tasks;

namespace MonoIntegrationNew.Interfaces
{
    public interface IIntegrationServices
    {
        Task<string> InitiateAccountLinkingAsync(MonoAccountInitiateRequest request);
        Task ProcessAccountConnectedEvent(MonoWebhookEvent webhookEvent);
        Task ProcessAccountUpdatedEvent(MonoWebhookEvent webhookEvent);
        Task<StatementResponse> GetAccountStatementAsync(StatementRequest statementRequest);
        
    }
}
