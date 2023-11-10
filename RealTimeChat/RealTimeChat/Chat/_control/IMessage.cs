using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealTimeChat.Chat
{
    public interface IMessage
    {
        string Message { get; set; }
        string MessageTime { get; set; }
        string Nickname { get; set; }
    }
}
