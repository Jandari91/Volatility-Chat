using Akka.Actor;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealTimeChat.Common
{
    public class QueueDeclareConsumer : IRabbitConsumer
    {
        private readonly IActorRef _consumerActor;
        private readonly ConnectionFactory _connectionFactory;
        private readonly string _queueName;

        private IConnection _connection;
        private IModel _channel;

        public QueueDeclareConsumer(IActorRef consumerActor, ConnectionFactory connectionFactory, string queueName)
        {
            _consumerActor = consumerActor;
            _connectionFactory = connectionFactory;
            _queueName = queueName;
        }

        public void Received()
        {
            var factory = _connectionFactory;

            // RabbitMQ Server Connection Retry
            factory.AutomaticRecoveryEnabled = true;
            factory.NetworkRecoveryInterval = TimeSpan.FromSeconds(10);
            factory.UseBackgroundThreadsForIO = true;

            try
            {
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                _channel.QueueDeclare(_queueName, false, false, true, null);

                _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
                var consumer = new EventingBasicConsumer(_channel);
                consumer.Received += _messageReceivedHandle;
                _channel.BasicConsume(_queueName, false, consumer);
            }
            catch (Exception ex)
            {
                //_logger.Error($"QueueDeclareConsumerActor RabbitMQ Connection Exception {ex.ToString()}");
            }
        }

        private void _messageReceivedHandle(object model, BasicDeliverEventArgs ea)
        {
            var jsonde = _jsonDeSerialize(Encoding.UTF8.GetString(ea.Body));
            _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            if (jsonde == null)
                return;

            _consumerActor.Tell(jsonde);
            //_logger.Info($"QueueDeclare Consumer Receive message from RabbitMQ_Server : {jsonde.GetType().ToString()}");

        }

        private object _jsonDeSerialize(string message)
        {
            try
            {
                object result = JsonConvert.DeserializeObject<object>(message, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All
                });
                return result;
            }
            catch (Exception ex)
            {
                //_logger.Error($"RabbitMQ Message json Deserialize Exception{ex.ToString()}");
                return null;
            }
        }

        public void Dispose()
        {
            _channel.Close();
            _connection.Close();
            _connection.Dispose();
            _channel.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
