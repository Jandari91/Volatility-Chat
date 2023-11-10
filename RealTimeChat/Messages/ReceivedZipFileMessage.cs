using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messages
{
    public class ReceivedZipFileMessage
    {
        public string Guid;
        public byte[] Contents;
        public string Nickname;
        public string FileName;
    }
}
