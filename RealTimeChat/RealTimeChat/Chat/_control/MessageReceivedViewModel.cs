using Microsoft.Win32;
using MvvmApi;
using MvvmDialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace RealTimeChat.Chat
{
    public class MessageReceivedViewModel : PropertyChangedHelper, IMessage
    {
        private readonly IDialogService _dialogService;
        public string Message { get; set; }

        public string MessageTime { get; set; }

        public string Nickname { get; set; }

        public byte[] FileData { get; set; }

        public Visibility TextMessageVisible { get; set; }

        public Visibility ImageMessageVisible { get; set; }

        public ICommand SaveImageCommand { get; set; }
        public ICommand SaveZipFileCommand { get; set; }
        public ICommand ViewImageCommand { get; set; }

        public MessageReceivedViewModel()
        {
            InitializeCommands();
            _dialogService = new DialogService();
        }

        private void InitializeCommands()
        {
            SaveImageCommand = new DelegateCommand(OnSaveImage);
            SaveZipFileCommand = new DelegateCommand(OnSaveFile);
            ViewImageCommand = new DelegateCommand(OnViewImage);
        }

        private void OnViewImage()
        {
            var dialogViewModel = new ImageViewDialogViewModel();
            dialogViewModel.FileData = FileData;

            bool? success = _dialogService.ShowDialog(this, dialogViewModel);
            if(success == true)
            {

            }
        }

        private void OnSaveFile()
        {
            var imageContents = ChatViewModel.SelectedMessage as MessageReceivedViewModel;

            byte[] data = imageContents.FileData;

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "ZIP File | *.ZIP";
            saveFileDialog.RestoreDirectory = true;

            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName;
                File.WriteAllBytes(filePath, data);
            }
        }

        private void OnSaveImage()
        {
            var imageContents = ChatViewModel.SelectedMessage as MessageReceivedViewModel;

            byte[] data = imageContents.FileData;

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter= "JPEG File | *.JPG";
            saveFileDialog.RestoreDirectory = true;

            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName;
                File.WriteAllBytes(filePath, data);
            }
        }
    }
}
