using Akka.Actor;
using Akka.Event;
using Messages;
using RabbitMQ.Client;
using RealTimeChat.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealTimeChat.Chat
{
    public class ProducerActor : ReceiveActor
    {
        private readonly ILoggingAdapter _logger;
        private readonly ConnectionFactory _connectionFactory;
        private readonly string _exchangeName;

        public static Props Props(ConnectionFactory connectionFactory, string exchangeName)
        {
            return Akka.Actor.Props.Create(() => new ProducerActor(connectionFactory, exchangeName));
        }

        public ProducerActor(ConnectionFactory connectionFactory, string exchangeName)
        {
            _logger = Context.GetLogger();
            _connectionFactory = connectionFactory;
            _exchangeName = exchangeName;

            RegisterMessageHandlers();
        }

        private void RegisterMessageHandlers()
        {
            Receive<SendTextMessage>(_ => Handle(_));
            Receive<SendEnterMessage>(_ => Handle(_));
            Receive<SendImageMessage>(_ => Handle(_));
            Receive<SendZipFileMessage>(_ => Handle(_));
        }

        private void Handle(SendTextMessage msg)
        {
            using (var produce = new PublishProducer(_connectionFactory, _exchangeName))
            {
                produce.SendMessage(msg);
            }
        }

        private void Handle(SendEnterMessage msg)
        {
            using (var produce = new PublishProducer(_connectionFactory, _exchangeName))
            {
                produce.SendMessage(msg);
            }
        }

        private void Handle(SendImageMessage msg)
        {
            using (var produce = new PublishProducer(_connectionFactory, _exchangeName))
            {
                produce.SendMessage(msg);
            }
        }

        private void Handle(SendZipFileMessage msg)
        {
            using (var produce = new PublishProducer(_connectionFactory, _exchangeName))
            {
                produce.SendMessage(msg);
            }
        }
    }
}
