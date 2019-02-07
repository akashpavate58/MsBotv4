using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MsBotv4.Bot
{
    public class ChatUserState
    {
        public string Username { get; set; }

        public static ChatUserState Default => new ChatUserState
        {
            Username = ""
        };
    }
}
