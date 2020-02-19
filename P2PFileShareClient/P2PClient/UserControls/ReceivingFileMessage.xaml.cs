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
    /// DownloadMessage.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ReceivingFileMessage : UserControl
    {
        public ReceivingFile _ReceivingFile { get; set; }
        public bool IsReceivingOver { get; set; }

        public ReceivingFileMessage(ReceivingFile receivingFile)
        {
            InitializeComponent();
            this._ReceivingFile = receivingFile;
            this.IsReceivingOver = false;
            this.TextBlock_FileName.Text = "데이터 갱신 중...";
        }

        public void SynchronizeReceivingFile(ReceivingFile file)
        {
            long leftByteSize = file.GetLeftByteSize();

            TextBlock_FileName.Text = file.GetFileName();
            TextBlock_DownloadBytes.Text = file.FileSize - leftByteSize + " / " + file.FileSize + "[" + "]";
            TextBlock_LeftTime.Text = file.GetLeftTimeTotalSeconds() / 60 + "분 " + file.GetLeftTimeTotalSeconds() % 60 + "초";
            ProgressBar_DownloadBytes.Value = file.FileSize - leftByteSize / file.FileSize;
        }

        public void Finish()
        {
            IsReceivingOver = true;
            TextBlock_LeftTime.Text = "";
            TextBlock_DownloadBytes.HorizontalAlignment = HorizontalAlignment.Center;
            TextBlock_DownloadBytes.Text = "다운로드 완료";
        }
    }
}
