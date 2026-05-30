using Azure.AI.Extensions.OpenAI;
using System;
using System.Collections.Generic;
using System.Text;

namespace portfolio_functions.Models
{
    public record AgentServiceResponse
    (
        string ConversationId,
        ProjectResponsesClient ProjectResponsesClient
    );
}
