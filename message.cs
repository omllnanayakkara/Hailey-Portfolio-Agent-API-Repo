using Azure;
using Azure.AI.Extensions.OpenAI;
using Azure.Core;
using Azure.Messaging.WebPubSub;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using OpenAI.Responses;
using portfolio_functions.Services;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

#pragma warning disable OPENAI001

namespace portfolio_functions;

record ServerMessage(string message, string status);

public class Message
{
    private readonly ILogger<Message> _logger;
    private readonly ProjectResponsesClient _responseClient;
    public Message(ILogger<Message> logger, IAgentClient agentClient)
    {
        _logger = logger;
        _responseClient = agentClient.GetProjectResponsesClient();
    }

    [Function("message")]
    [WebPubSubOutput(Hub = "agentchat", Connection = "WebPubSubConnectionString")]
    public async Task RunMessage(
    [WebPubSubTrigger("agentchat", WebPubSubEventType.User, "message")] UserEventRequest request)
    {
        var client = new WebPubSubServiceClient(Environment.GetEnvironmentVariable("WebPubSubConnectionString"), "agentchat");
        try
        {
            Console.WriteLine(request.Data.ToString());
            _logger.LogError(request.Data.ToString());

            var streamresponses = _responseClient.CreateResponseStreamingAsync(request.Data.ToString());

            // Stream the response
            await foreach (var streamResponse in streamresponses)
            {

                if (streamResponse is StreamingResponseCreatedUpdate createUpdate)
                {
                    Console.WriteLine($"Stream response created with ID: {createUpdate.Response.Id}");
                    client.SendToUser(
                         request.ConnectionContext.UserId,
                         JsonSerializer.Serialize(new ServerMessage("START", "START")),
                         ContentType.ApplicationJson
                    );
                    
                }
                else if (streamResponse is StreamingResponseOutputTextDeltaUpdate textDelta)
                {
                    Console.WriteLine($"Delta: {textDelta.Delta}");
                    client.SendToUser(
                         request.ConnectionContext.UserId,
                         JsonSerializer.Serialize(new ServerMessage(textDelta.Delta, "INPROGRESS")),
                         ContentType.ApplicationJson
                    );

                }
                else if (streamResponse is StreamingResponseOutputTextDoneUpdate textDoneUpdate)
                {
                    Console.WriteLine($"Response done with full message: {textDoneUpdate.Text}");
                    client.SendToUser(
                         request.ConnectionContext.UserId,
                         JsonSerializer.Serialize(new ServerMessage(textDoneUpdate.Text, "END")),
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
                 JsonSerializer.Serialize(new ServerMessage(ex.Message, "ERROR")),
                 ContentType.ApplicationJson
            );
        }

    }
}
