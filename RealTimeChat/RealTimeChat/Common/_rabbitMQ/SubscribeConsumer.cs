using Akka.Actor;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQApi;
using System;
using System.Text;

namespace RealTimeChat.Common
{
    public class SubscribeConsumer : IRabbitConsumer
    {
        private readonly IActorRef _consumerActor;
        private readonly ConnectionFactory _connectionFactory;
        private readonly string _exchangeName;

        private IConnection _connection;
        private IModel _channel;
        private string _userNickname;

        public SubscribeConsumer(IActorRef consumerActor, ConnectionFactory connectionFactory, string exchangeName, string nickname)
        {
            _consumerActor = consumerActor;
            _connectionFactory = connectionFactory;
            _exchangeName = exchangeName;
            _userNickname = nickname;

            Received();
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

                _channel.ExchangeDeclare(exchange: _exchangeName, type: "fanout", autoDelete: true);

                var queueName = _channel.QueueDeclare().QueueName;
                _channel.QueueBind(queue: queueName, exchange: _exchangeName, routingKey: "");

                var consumer = new EventingBasicConsumer(_channel);            
                consumer.Received += _messageReceivedHandle;

                _channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer, consumerTag:_userNickname);

            }
            catch (Exception ex)
            {
                //_logger.Error($"ConsumerActor RabbitMQ Connection Exception {ex.ToString()}");
            }
        }

        private void _messageReceivedHandle(object model, BasicDeliverEventArgs ea)
        {
            object jsonde = null;
            if (ea.BasicProperties.ContentType.Equals("application/json"))
                jsonde = _jsonDeSerialize(Encoding.UTF8.GetString(ea.Body));
            else
                jsonde = Encoding.UTF8.GetString(ea.Body);

            if (jsonde == null)
                return;

            _consumerActor.Tell(jsonde);
            //_logger.Info($"Subscribe Consumer Receive message from RabbitMQ_Server : {jsonde.GetType().ToString()}");
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
