using Akka.Actor;
using Messages;
using Microsoft.Win32;
using MvvmApi;
using RabbitMQ.Client;
using RealTimeChat.Common.Control;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace RealTimeChat.Chat
{
    public class ChatViewModel : PropertyChangedHelper
    {
        private readonly ConnectionFactory _connectionFactory;
        private readonly ActorSystem _actorSystem;
        private readonly IActorRef _chatViewModelActor;
        private readonly string _guid;

        public ICommand MessageSendCommand { get; set; }
        public ICommand ImageSendCommand { get; set; }
        public ICommand FileSendCommand { get; set; }


        private RangeObservableCollection<string> _usersInRoom;
        public RangeObservableCollection<string> UsersInRoom
        {
            get { return _usersInRoom; }
            set { SetField(ref _usersInRoom, value, "UsersInRoom"); }
        }

        private ObservableCollection<IMessage> _messages;
        public ObservableCollection<IMessage> Messages
        {
            get { return _messages; }
            set { SetField(ref _messages, value, "Messages"); }
        }

        private string _userNickname;
        public string UserNickname
        {
            get { return _userNickname; }
            set { SetField(ref _userNickname, value, "UserNickname"); }
        }

        private string _roomName;
        public string RoomName
        {
            get { return _roomName; }
            set { SetField(ref _roomName, value, "RoomName"); }
        }

        private string _roomPeople;
        public string RoomPeople
        {
            get { return _roomPeople; }
            set { SetField(ref _roomPeople, value, "RoomPeople"); }
        }

        private string _inputMessage;
        public string InputMessage
        {
            get { return _inputMessage; }
            set { SetField(ref _inputMessage, value, "InputMessage"); }
        }

        public static IMessage SelectedMessage { get; set; }

        public ChatViewModel(string chatRoomName, string nickName, ActorSystem actorSystem, ConnectionFactory connectionFactory)
        {
            RoomName = chatRoomName;
            UserNickname = nickName;
            _connectionFactory = connectionFactory;
            _actorSystem = actorSystem;
            

            InitializeControls();
            InitializeCommands();

            _guid = Guid.NewGuid().ToString();
            _chatViewModelActor = actorSystem.ActorOf(ChatViewModelActor.Props(this, _connectionFactory, RoomName, _guid, UserNickname), nameof(ChatViewModelActor));
            _chatViewModelActor.Tell(new SendEnterMessage()
            {
                Msg = $"{UserNickname}님이 입장했습니다."
            });
        }

        private void InitializeControls()
        {
            UsersInRoom = new RangeObservableCollection<string>();
            Messages = new ObservableCollection<IMessage>();
        }

        private void InitializeCommands()
        {
            MessageSendCommand = new DelegateCommand(OnMessageSend);
            ImageSendCommand = new DelegateCommand(OnImageSend);
            FileSendCommand = new DelegateCommand(OnFileSend);
        }

        private void OnFileSend()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Zip File | *.zip";
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == true)
            {
                DateTime nowTime = DateTime.Now;
                var sentMessageTime = nowTime.ToString("ddd") + " " + DateTime.Now.ToShortTimeString();

                string filePath = openFileDialog.FileName;
                byte[] fileContents = File.ReadAllBytes(filePath);
                string fileName = openFileDialog.SafeFileName;

                Messages.Add(new MessageSentViewModel()
                {
                    Message = fileName,
                    MessageTime = sentMessageTime,
                    FileData = fileContents,
                    TextMessageVisible = Visibility.Visible,
                    ImageMessageVisible = Visibility.Collapsed
                });
                _chatViewModelActor.Tell(new SendZipFileMessage()
                {
                    Guid = _guid,
                    Contents = fileContents,
                    Nickname = UserNickname,
                    FileName = fileName
                });
            }
        }

        private void OnImageSend()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "JPEG File | *.JPG";
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == true)
            {
                DateTime nowTime = DateTime.Now;
                var sentMessageTime = nowTime.ToString("ddd") + " " + DateTime.Now.ToShortTimeString();

                string filePath = openFileDialog.FileName;
                byte[] fileContents = File.ReadAllBytes(filePath);

                Messages.Add(new MessageSentViewModel()
                {
                    Message = "사진",
                    MessageTime = sentMessageTime,
                    FileData = fileContents,
                    TextMessageVisible = Visibility.Collapsed,
                    ImageMessageVisible=Visibility.Visible
                });
                _chatViewModelActor.Tell(new SendImageMessage()
                {
                    Guid = _guid,
                    Contents = fileContents,
                    Nickname = UserNickname
                });
            }          
        }

        private void OnMessageSend()
        {
            if(InputMessage == null || InputMessage == string.Empty)
            {
                MessageBox.Show("메시지를 입력하세요", "Warning");
                return;
            }
            DateTime nowTime = DateTime.Now;
            var sentMessageTime = nowTime.ToString("ddd") + " " + DateTime.Now.ToShortTimeString();

            Messages.Add(new MessageSentViewModel()
            {
                Message = InputMessage,
                MessageTime = sentMessageTime,
                TextMessageVisible = Visibility.Visible,
                ImageMessageVisible = Visibility.Collapsed
            });
            _chatViewModelActor.Tell(new SendTextMessage()
            {
                Guid = _guid,
                Msg = InputMessage,
                Nickname = UserNickname
            });

            InputMessage = string.Empty;
        }

        public void ReceivedEnterMessage(ReceivedEnterMessage msg)
        {
            DateTime nowTime = DateTime.Now;
            var receivedMessageTime = nowTime.ToString("ddd") + " " + DateTime.Now.ToShortTimeString();

            Messages.Add(new MessageEnteredViewModel()
            {
                Message = msg.Msg,
                MessageTime = receivedMessageTime
            });
        }

        public void ReceivedTextMessage(ReceivedTextMessage msg)
        {
            DateTime nowTime = DateTime.Now;
            var receivedMessageTime = nowTime.ToString("ddd") + " " + DateTime.Now.ToShortTimeString();

            Messages.Add(new MessageReceivedViewModel()
            {
                Message = msg.Msg,
                MessageTime = receivedMessageTime,
                Nickname = msg.Nickname,
                TextMessageVisible = Visibility.Visible,
                ImageMessageVisible = Visibility.Collapsed
            });
        }

        public void ReceivedImageMessage(ReceivedImageMessage msg)
        {
            DateTime nowTime = DateTime.Now;
            var receivedMessageTime = nowTime.ToString("ddd") + " " + DateTime.Now.ToShortTimeString();

            Messages.Add(new MessageReceivedViewModel()
            {
                Message = "사진",
                FileData = msg.Contents,
                MessageTime = receivedMessageTime,
                Nickname = msg.Nickname,
                TextMessageVisible = Visibility.Collapsed,
                ImageMessageVisible = Visibility.Visible
            });
        }

        public void ReceivedZipMessage(ReceivedZipFileMessage msg)
        {
            DateTime nowTime = DateTime.Now;
            var receivedMessageTime = nowTime.ToString("ddd") + " " + DateTime.Now.ToShortTimeString();

            Messages.Add(new MessageReceivedViewModel()
            {
                Message = msg.FileName,
                FileData = msg.Contents,
                MessageTime = receivedMessageTime,
                Nickname = msg.Nickname,
                TextMessageVisible = Visibility.Visible,
                ImageMessageVisible = Visibility.Collapsed
            });
        }
    }
}
