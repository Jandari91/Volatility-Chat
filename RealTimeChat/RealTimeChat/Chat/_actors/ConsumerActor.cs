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
    public class ConsumerActor : ReceiveActor
    {
        private readonly ILoggingAdapter _logger;
        private readonly IActorRef _chatViewModelActor;
        private readonly string _guid;

        private readonly SubscribeConsumer _subScribeConsumer;
        private readonly ConnectionFactory _connectionFactory;
        private readonly string _exchangeName;
        private string _nickname;

        public static Props Props(IActorRef chatViewModelActor, ConnectionFactory connectionFactory, string exchangeName, string guid, string _userNickname)
        {
            return Akka.Actor.Props.Create(() => new ConsumerActor(chatViewModelActor, connectionFactory, exchangeName, guid, _userNickname));
        }

        public ConsumerActor(IActorRef chatViewModelActor, ConnectionFactory connectionFactory, string exchangeName, string guid, string _userNickname)
        {
            _logger = Context.GetLogger();
            _connectionFactory = connectionFactory;
            _chatViewModelActor = chatViewModelActor;
            _exchangeName = exchangeName;
            _guid = guid;
            _nickname = _userNickname;

            _subScribeConsumer = new SubscribeConsumer(Context.Self, _connectionFactory, _exchangeName, _nickname);

            RegisterMessageHandlers();
        }

        private void RegisterMessageHandlers()
        {
            Receive<SendEnterMessage>(_ => Handle(_));
            Receive<SendTextMessage>(_ => Handle(_));
            Receive<SendImageMessage>(_ => Handle(_));
            Receive<SendZipFileMessage>(_ => Handle(_));
        }

        protected override void PostStop()
        {
            _subScribeConsumer.Dispose();
            base.PostStop();
        }

        private void Handle(SendTextMessage msg)
        {
            if (!_guid.Equals(msg.Guid))
            {
                _chatViewModelActor.Tell(new ReceivedTextMessage()
                {
                    Msg = msg.Msg,
                    Nickname = msg.Nickname
                });
            }
        }

        private void Handle(SendImageMessage msg)
        {
            if (!_guid.Equals(msg.Guid))
            {
                _chatViewModelActor.Tell(new ReceivedImageMessage()
                {
                    Contents=msg.Contents,
                    Nickname = msg.Nickname
                });
            }
        }

        private void Handle(SendZipFileMessage msg)
        {
            if (!_guid.Equals(msg.Guid))
            {
                _chatViewModelActor.Tell(new ReceivedZipFileMessage()
                {
                    Contents = msg.Contents,
                    Nickname = msg.Nickname,
                    FileName = msg.FileName
                });
            }
        }

        private void Handle(SendEnterMessage msg)
        {
            _chatViewModelActor.Tell(new ReceivedEnterMessage() { Msg = msg.Msg});
        }
    }
}
