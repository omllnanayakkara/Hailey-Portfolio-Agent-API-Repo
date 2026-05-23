using Azure.AI.Extensions.OpenAI;
using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Extensions.Logging;
using OpenAI.Responses;

#pragma warning disable OPENAI001

namespace portfolio_functions.Services
{
    public class AgentService(ILogger<AgentService> logger, IAgentClient agentClient) : IAgentService
    {
        private readonly ProjectResponsesClient responseClient = agentClient.GetProjectResponsesClient();
        public string SendMessage(string message) 
        {
            try
            {

                // Use the agent to generate a response
                ResponseResult response = responseClient.CreateResponse(
                    message
                );
                string reply = response.GetOutputText();
                Console.WriteLine(response.GetOutputText());
                return reply;
            }
            catch (Exception ex) {
                logger.LogError("Message send failed: {0}, error: {1}", message, ex.Message);
                throw new Exception(ex.Message);
            }
        }
    }
}
