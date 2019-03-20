using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
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

        private DialogSet _dialogs;

        public Chatbot(BotServices services, ChatbotStateAccessor accessor)
        {
            _accessor = accessor ?? throw new ArgumentNullException(nameof(accessor));
            _services = services ?? throw new System.ArgumentNullException(nameof(services));
            if (!_services.LuisServices.ContainsKey(Constants.LuisKey))
            {
                throw new System.ArgumentException($"Invalid configuration. Please check your '.bot' file for a LUIS service named '{Constants.LuisKey}'.");
            }

            _dialogs = new DialogSet(_accessor.ConversationDialogState);

            // This array defines how the Waterfall will execute.
            var waterfallSteps = new WaterfallStep[]
            {
                NameStepAsync,
                AcquireNameStepAsync
            };

            // Add named dialogs to the DialogSet. These names are saved in the dialog state.
            _dialogs.Add(new WaterfallDialog("details", waterfallSteps)); //(dialogId, dialogsInThatDialogId)
            _dialogs.Add(new TextPrompt("name")); // (dialogId)
            _dialogs.Add(new NumberPrompt<int>("age"));
            _dialogs.Add(new ConfirmPrompt("confirm"));
        }

        private async Task<DialogTurnResult> NameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync("name", new PromptOptions {
                Prompt = MessageFactory.Text("Please enter your name")
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> AcquireNameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string name = (string)stepContext.Result;
            var state = await _accessor.FetchUserStateAsync(stepContext.Context, cancellationToken);
            state.Username = name.Trim();
            await _accessor.SaveUserStateAsync(stepContext.Context, state, cancellationToken);
            await stepContext.Context.SendActivityAsync($"Thank you {name}");
            return await stepContext.EndDialogAsync();
        }

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            
            switch (turnContext.Activity.Type)
            {
                case ActivityTypes.Message:
                    {
                        var dialogContext = await _dialogs.CreateContextAsync(turnContext, cancellationToken);
                        var results = await dialogContext.ContinueDialogAsync(cancellationToken);

                        ////This is the way to Get and Set State Values from Conversation State
                        //var state = await _accessor.FetchStateAsync(turnContext, cancellationToken);
                        //state.NameOfTheUser = "Akash";
                        //await _accessor.SaveStateAsync(turnContext, state);

                        //await turnContext.SendActivityAsync("Hi, I'm John - The Bot. How can I help you?");


                        // Check LUIS model
                        if (results.Status == DialogTurnStatus.Empty)
                        {
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
                                    await dialogContext.BeginDialogAsync("details", null, cancellationToken);
                                    break;
                                default:
                                    await turnContext.SendActivityAsync("I am unable to understand that. I am very sorry for not being able to assit you on this.");
                                    break;
                            } 
                        }

                        break;
                    }
                default:
                    {
                        await HandleActionsAsync(turnContext, cancellationToken);
                        break;
                    }
            }

            await _accessor.ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
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
