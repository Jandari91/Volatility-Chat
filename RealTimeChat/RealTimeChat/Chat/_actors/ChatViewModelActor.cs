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
using System.Windows;

namespace RealTimeChat.Chat
{
    public class ChatViewModelActor : ReceiveActor
    {
        private readonly ILoggingAdapter _logger;
        private readonly ChatViewModel _chatViewModel;
        private readonly ConnectionFactory _connectionFactory;
        private readonly IActorRef _producerActor;
        private readonly IActorRef _consumerActor;
        private readonly string _exchangeName;
        private readonly ICancelable _schedule;
        private readonly string _guid;
        private readonly string _userNickname;

        public static Props Props(ChatViewModel chatViewModel, ConnectionFactory connectionFactory, string exchangeName, string guid, string userNickname)
        {
            return Akka.Actor.Props.Create(() => new ChatViewModelActor(chatViewModel, connectionFactory, exchangeName, guid,userNickname));
        }

        public ChatViewModelActor(ChatViewModel chatViewModel, ConnectionFactory connectionFactory, string exchangeName, string guid, string userNickname)
        {
            _chatViewModel = chatViewModel;
            _connectionFactory = connectionFactory;
            _exchangeName = exchangeName;
            _guid = guid;
            _userNickname = userNickname;

            _producerActor = Context.ActorOf(ProducerActor.Props(_connectionFactory, _exchangeName), nameof(ProducerActor));
            _consumerActor = Context.ActorOf(ConsumerActor.Props(Context.Self, _connectionFactory, _exchangeName, _guid, _userNickname), nameof(ConsumerActor));

            _schedule = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(
                TimeSpan.Zero,
                TimeSpan.FromSeconds(10),         // 메시지 간격(시간)
                Self,              // 메시지 보내는 액터
                new FindRoomPepleMessage(),  // 메시지 
                Self);             // 메시지 받는 액터

            RegisterMessageHandlers();
        }

        private void RegisterMessageHandlers()
        {
            Receive<SendTextMessage>(_ => Handle(_));
            Receive<SendImageMessage>(_ => Handle(_));
            Receive<SendZipFileMessage>(_ => Handle(_));
            Receive<FindRoomPepleMessage>(_ => Handle(_));

            Receive<SendEnterMessage>(_ => Handle(_));
            Receive<ReceivedTextMessage>(_ => Handle(_));
            Receive<ReceivedEnterMessage>(_ => Handle(_));
            Receive<ReceivedImageMessage>(_ => Handle(_));
            Receive<ReceivedZipFileMessage>(_ => Handle(_));
        }

        private void Handle(FindRoomPepleMessage msg)
        {
            //var restApi = new ExchangeList(_connectionFactory);
            //var usersInRoom = restApi.GetConsumerTagsOfExchange(_exchangeName);

            var usersInRoom = RabbitRestApi.Create()
                .SetHostName(_connectionFactory.HostName)
                .SetPassword(_connectionFactory.Password)
                .SetUserName(_connectionFactory.UserName)
                .SetVirtualHost(_connectionFactory.VirtualHost)
                .Get()
                .ConsumerTagsOfExchange(_exchangeName);

            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                _chatViewModel.RoomPeople = RabbitRestApi.Create()
                                .SetHostName(_connectionFactory.HostName)
                                .SetPassword(_connectionFactory.Password)
                                .SetUserName(_connectionFactory.UserName)
                                .SetVirtualHost(_connectionFactory.VirtualHost)
                                .Get()
                                .ConsumersOfExchange(_exchangeName).Consumers.ToString();
                _chatViewModel.UsersInRoom.Clear();
                _chatViewModel.UsersInRoom.AddRange(usersInRoom);
            });
            
        }

        private void Handle(SendEnterMessage msg)
        {
            _producerActor.Tell(msg);
        }

        private void Handle(ReceivedEnterMessage msg)
        {
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                _chatViewModel.ReceivedEnterMessage(msg);
            });
        }

        private void Handle(SendTextMessage msg)
        {
            _producerActor.Tell(msg);
        }

        private void Handle(SendImageMessage msg)
        {
            _producerActor.Tell(msg);
        }

        private void Handle(SendZipFileMessage msg)
        {
            _producerActor.Tell(msg);
        }

        private void Handle(ReceivedTextMessage msg)
        {
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                _chatViewModel.ReceivedTextMessage(msg);
            });
        }

        private void Handle(ReceivedImageMessage msg)
        {
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                _chatViewModel.ReceivedImageMessage(msg);
            });
        }

        private void Handle(ReceivedZipFileMessage msg)
        {
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                _chatViewModel.ReceivedZipMessage(msg);
            });
        }
    }
}
