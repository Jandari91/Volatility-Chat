using MvvmApi;
using RabbitMQ.Client;
using RealTimeChat.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace RealTimeChat.ChatRoomList
{
    public class ChatRoomListViewModel : PropertyChangedHelper
    {
        private readonly ConnectionFactory _connectionFactory;

        public event EventHandler<string> EnterRoomComplated;
        public ICommand EnterRoomCommand { get; set; }
        public ICommand CreateRoomCommand { get; set; }
        public ICommand RefreshCommand { get; set; }

        private ObservableCollection<ChatRoomInfo> _chatRooms;
        public ObservableCollection<ChatRoomInfo> ChatRooms
        {
            get { return _chatRooms; }
            set { SetField(ref _chatRooms, value, "ChatRooms"); }
        }

        
        public ChatRoomInfo SelectedRoom { get; set; }

        private string _name;
        public string Name
        {
            get { return _name; }
            set { SetField(ref _name, value, "Name"); }
        }

        private string _people;
        public string People
        {
            get { return _people; }
            set { SetField(ref _people, value, "People"); }
        }

        public ChatRoomListViewModel(ConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;

            InitializeControls();
            InitializeCommands();
        }

        private void InitializeControls()
        {
            ChatRooms = new ObservableCollection<ChatRoomInfo>();

            //var rabbitRestApi = new ExchangeList(_connectionFactory);
            //var queues = rabbitRestApi.GetQueues();

            var queues = RabbitRestApi.Create()
                .SetHostName(_connectionFactory.HostName)
                .SetPassword(_connectionFactory.Password)
                .SetUserName(_connectionFactory.UserName)
                .SetVirtualHost(_connectionFactory.VirtualHost)
                .Get()
                .ConsumersOfExchangeList();
           
            foreach(var queue in queues)
            {
                ChatRooms.Add(new ChatRoomInfo()
                {
                    ChatRoomName = queue.Name,
                    NumOfTalkers = queue.Consumers.ToString()
                });
            }
        }

        private void InitializeCommands()
        {
            EnterRoomCommand = new DelegateCommand<object>(_ => OnEnterRoom(_));
            CreateRoomCommand = new DelegateCommand<string>(_ => OnCreateRoom(_));
            RefreshCommand = new DelegateCommand(OnRefresh);
        }

        private void OnRefresh()
        {
            InitializeControls();
        }

        private void OnCreateRoom(string chatRoomName)
        {
            EnterRoomComplated(this, chatRoomName);
        }

        private void OnEnterRoom(object _)
        {  
            SelectedRoom = _ as ChatRoomInfo;
            EnterRoomComplated(this, SelectedRoom.ChatRoomName);
        }
    }
}
