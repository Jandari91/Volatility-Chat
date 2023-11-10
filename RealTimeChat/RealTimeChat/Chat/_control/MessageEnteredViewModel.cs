using MvvmApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealTimeChat.Chat
{
    public class MessageEnteredViewModel : IMessage
    {
        public string Message { get; set; }

        public string MessageTime { get; set; }

        public string Nickname { get; set; }
    }
}
