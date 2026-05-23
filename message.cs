using Azure;
using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using portfolio_functions.Services;

namespace portfolio_functions;

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
    public SendToUserAction RunMessage(
    [WebPubSubTrigger("agentchat", WebPubSubEventType.User, "message")] UserEventRequest request)
    {
        try
        {
            var response = _agentService.SendMessage(request.Data.ToString());
            Console.WriteLine(request.Data.ToString());
            _logger.LogError(request.Data.ToString());
            return new SendToUserAction
            {
                UserId = request.ConnectionContext.UserId,
                Data = BinaryData.FromString(response),
                DataType = WebPubSubDataType.Text
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            _logger.LogError(ex.Message);
            return new SendToUserAction
            {
                UserId = request.ConnectionContext.UserId,
                Data = BinaryData.FromString($"[{request.ConnectionContext.UserId}] Something went wrong, please try again later."),
                DataType = WebPubSubDataType.Text
            };
        }

    }
}
