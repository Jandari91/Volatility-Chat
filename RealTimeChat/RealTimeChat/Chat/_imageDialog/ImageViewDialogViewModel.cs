using MvvmApi;
using MvvmDialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealTimeChat.Chat
{
    public class ImageViewDialogViewModel : PropertyChangedHelper, IModalDialogViewModel
    {
        private bool? _dialogResult;
        public bool? DialogResult
        {
            get { return _dialogResult; }
            set { SetField(ref _dialogResult, value, "DialogResult"); }
        }

        private byte[] _fileData;
        public byte[] FileData
        {
            get { return _fileData; }
            set { SetField(ref _fileData, value, "FileData"); }
        }
    }
}
