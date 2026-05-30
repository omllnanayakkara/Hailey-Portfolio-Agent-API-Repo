using Azure.AI.Extensions.OpenAI;
using Azure.AI.Projects;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace portfolio_functions.Services
{
    public interface IAgentClient
    {
        AIProjectClient GetProjectClient();
    }
}
