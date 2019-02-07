using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MsBotv4.Bot
{
    public class ChatbotState
    {
        public string NameOfTheUser { get; set; }
        public string SpokenLanguage { get; set; }

        public static ChatbotState Default => new ChatbotState {
            NameOfTheUser = "",
            SpokenLanguage = Constants.DefaultLanguage
        };
    }
}
