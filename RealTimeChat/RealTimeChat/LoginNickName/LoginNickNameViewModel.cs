using MvvmApi;
using RabbitMQ.Client;
using RealTimeChat.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace RealTimeChat.LoginNickName
{
    public class LoginNickNameViewModel : PropertyChangedHelper
    {
        private readonly ConnectionFactory _connectionFactory;
        private readonly string _exchange;

        public event EventHandler<string> EnterChatComplated;
        public ICommand ChatEnterCommand { get; set; }

        private string _nickname;
        public string Nickname
        {
            get { return _nickname; }
            set { SetField(ref _nickname, value, "Nickname"); }
        }

        public LoginNickNameViewModel(ConnectionFactory connectionFactory, string exchange)
        {
            _connectionFactory = connectionFactory;
            _exchange = exchange;
            InitializeCommands();
            InitializeControl();
        }

        private void InitializeControl()
        {
            
        }

        private void InitializeCommands()
        {
            ChatEnterCommand = new DelegateCommand(OnChatEnter);
        }

        private void OnChatEnter()
        {
            if(_checkNickName(Nickname))
            {
                MessageBox.Show($"{Nickname}은 이미 존재합니다.");
            } 
            else
            {
                EnterChatComplated(this, Nickname);
            }
        }

        private bool _checkNickName(string nickName)
        {
            var nickNames = RabbitRestApi.Create()
                                .SetHostName(_connectionFactory.HostName)
                                .SetPassword(_connectionFactory.Password)
                                .SetUserName(_connectionFactory.UserName)
                                .SetVirtualHost(_connectionFactory.VirtualHost)
                                .Get()
                                .ConsumerTagsOfExchange(_exchange);
            return nickNames.Contains(nickName);
        }
    }
}
