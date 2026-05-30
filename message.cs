using Azure.AI.Extensions.OpenAI;
using Azure.Core;
using Azure.Messaging.WebPubSub;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using OpenAI.Responses;
using portfolio_functions.Models;
using portfolio_functions.Services;
using System.Text.Json;

#pragma warning disable OPENAI001

namespace portfolio_functions;

record ServerMessage(string? conversationId, string message, string status);

public class Message
{
    private readonly ILogger<Message> _logger;
    private readonly IAgentService _agentService;
    public Message(ILogger<Message> logger, IAgentService agentService)
    {
        _logger = logger;
        _agentService = agentService;
    }

    [Function("message")]
    [WebPubSubOutput(Hub = "agentchat", Connection = "WebPubSubConnectionString")]
    public async Task RunMessage(
    [WebPubSubTrigger("agentchat", WebPubSubEventType.User, "message")] UserEventRequest request)
    {
        var client = new WebPubSubServiceClient(Environment.GetEnvironmentVariable("WebPubSubConnectionString"), "agentchat");
        var userMessage = request.Data.ToObjectFromJson<UserChatMessage>();
        _logger.LogInformation("Chat message received: message {0} from conversation {1}", userMessage.message, userMessage.conversationId);
        bool _internal = false;
        try
        {
            var (ConversationId, ProjectResponsesClient) = _agentService.GetProjectResponsesClient();
            var streamresponses = ProjectResponsesClient.CreateResponseStreamingAsync(request.Data.ToString());

            // Stream the response
            await foreach (var streamResponse in streamresponses)
            {

                if (streamResponse is StreamingResponseCreatedUpdate createUpdate)
                {
                    Console.WriteLine($"Stream response created with ID: {createUpdate.Response.Id}");
                    client.SendToUser(
                         request.ConnectionContext.UserId,
                         JsonSerializer.Serialize(new ServerMessage(ConversationId, "START", "START")),
                         ContentType.ApplicationJson
                    );
                    
                }
                else if (streamResponse is StreamingResponseOutputTextDeltaUpdate textDelta)
                {
                    Console.WriteLine($"Delta: {textDelta.Delta}");
                    if (textDelta.Delta.Contains("{"))
                    {
                        _internal = true;
                        continue;
                    }

                    if (_internal)
                    {
                        continue;
                    }
                    client.SendToUser(
                         request.ConnectionContext.UserId,
                         JsonSerializer.Serialize(new ServerMessage(ConversationId, textDelta.Delta, "INPROGRESS")),
                         ContentType.ApplicationJson
                    );

                }
                else if (streamResponse is StreamingResponseOutputTextDoneUpdate textDoneUpdate)
                {
                    Console.WriteLine($"Response done with full message: {textDoneUpdate.Text}");
                    if (_internal)
                    {
                        _internal = false;
                        continue;
                    }
                    client.SendToUser(
                         request.ConnectionContext.UserId,
                         JsonSerializer.Serialize(new ServerMessage(ConversationId, textDoneUpdate.Text, "END")),
                         ContentType.ApplicationJson
                    );
                }
                else if (streamResponse is StreamingResponseErrorUpdate errorUpdate)
                {
                    throw new InvalidOperationException($"The stream has failed with the error: {errorUpdate.Message}");
                }
                
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            _logger.LogError(ex.Message);
            client.SendToUser(
                 request.ConnectionContext.UserId,
                 JsonSerializer.Serialize(new ServerMessage(userMessage.conversationId, ex.Message, "ERROR")),
                 ContentType.ApplicationJson
            );
        }

    }
}
