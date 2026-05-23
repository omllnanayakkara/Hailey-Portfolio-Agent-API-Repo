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
        public ProjectResponsesClient GetProjectResponsesClient()
        {
            string _endpoint = Environment.GetEnvironmentVariable("AgentEndpoint")!;
            string _agentName = Environment.GetEnvironmentVariable("AgentName")!;
            string _agentVersion = Environment.GetEnvironmentVariable("AgentVersion")!;
            AIProjectClient projectClient = new(endpoint: new Uri(_endpoint), tokenProvider: new DefaultAzureCredential());

            AgentReference agentReference = new(name: _agentName, version: _agentVersion);
            return projectClient.OpenAI.GetProjectResponsesClientForAgent(agentReference);
        }
    }
}
