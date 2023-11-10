using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealTimeChat.Common
{
    public class QueueDeclareProducer : IRabbitProducer
    {
        private readonly ConnectionFactory _rabbitMQConnection;
        private readonly string _queueName;

        public QueueDeclareProducer(ConnectionFactory rabbitMQConnection, string queueName)
        {
            _rabbitMQConnection = rabbitMQConnection;
            _queueName = queueName;
        }

        public void SendMessage(object message)
        {
            var factory = _rabbitMQConnection;

            try
            {
                using (var connection = factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(_queueName, false, false, true, null);
                    var body = Encoding.UTF8.GetBytes(_jsonSerialize(message));
                    var properties = _setMessageProperties(channel);
                    channel.BasicPublish("", _queueName, properties, body);
                }
            }
            catch (Exception ex)
            {
                //_logger.Error($"QueueDeclareProduce RabbitMQ Connection Exception {ex.ToString()}");
            }
        }

        private IBasicProperties _setMessageProperties(IModel model)
        {
            var props = model.CreateBasicProperties();
            props.ContentType = "application/json";
            props.DeliveryMode = 1;
            props.Expiration = "100000";
            return props;
        }

        private string _jsonSerialize(object message)
        {
            try
            {
                string result = JsonConvert.SerializeObject(message, Formatting.Indented, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Objects,
                    TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Full
                });

                return result;
            }
            catch (Exception ex)
            {
                //_logger.Error($"RabbitMQ Message json Serialize Exception{ex.ToString()}");
                return String.Empty;
            }
        }

        public void SendFile(string filePath)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
