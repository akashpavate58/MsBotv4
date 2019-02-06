using Microsoft.Bot.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MsBotv4.Bot
{
    public class ChatbotStateAccessor
    {
        public ConversationState ConversationState { get; }
        public IStatePropertyAccessor<ChatbotState> ChatbotStateProperty { get; set; }
        public static string ChatbotStateName { get; } = $"{nameof(ChatbotStateAccessor)}.ChatbotStateProperty";

        public ChatbotStateAccessor(ConversationState conversationState)
        {
            ConversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));
        }

        public async Task<ChatbotState> FetchStateAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await this.ChatbotStateProperty.GetAsync(turnContext, () => ChatbotState.Default, cancellationToken);
        }

        public async Task SaveStateAsync(ITurnContext turnContext, ChatbotState newState, CancellationToken cancellationToken = default(CancellationToken))
        {
            await this.ChatbotStateProperty.SetAsync(turnContext, newState, cancellationToken);
            await this.ConversationState.SaveChangesAsync(turnContext);
        }
    }
}
