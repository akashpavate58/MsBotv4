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
        private readonly BotServices _services;

        public Chatbot(BotServices services, ChatbotStateAccessor accessor)
        {
            _accessor = accessor ?? throw new ArgumentNullException(nameof(accessor));
            _services = services ?? throw new System.ArgumentNullException(nameof(services));
            if (!_services.LuisServices.ContainsKey(Constants.LuisKey))
            {
                throw new System.ArgumentException($"Invalid configuration. Please check your '.bot' file for a LUIS service named '{Constants.LuisKey}'.");
            }
        }

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            switch (turnContext.Activity.Type)
            {
                case ActivityTypes.Message:
                    {
                        ////This is the way to Get and Set State Values from Conversation State
                        //var state = await _accessor.FetchStateAsync(turnContext, cancellationToken);
                        //state.NameOfTheUser = "Akash";
                        //await _accessor.SaveStateAsync(turnContext, state);

                        //await turnContext.SendActivityAsync("Hi, I'm John - The Bot. How can I help you?");


                        // Check LUIS model
                        var recognizerResult = await _services.LuisServices[Constants.LuisKey].RecognizeAsync(turnContext, cancellationToken);
                        var topIntent = recognizerResult?.GetTopScoringIntent();

                        switch (topIntent.Value.intent)
                        {
                            case Intent_None:
                                await turnContext.SendActivityAsync("Sorry, I wasn't quite able to get that. Can you try re-phrasing your sentence, please?");
                                break;
                            case Intent_Greeting:
                                await turnContext.SendActivityAsync("Hi, I am Juliet, a new recuit to BBB Auto Insurance Company. " +
                                    "I am virtual assistant who's always ready to help you with your insurance needs");
                                await turnContext.SendActivityAsync("How can I help you today?");
                                break;
                            case Intent_Capabilities:
                                await turnContext.SendActivityAsync("I can help you get an insurance quote for your car.\nTry saying,\n*I need an insurance quote*");
                                break;
                            case Intent_InsuranceQuote:
                                await turnContext.SendActivityAsync("Sure, I can help with getting an insurance quote");
                                break;
                            default:
                                await turnContext.SendActivityAsync("I am unable to understand that. I am very sorry for not being able to assit you on this.");
                                break;
                        }

                        break;
                    }
                default:
                    {
                        await HandleActionsAsync(turnContext, cancellationToken);
                        break;
                    }
            }
        }

        public const string Intent_InsuranceQuote = "InsuranceQuote";
        public const string Intent_Greeting = "Greeting";
        public const string Intent_Capabilities = "Capabilities";
        public const string Intent_None = "None";


        public async Task HandleActionsAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            return;
        }
    }
}
