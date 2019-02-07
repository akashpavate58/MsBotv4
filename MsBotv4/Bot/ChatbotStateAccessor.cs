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
        public UserState UserState { get; }
        public IStatePropertyAccessor<ChatbotState> ChatbotStateProperty { get; set; }
        public IStatePropertyAccessor<ChatUserState> ChatUserStateProperty { get; set; }
        public static string ChatbotStateName { get; } = $"{nameof(ChatbotStateAccessor)}.ChatbotStateProperty";
        public static string ChatUserStateName { get; } = $"{nameof(ChatbotStateAccessor)}.ChatUserStateProperty";
        //=================================================================================================================================
        public static ChatbotStateAccessor Create(ConversationState conversationState, UserState userState)
        {
            return new ChatbotStateAccessor(conversationState, userState)
            {
                ChatbotStateProperty = conversationState.CreateProperty<ChatbotState>(ChatbotStateAccessor.ChatbotStateName),
                ChatUserStateProperty = userState.CreateProperty<ChatUserState>(ChatbotStateAccessor.ChatUserStateName)
            };
        }
        //=================================================================================================================================
        public ChatbotStateAccessor(ConversationState conversationState, UserState userState)
        {
            ConversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));
            UserState = userState ?? throw new ArgumentNullException(nameof(userState));
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

        public async Task<ChatUserState> FetchUserStateAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await this.ChatUserStateProperty.GetAsync(turnContext, () => ChatUserState.Default, cancellationToken);
        }

        public async Task SaveUserStateAsync(ITurnContext turnContext, ChatUserState newState, CancellationToken cancellationToken = default(CancellationToken))
        {
            await this.ChatUserStateProperty.SetAsync(turnContext, newState, cancellationToken);
            await this.UserState.SaveChangesAsync(turnContext);
        }
    }
}
