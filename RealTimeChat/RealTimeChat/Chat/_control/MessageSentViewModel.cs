using MvvmApi;
using MvvmDialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace RealTimeChat.Chat
{
    public class MessageSentViewModel : PropertyChangedHelper, IMessage
    {
        private readonly IDialogService _dialogService;
        public string Message { get; set; }

        public string MessageTime { get; set; }

        public string Nickname { get; set; }

        public byte[] FileData { get; set; }

        public Visibility TextMessageVisible { get; set; }

        public Visibility ImageMessageVisible { get; set; }

        public ICommand ViewImageCommand { get; set; }

        public MessageSentViewModel()
        {
            InitializeCommands();
            _dialogService = new DialogService();
        }

        private void InitializeCommands()
        {
            ViewImageCommand = new DelegateCommand(OnViewImage);
        }

        private void OnViewImage()
        {
            var dialogViewModel = new ImageViewDialogViewModel();
            dialogViewModel.FileData = FileData;

            bool? success = _dialogService.ShowDialog(this, dialogViewModel);
            if (success == true)
            {

            }
        }
    }
}
