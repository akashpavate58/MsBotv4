using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MsBotv4.Bot
{
    public class Chatbot : IBot
    {
        private readonly ChatbotStateAccessor _accessor;

        public Chatbot(ChatbotStateAccessor accessor)
        {
            _accessor = accessor ?? throw new ArgumentNullException(nameof(accessor));
        }

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            switch (turnContext.Activity.Type)
            {
                case ActivityTypes.Message:
                    {
                        //This is the way to Get and Set State Values from Conversation State
                        var state = await _accessor.FetchStateAsync(turnContext, cancellationToken);
                        state.NameOfTheUser = "Akash";
                        await _accessor.SaveStateAsync(turnContext, state);

                        await turnContext.SendActivityAsync("Hi, I'm John - The Bot. How can I help you?");
                        break;
                    }
                default:
                    {
                        await HandleActionsAsync(turnContext, cancellationToken);
                        break;
                    }
            }
        }

        public async Task HandleActionsAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            return;
        }
    }
}
