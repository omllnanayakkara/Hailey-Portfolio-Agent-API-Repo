using Azure.AI.Extensions.OpenAI;
using Azure.AI.Projects;
using portfolio_functions.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace portfolio_functions.Services
{
    public class AgentService(IAgentClient agentClient) : IAgentService
    {
        private readonly IAgentClient _agentClient = agentClient;
        public AgentServiceResponse GetProjectResponsesClient(string? coversationId = null)
        {
            string _agentName = Environment.GetEnvironmentVariable("AgentName")!;
            string _agentVersion = Environment.GetEnvironmentVariable("AgentVersion")!;
            AIProjectClient projectClient = _agentClient.GetProjectClient();
            ProjectConversation conversation = coversationId is not null ? 
                projectClient.OpenAI.Conversations.GetProjectConversation(coversationId) :  
                projectClient.OpenAI.Conversations.CreateProjectConversation();
            AgentReference agentReference = new(name: _agentName, version: _agentVersion);
            var progectResponsesClient = projectClient.OpenAI.GetProjectResponsesClientForAgent(agentReference, conversation.Id);
            return new AgentServiceResponse(conversation.Id, progectResponsesClient);
        }
    }
}
