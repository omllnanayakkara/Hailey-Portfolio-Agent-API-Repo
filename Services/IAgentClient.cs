using Azure.AI.Extensions.OpenAI;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace portfolio_functions.Services
{
    public interface IAgentClient
    {
        ProjectResponsesClient GetProjectResponsesClient();
    }
}
