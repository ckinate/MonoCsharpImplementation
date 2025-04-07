using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MonoIntegrationNew.Interfaces;
using MonoIntegrationNew.Models;
using System.Text.Json;

namespace MonoIntegrationNew.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MonoIntegrationController : ControllerBase
    {
        private readonly IIntegrationServices _integrationServices;
        private readonly ILogger<MonoIntegrationController> _logger;
        public MonoIntegrationController(IIntegrationServices integrationServices, ILogger<MonoIntegrationController> logger)
        {
            _integrationServices = integrationServices;
            _logger = logger;

        }
        [HttpPost("initiate")]
        public async Task<IActionResult> InitiateAccountLinking([FromBody] Customer request)
        {
            if (string.IsNullOrEmpty(request.Name)|| string.IsNullOrEmpty(request.Email))
            {
                return BadRequest("Customer name and email are required");
            } 

            var initiateRequest = new MonoAccountInitiateRequest
            {
                Customer = request
            };
            try
            {
               var integrationRequestService = await _integrationServices.InitiateAccountLinkingAsync(initiateRequest);
                return Ok(new
                {
                    success = true,
                    monoUrl = integrationRequestService,
                    message = "Account linking initiated successfully"
                });
               
            }
            catch (Exception ex) 
            { 
                return StatusCode(500, ex.Message);
            }
            

        }

        [HttpPost("webhook")]
        public async Task<IActionResult> HandleWebhook()
        {
            try
            {
                // Read the request body
                using var reader = new StreamReader(Request.Body);
                var body = await reader.ReadToEndAsync();

                _logger.LogInformation("Received webhook: {WebhookBody}", body);

                // Deserialize the webhook payload
                var webhookEvent = JsonSerializer.Deserialize<MonoWebhookEvent>(body,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                // Process different event types
                switch (webhookEvent.Event)
                {
                    case "mono.events.account_connected":
                        await _integrationServices.ProcessAccountConnectedEvent(webhookEvent);
                        break;

                    case "mono.events.account_updated":
                        await _integrationServices.ProcessAccountUpdatedEvent(webhookEvent);
                        break;

                    default:
                        _logger.LogInformation("Unhandled webhook event: {EventType}", webhookEvent.Event);
                        break;
                }

                // Acknowledge receipt of the webhook
                return Ok(new { status = "received" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing webhook");
                return StatusCode(500, "Error processing webhook");
            }
        }
        public async Task<ActionResult<StatementResponse>> GetAccountStatement([FromBody] StatementRequest statementRequest)
        {
            if (string.IsNullOrEmpty(statementRequest.CustomerName)|| string.IsNullOrEmpty(statementRequest.CustomerEmail))
            {
                return BadRequest("Customer name and email is required");
            }
            var statementResponse = await  _integrationServices.GetAccountStatementAsync(statementRequest);

            return Ok(statementResponse);
        }
    }
}
