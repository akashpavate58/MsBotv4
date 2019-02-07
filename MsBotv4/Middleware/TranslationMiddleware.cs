using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using MsBotv4.Bot;
using MsBotv4.Services.Translation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MsBotv4.Middleware
{
    public class TranslationMiddleware : IMiddleware
    {
        private readonly MicrosoftTranslator _translator;
        private readonly ChatbotStateAccessor _accessor;
        private DialogSet _dialogs;

        public TranslationMiddleware(MicrosoftTranslator translator, ChatbotStateAccessor accessor)
        {
            _translator = translator ?? throw new ArgumentNullException(nameof(translator));
            _accessor = accessor ?? throw new System.ArgumentNullException(nameof(accessor));

            _dialogs = new DialogSet(_accessor.ConversationDialogState);
        }

        /*
         Please change OnTurnAsync logic to better suit your dialog needs. 
             */
        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                var dialogContext = await _dialogs.CreateContextAsync(turnContext, cancellationToken);
                if (dialogContext.ActiveDialog == null)
                {
                    var model = await _translator.TranslateAsync(turnContext.Activity.Text, Constants.DefaultLanguage, cancellationToken);

                    var botState = await _accessor.FetchStateAsync(turnContext);
                    botState.SpokenLanguage = model.DetectedLanguage;
                    await _accessor.SaveStateAsync(turnContext, botState);

                    turnContext.Activity.Text = model.Text;

                    turnContext.OnSendActivities(HandleBotResponses);
                    turnContext.OnUpdateActivity(HandleBotResponse);
                }
                else
                {
                    dialogContext.Context.OnSendActivities(HandleBotResponses);
                    dialogContext.Context.OnUpdateActivity(HandleBotResponse);
                }
            }

            await next(cancellationToken).ConfigureAwait(false);
        }

        #region TranslationEventHandlers
        private async Task<ResourceResponse> HandleBotResponse(ITurnContext turnContext, Activity activity, Func<Task<ResourceResponse>> next)
        {
            // Translate messages sent to the user to user language
            if (activity.Type == ActivityTypes.Message)
            {
                var botState = await _accessor.FetchStateAsync(turnContext);
                if (botState.SpokenLanguage != Constants.DefaultLanguage)
                {
                    await TranslateMessageActivityAsync(activity.AsMessageActivity(), botState.SpokenLanguage);
                }
            }

            return await next();
        }

        private async Task<ResourceResponse[]> HandleBotResponses(ITurnContext turnContext, List<Activity> activities, Func<Task<ResourceResponse[]>> next)
        {
            var botState = await _accessor.FetchStateAsync(turnContext);
            // Translate messages sent to the user to user language
            if (botState.SpokenLanguage != Constants.DefaultLanguage)
            {
                List<Task> tasks = new List<Task>();
                foreach (Activity currentActivity in activities.Where(a => a.Type == ActivityTypes.Message))
                {
                    tasks.Add(TranslateMessageActivityAsync(currentActivity.AsMessageActivity(), botState.SpokenLanguage));
                }

                if (tasks.Any())
                {
                    await Task.WhenAll(tasks).ConfigureAwait(false);
                }
            }

            return await next();
        }

        private async Task TranslateMessageActivityAsync(IMessageActivity activity, string targetLocale, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (activity.Type == ActivityTypes.Message)
            {
                activity.Text = (await _translator.TranslateAsync(activity.Text, targetLocale)).Text;
            }
        }
        #endregion
    }
}
