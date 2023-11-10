using Akka.Actor;
using Akka.Configuration;
using Akka.Event;
using MvvmApi;
using RabbitMQ.Client;
using RealTimeChat.Chat;
using RealTimeChat.ChatRoomList;
using RealTimeChat.Common;
using RealTimeChat.LoginNickName;
using Squirrel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace RealTimeChat
{
    public class MainViewModel : PropertyChangedHelper
    {
        private Config _config;
        private ActorSystem _system;
        private ILoggingAdapter _logger;
        private ConnectionFactory _connectionFactory;
        private string _chatRoomName;
        private string _nickName;

        private ChatViewModel _chatViewModel { get; set; }
        private ChatRoomListViewModel _chatRoomListViewModel { get; set; }
        private LoginNickNameViewModel _loginNickNameViewModel { get; set; }

        public ICommand WindowLoadedCommand { get; set; }

        private object _currentPage;
        public object CurrentPage
        {
            get { return _currentPage; }
            set { SetField(ref _currentPage, value, "CurrentPage"); }
        }

        public MainViewModel()
        {
            InitializeControls();
            InitializeCommand();
        }

        private void InitializeCommand()
        {
            WindowLoadedCommand = new DelegateCommand(_mainViewLoadedCommandAction);
        }

        private void InitializeControls()
        {
            _config = _createConfig();
            var results = _createActorSystem(_config);
            _system = results.System;
            _logger = results.Logger;
            _connectionFactory = _createRabbitMqConnectionFactory(_config);

            _chatRoomListViewModel = new ChatRoomListViewModel(_connectionFactory);
            _chatRoomListViewModel.EnterRoomComplated += _enterRoom;

            CurrentPage = _chatRoomListViewModel;
        }

        private void _enterRoom(object sender, string chatRoomName)
        {
            _chatRoomName = chatRoomName;
            _loginNickNameViewModel = new LoginNickNameViewModel(_connectionFactory, chatRoomName);
            _loginNickNameViewModel.EnterChatComplated += _enterChat;
            CurrentPage = _loginNickNameViewModel;
        }

        private void _enterChat(object sender, string nickName)
        {
            _nickName = nickName;

            _chatViewModel = new ChatViewModel(_chatRoomName, _nickName, _system, _connectionFactory);
            CurrentPage = _chatViewModel;
        }

        private Config _createConfig()
        {
            return HoconHelper.ReadConfigurationFromHoconFile(Assembly.GetExecutingAssembly());
        }

        private ConnectionFactory _createRabbitMqConnectionFactory(Config config)
        {
            ConnectionFactory factory = new ConnectionFactory()
            {
                HostName = config.GetString("rabbitmq.connection.hostname"),
                VirtualHost = config.GetString("rabbitmq.connection.vhost"),
                UserName = config.GetString("rabbitmq.connection.username"),
                Password = config.GetString("rabbitmq.connection.password")
            };

            try
            {
                using (var connection = factory.CreateConnection())
                {
                    _logger.Info($"RabbitMQ Connection OK. HostName = {factory.HostName}" +
                    $", VirtualHost={factory.VirtualHost}" +
                    $", UserName={factory.UserName}" +
                    $", Password={factory.Password}");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"RabbitMQ Connection Error." +
                    $"HostName = {factory.HostName}" +
                    $",VirtualHost={factory.VirtualHost}" +
                    $",UserName={factory.UserName}" +
                    $",Password={factory.Password}" +
                    $",Exception={ex.ToString()}");
            }

            return factory;
        }

        private (ActorSystem System, ILoggingAdapter Logger) _createActorSystem(Config config)
        {
            var system = ActorSystem.Create("actorSystem", config);
            var logger = system.Log;
            return (system, logger);
        }

        private async void _mainViewLoadedCommandAction()
        {
            //using (var updateManager = new UpdateManager(@"D:\Squirrel_Test\Squirrel_Test\nuget\Releasify"))
            //{
            //    var releaseEntry = await updateManager.UpdateApp();
            //}
        }
    }
}
