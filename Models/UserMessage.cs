using System;
using System.Collections.Generic;
using System.Text;

namespace portfolio_functions.Models
{
    public record UserChatMessage
    (
        string? conversationId,
        string message
    );
}
