using Azure.AI.Extensions.OpenAI;
using Azure.AI.Projects;
using Azure.AI.Projects.Agents;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace portfolio_functions.Services
{
    public class AgentClient : IAgentClient
    {
        public AIProjectClient GetProjectClient()
        {
            string _endpoint = Environment.GetEnvironmentVariable("AgentEndpoint")!;
            AIProjectClient projectClient = new(endpoint: new Uri(_endpoint), tokenProvider: new DefaultAzureCredential());
            return projectClient;
        }
    }
}
