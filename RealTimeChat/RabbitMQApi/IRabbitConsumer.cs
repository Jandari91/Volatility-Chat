using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQApi
{
    public interface IRabbitConsumer : IDisposable
    {
        void Received();
    }
}
