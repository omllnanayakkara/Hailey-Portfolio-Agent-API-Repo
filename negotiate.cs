using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace portfolio_functions;

public class Negotiate
{
    private readonly ILogger<Negotiate> _logger;

    public Negotiate(ILogger<Negotiate> logger)
    {
        _logger = logger;
    }

    [Function("negotiate")]
    public HttpResponseData RunNegotiate([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req,
    [WebPubSubConnectionInput(Hub = "agentchat", UserId = "{query.user}", Connection = "WebPubSubConnectionString")] WebPubSubConnection connectionInfo)
    {
        var response = req.CreateResponse(HttpStatusCode.OK);
        response.WriteAsJsonAsync(connectionInfo);
        return response;
    }
}
