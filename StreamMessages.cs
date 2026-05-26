using Azure;
using Azure.AI.Extensions.OpenAI;
using Azure.AI.Projects.Agents;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using OpenAI.Responses;
using portfolio_functions.Services;
using System.Text.Json;

namespace portfolio_functions;
#pragma warning disable OPENAI001

record UserMessage(string message);

public class StreamMessages
{
    private readonly ILogger<StreamMessages> _logger;
    private readonly ProjectResponsesClient _responseClient;
    public StreamMessages(ILogger<StreamMessages> logger, IAgentClient agentClient)
    {
        _responseClient = agentClient.GetProjectResponsesClient();
        _logger = logger;
    }

    [Function("StreamMessages")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");
        var usermessage = await JsonSerializer.DeserializeAsync<UserMessage>(req.Body);
        var streamresponses = _responseClient.CreateResponseStreamingAsync(usermessage!.message);

        // Set response headers for streaming
        req.HttpContext.Response.ContentType = "text/event-stream";
        req.HttpContext.Response.Headers.Add("Cache-Control", "no-cache");
        req.HttpContext.Response.Headers.Add("Connection", "keep-alive");

        // Stream the response
        await foreach (var streamResponse in streamresponses)
        {
            //var jsonChunk = JsonSerializer.Serialize(chunk);
            //await req.HttpContext.Response.WriteAsync($"data: {jsonChunk}\n\n");
            await req.HttpContext.Response.Body.FlushAsync();
            if (streamResponse is StreamingResponseCreatedUpdate createUpdate)
            {
                Console.WriteLine($"Stream response created with ID: {createUpdate.Response.Id}");
                await req.HttpContext.Response.WriteAsync($"Stream response created with ID: {createUpdate.Response.Id}\n\n");
            }
            else if (streamResponse is StreamingResponseOutputTextDeltaUpdate textDelta)
            {
                Console.WriteLine($"Delta: {textDelta.Delta}");
                await req.HttpContext.Response.WriteAsync($"data: {textDelta.Delta}\n\n");
            }
            else if (streamResponse is StreamingResponseOutputTextDoneUpdate textDoneUpdate)
            {
                Console.WriteLine($"Response done with full message: {textDoneUpdate.Text}");
                await req.HttpContext.Response.WriteAsync($"full message: {textDoneUpdate.Text}\n\n");
            }
            else if (streamResponse is StreamingResponseErrorUpdate errorUpdate)
            {
                throw new InvalidOperationException($"The stream has failed with the error: {errorUpdate.Message}");
            }
            await req.HttpContext.Response.Body.FlushAsync();

        }

        return new EmptyResult();
    }
}
