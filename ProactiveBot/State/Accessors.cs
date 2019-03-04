using ContentmentBot.State;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContentmentBot.State
{
    public class Accessors
    {
        public Accessors(Microsoft.Bot.Builder.ConversationState conversationState, Microsoft.Bot.Builder.UserState userState)
        {
            ConversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));
            UserState = userState ?? throw new ArgumentNullException(nameof(userState));
        }

        public static string UserProfileName { get; } = "UserProfile";
        public Microsoft.Bot.Builder.IStatePropertyAccessor<UserProfile> UserProfileAccessor { get; set; }
        public Microsoft.Bot.Builder.UserState UserState { get; }

        public static string ConversationDataName { get; } = "ConversationData";
        public Microsoft.Bot.Builder.IStatePropertyAccessor<ConversationData> ConversationDataAccessor { get; set; }
        public Microsoft.Bot.Builder.ConversationState ConversationState { get; }

        public static string DialogStateName { get; } = "DialogPromptBotAccessors.DialogState";
        public IStatePropertyAccessor<DialogState> DialogStateAccessor { get; set; }

        public static string ContentmentName { get; } = "Contentment";
        public IStatePropertyAccessor<Contentment> ContentmentAccessor { get; set; }
    }
}