using Azure.AI.Extensions.OpenAI;
using Microsoft.Extensions.Configuration;
using portfolio_functions.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace portfolio_functions.Services
{
    public interface IAgentService
    {
        AgentServiceResponse GetProjectResponsesClient(string? coversationId=null);
    }
}
