// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ContentmentBot.State;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace ProactiveBot
{
    /// <summary>
    /// Represents a bot that processes incoming activities.
    /// For each user interaction, an instance of this class is created and the OnTurnAsync method is called.
    /// This is a Transient lifetime service. Transient lifetime services are created
    /// each time they're requested. Objects that are expensive to construct, or have a lifetime
    /// beyond a single turn, should be carefully managed.
    /// For example, the <see cref="MemoryStorage"/> object and associated
    /// <see cref="IStatePropertyAccessor{T}"/> object are created with a singleton lifetime.
    /// </summary>
    /// <seealso cref="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-2.1"/>
    public class ProactiveBot : IBot
    {
        private const string ExampleWaterfallDialog = "exampleWaterfallDialog";
        private const string FirstPrompt = "firstPrompt";
        private const string SecondPrompt = "secondPrompt";
        private const string ThirdPrompt = "thirdPrompt";

        private readonly Accessors _accessors;
        private readonly DialogSet _dialogSet;
        private Activity externalActivity;

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>                        
        public ProactiveBot(Accessors accessors)
        {
            _dialogSet = new DialogSet(_accessors.DialogStateAccessor);


            _dialogSet.Add(new ChoicePrompt(FirstPrompt));
            _dialogSet.Add(new TextPrompt(SecondPrompt, Validator));
            _dialogSet.Add(new NumberPrompt<int>(ThirdPrompt, Validator));



            WaterfallStep[] stepsExampleWaterfallDialog = new WaterfallStep[]
            {
                            FirstPromptAsync,
                            SecondPromptAsync,
                            ThirdPromptAsync,
                            LastStepUserProfileWaterfallDialogAsync,
            };


            _dialogSet.Add(new WaterfallDialog(ExampleWaterfallDialog, stepsExampleWaterfallDialog));

        }

        /// <summary>
        /// Every conversation turn calls this method.
        /// </summary>
        /// <param name="turnContext">A <see cref="ITurnContext"/> containing all the data needed
        /// for processing this conversation turn. </param>
        /// <param name="cancellationToken">(Optional) A <see cref="CancellationToken"/> that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> that represents the work queued to execute.</returns>
        /// <seealso cref="BotStateSet"/>
        /// <seealso cref="ConversationState"/>
        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Greet user with options menu. Runs only when the conversation is initiated.
            if (turnContext.Activity.Type == ActivityTypes.ConversationUpdate)
            {
                if (turnContext.Activity.MembersAdded != null)
                {
                    foreach (var member in turnContext.Activity.MembersAdded)
                    {
                        if (member.Id != turnContext.Activity.Recipient.Id)
                        {
                            await turnContext.SendActivityAsync("Welcome");
                            await turnContext.SendActivityAsync("Send \"1\" to set this conversation as the receiving conversation (store ConversationReference)");
                            await turnContext.SendActivityAsync("Send \"2\" to start a dialog.");
                            await turnContext.SendActivityAsync("Send \"3[your message]\" to send a message.");
                        }
                    }
                }
            }

            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                // Set this conversation as the receiving conversation (store ConversationReference in ConversationReference.json)
                if (turnContext.Activity.Text == "1")
                {
                    ConversationReference conversationReference = turnContext.Activity.GetConversationReference();
                    //ConversationReference conversationReference = DownloadSerializedJSONData<ConversationReference>("https://api.myjson.com/bins/14v9uw");
                    File.WriteAllText(@"./ConversationReference.json", JsonConvert.SerializeObject(conversationReference));
                }

                // Send the text after "2" as a message.
                else if (turnContext.Activity.Text.StartsWith("2"))
                {
                    // Get conversationreference from file
                    ConversationReference conversationReference = JsonConvert.DeserializeObject<ConversationReference>(File.ReadAllText(@"./ConversationReference.json"));

                    // Store the activity as a member variable
                    externalActivity = turnContext.Activity;

                    // Use the ContinueConversationAsync method on the BotAdapter and pass the receiving conversation's ConversationReference
                    await turnContext.Adapter.ContinueConversationAsync("ProactiveBot", conversationReference, CreateCallbackMessage(), cancellationToken);
                }

                // Activate a dialog inside the receiving conversation
                else if (turnContext.Activity.Text.StartsWith("3"))
                {
                    // Get conversationreference from file
                    ConversationReference conversationReference = JsonConvert.DeserializeObject<ConversationReference>(File.ReadAllText(@"./ConversationReference.json"));

                    // Store the activity as a member variable
                    externalActivity = turnContext.Activity;

                    // Use the ContinueConversationAsync method on the BotAdapter and pass the receiving conversation's ConversationReference
                    await turnContext.Adapter.ContinueConversationAsync("123", conversationReference, CreateCallbackDialog(), cancellationToken);
                }

                //else
                //{
                //    DialogContext dc = await _dialogSet.CreateContextAsync(turnContext, cancellationToken);

                //    if (dc.Stack.Exists(dialog => dialog.Id == ExampleWaterfallDialog))
                //    {
                //        await dc.BeginDialogAsync(ExampleWaterfallDialog, null, cancellationToken);
                //    }
                //}

                // if dialog stack heeft een dialog 
                // start dialog
            }
        }

        // BotCallbackHandler for sending a message.
        private BotCallbackHandler CreateCallbackMessage()
        {
            return async (turnContext, token) =>
            {
                await turnContext.SendActivityAsync(this.externalActivity.Text.Substring(1));
            };
        }

        // BotCallBackHandler for activating a dialog
        private BotCallbackHandler CreateCallbackDialog()
        {
            return async (turnContext, cancellationToken) =>
            {
                // Activate ExampleWaterfall dialog
                    DialogContext dc = await _dialogSet.CreateContextAsync(turnContext, cancellationToken);
                    await dc.BeginDialogAsync(ExampleWaterfallDialog, null, cancellationToken);
            };
        }

        private async Task<DialogTurnResult> FirstPromptAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //// [Template] store result from the previous step.
            //stepContext.Values["result"] = stepContext.Result;

            return await stepContext.PromptAsync(
                FirstPrompt,
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Make a choice."),
                    RetryPrompt = MessageFactory.Text("Try making a choice again."),
                    Choices = ChoiceFactory.ToChoices(new List<string> { "Yes", "No" }),
                },
                cancellationToken);
        }

        private async Task<DialogTurnResult> SecondPromptAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Prompt for the users name. The result of the prompt is returned to the next step of the waterfall.
            return await stepContext.PromptAsync(SecondPrompt, new PromptOptions
            {
                Prompt = MessageFactory.Text("This is a prompt"),
                RetryPrompt = MessageFactory.Text("Please try again")
            },
            cancellationToken
            );
        }

        private async Task<bool> Validator(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            // Validator will always return true for simplicity
            return await Task.FromResult(true);
        }
    }
}