// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
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
        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>                        
        public ProactiveBot()
        {
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
                            await turnContext.SendActivityAsync("Send \"[your message]\" to send a message to the receiving conversation.");
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

                // Send Activity.Text to receiving conversation (stored ConversationReference). 
                else
                {
                    // Get stored ConversationReference from file.
                    ConversationReference conversationReference = JsonConvert.DeserializeObject<ConversationReference>(File.ReadAllText(@"./ConversationReference.json"));

                    // Use the ContinueConversationAsync method on the BotAdapter and pass the receiving conversation's ConversationReference
                    await turnContext.Adapter.ContinueConversationAsync("ProactiveBot", conversationReference, CreateCallbackMessage(turnContext.Activity.Text), cancellationToken);
                }
            }
        }

        // BotCallbackHandler for sending a message.
        private BotCallbackHandler CreateCallbackMessage(string message)
        {
            return async (turnContext, token) =>
            {
                await turnContext.SendActivityAsync(message);
            };
        }
    }
}