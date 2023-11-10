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
    public class PublishProducer : IRabbitProducer
    {
        private readonly ConnectionFactory _rabbitMQConnection;
        private readonly string _exchangeName;

        public PublishProducer(ConnectionFactory rabbitMQConnection, string exchangeName)
        {
            _rabbitMQConnection = rabbitMQConnection;
            _exchangeName = exchangeName;
        }

        public void SendMessage(object message)
        {
            var factory = _rabbitMQConnection;

            // RabbitMQ Server Connection Retry
            factory.AutomaticRecoveryEnabled = true;
            factory.NetworkRecoveryInterval = TimeSpan.FromSeconds(10);

            try
            {
                using (var connection = factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare(exchange: _exchangeName, type: "fanout", autoDelete:true);
                    
                    var body = Encoding.UTF8.GetBytes(_jsonSerialize(message));

                    var properties = _setMessageExpiration(channel);
                    channel.BasicPublish(exchange: _exchangeName, routingKey: "", basicProperties: properties, body: body);
                }
            }
            catch (Exception ex)
            {
                //_logger.Error($"PublishProduce RabbitMQ Connection Exception {ex.ToString()}");
            }
        }

        public void SendFile(string filePath)
        {
            throw new NotImplementedException();
        }

        private string _jsonSerialize(object message)
        {
            string result = JsonConvert.SerializeObject(message, Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Full
            });
            return result;
        }

        private IBasicProperties _setMessageExpiration(IModel model)
        {
            var props = model.CreateBasicProperties();
            props.ContentType = "application/json";
            props.DeliveryMode = 1;
            props.Expiration = "100000";
            return props;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
