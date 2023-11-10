using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQApi
{
    public interface IRabbitProducer : IDisposable
    {
        void SendMessage(object message);
        void SendFile(string filePath);
    }
}
