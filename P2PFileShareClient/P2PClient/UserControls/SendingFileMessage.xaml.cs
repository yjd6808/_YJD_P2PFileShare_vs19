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
            this.TextBlock_Type.Text = "전송 중...";
        }

        public void SynchronizeSendingFile(SendingFile file)
        {
            file.AddSynchronizeTickCount(1);
            long leftByteSize = file.GetLeftByteSize();
            double percentage = file.GetPercentage();
            (TransferUnit unit, float speed) transferingSpeed = file.GetSendingSpeed();
            

            TextBlock_FileName.Text = file.GetFileName();
            TextBlock_DownloadBytes.Text = string.Format("{0:0.#} / {1:0.#} MB [ {2:0.##}% ]", (file.FileSize - leftByteSize) / (double)1048576, file.FileSize / (double)1048576, percentage);
            TextBlock_LeftTime.Text = string.Format("남은시간 : {0}분 {1}초 [ {2:0.##} {3}/s ]", file.GetLeftTimeTotalSeconds() / 60, file.GetLeftTimeTotalSeconds() % 60, transferingSpeed.speed, transferingSpeed.unit.ToString());
            ProgressBar_DownloadBytes.Value = percentage;
        }

        public void Finish()
        {
            IsSendingOver = true;
            TextBlock_LeftTime.Text = "";
            TextBlock_DownloadBytes.HorizontalAlignment = HorizontalAlignment.Center;
            TextBlock_DownloadBytes.Text = "전송 완료";
            TextBlock_FileName.Text = _SendingFile.GetFileName();
            TextBlock_Type.Text = "";
            ProgressBar_DownloadBytes.Value = 100.0f;
            
        }

        public void DisconnectedFromPeerOrServer()
        {
            TextBlock_DownloadBytes.HorizontalAlignment = HorizontalAlignment.Center;
            TextBlock_DownloadBytes.Text = "전송 중단";
            TextBlock_FileName.Text = _SendingFile.GetFileName();
            TextBlock_LeftTime.Text = "연결이 끊어졌습니다";
            TextBlock_Type.Text = "";
            _SendingFile.TerminateStream();
        }
    }
}
