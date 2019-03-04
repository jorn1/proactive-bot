using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContentmentBot.State
{
    public class Contentment
    {
        public FoundChoice Commute { get; set; }
        public FoundChoice Schedule { get; set; }
        public string Coffee { get; set; }
    } 
}
