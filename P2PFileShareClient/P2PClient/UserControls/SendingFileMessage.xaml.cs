using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace P2PClient
{
    /// <summary>
    /// SendingFileMessage.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SendingFileMessage : UserControl
    {
        public SendingFile _SendingFile { get; set; }
        public bool IsSendingOver { get; set; }

        public SendingFileMessage(SendingFile sendingFile)
        {
            InitializeComponent();
            this._SendingFile = sendingFile;
            this.IsSendingOver = false;
            this.TextBlock_FileName.Text = "데이터 갱신 중...";
        }

        public void SynchronizeSendingFile(SendingFile file)
        {
            long leftByteSize = file.GetLeftByteSize();

            TextBlock_FileName.Text = file.GetFileName();
            TextBlock_DownloadBytes.Text = file.FileSize - leftByteSize + " / " + file.FileSize + "[" + "]";
            TextBlock_LeftTime.Text = file.GetLeftTimeTotalSeconds() / 60 + "분 " + file.GetLeftTimeTotalSeconds() % 60 + "초";
            ProgressBar_DownloadBytes.Value = file.FileSize - leftByteSize / file.FileSize;
        }

        public void Finish()
        {
            IsSendingOver = true;
            TextBlock_LeftTime.Text = "";
            TextBlock_DownloadBytes.HorizontalAlignment = HorizontalAlignment.Center;
            TextBlock_DownloadBytes.Text = "전송 완료";

        }
    }
}
