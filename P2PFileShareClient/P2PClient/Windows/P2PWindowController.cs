// ===============================
// @AUTHOR      : 윤정도
// @CREATE DATE : 2020-02-15 오후 9:57:38   
// @PURPOSE     : P2PWindow 핵심 로직
// ===============================


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using P2PShared;
using System.Windows.Controls;

namespace P2PClient
{
    public partial class P2PWindow
    {
        private void SendText()
        {
            string msg = TextBox_Chat.Text;
            TextBox_Chat.Text = string.Empty;

            if (msg.Length <= 0) 
                return;

            if (ConnectedClient == null || ConnectedClient.IsP2PConnected == false)
            {
                AddChatMessage(new ChatMessage("상대방과 연결되어있지 않습니다", HorizontalAlignment.Right));
                return;
            }
            SendMessageToPeer(new P2PMessage(m_MasterClient.MyInfo.ID, msg));

            AddChatMessage(new ChatMessage(msg, HorizontalAlignment.Right));
            StackPanel_ScrollBar.ScrollToBottom();
        }
        public void ReceivePeerMessage(P2PMessage p2pMessage)
        {
            AddChatMessage(new ChatMessage(p2pMessage.Message, Brushes.Crimson, HorizontalAlignment.Left));
            StackPanel_ScrollBar.ScrollToBottom();
        }

        public void AddSendingFileMessage(SendingFile fileInfo)
        {
            SendingFileMessage control = new SendingFileMessage(fileInfo);

            AddChatMessage(control);
            m_SendingFileMessageControls.Add(control);
        }

        public void AddReceivingFileMessage(ReceivingFile fileInfo)
        {
            ReceivingFileMessage control = new ReceivingFileMessage(fileInfo);

            AddChatMessage(control);
            m_ReceivingFileMessageControls.Add(control);
        }

       
        public void SynchronizeSendingFileMessage(SendingFile fileInfo)
        {
            SendingFileMessage control =  m_SendingFileMessageControls.FirstOrDefault(x => x._SendingFile.FileID == fileInfo.FileID);
            control.SynchronizeSendingFile(fileInfo);
        }

        public void SynchronizeReceivingFileMessage(ReceivingFile fileInfo)
        {
            ReceivingFileMessage control = m_ReceivingFileMessageControls.FirstOrDefault(x => x._ReceivingFile.FileID == fileInfo.FileID);
            control.SynchronizeReceivingFile(fileInfo);
        }

        public void FinishSendingFileMessage(SendingFile fileInfo)
        {
            SendingFileMessage control = m_SendingFileMessageControls.FirstOrDefault(x => x._SendingFile.FileID == fileInfo.FileID);
            control.Finish();
        }

        public void FinishReceivingFileMessage(ReceivingFile fileInfo)
        {
            ReceivingFileMessage control = m_ReceivingFileMessageControls.FirstOrDefault(x => x._ReceivingFile.FileID == fileInfo.FileID);
            control.Finish();
        }


        public void AddChatMessage(UserControl userControl)
        {
            if (StackPanel_Chat.Children.Count >= 100)
                StackPanel_Chat.Children.RemoveAt(0);

            if (StackPanel_Chat.Children.Count == 0)
                StackPanel_Chat.Children.Add(userControl);
            else
            {
                int lastIdx = StackPanel_Chat.Children.Count - 1;

                if (userControl.GetType() == typeof(ChatMessage))
                {
                    //마지막 요소가 챗매시지 컨트롤이 아닐경우
                    if (StackPanel_Chat.Children[lastIdx].GetType() != typeof(ChatMessage))
                        //맨뒤 삽입
                        StackPanel_Chat.Children.Add(userControl);
                    else
                    {
                        int idx = lastIdx;
                        for (idx = StackPanel_Chat.Children.Count - 1; idx >= 0; idx--)
                        {
                            if (StackPanel_Chat.Children[idx].GetType() == typeof(ChatMessage))
                                break;
                            else if (StackPanel_Chat.Children[idx].GetType() == typeof(ReceivingFileMessage) && 
                                     ((ReceivingFileMessage)StackPanel_Chat.Children[idx]).IsReceivingOver)
                                break;
                            else if (StackPanel_Chat.Children[idx].GetType() == typeof(SendingFileMessage) &&
                                     ((SendingFileMessage)StackPanel_Chat.Children[idx]).IsSendingOver)
                                break;
                        }

                        StackPanel_Chat.Children.Insert(idx + 1, userControl); 
                    }
                }
                else //Sending Receiving 관련 메시지는 그냥 추가해주면됨
                    StackPanel_Chat.Children.Add(userControl);
            }
        }



        public void ReceivePeerMessage(P2PRequestPath P2PRequestPathPacket)
        {
            switch (P2PRequestPathPacket.Status)
            {
                case P2PRequestPathStatus.Failed:
                    {
                        AddChatMessage(
                            new ChatMessage(
                            P2PRequestPathPacket.Message,
                            Brushes.LightGray,
                            Brushes.Black,
                            HorizontalAlignment.Center));
                    }
                    break;
                case P2PRequestPathStatus.Success:
                    {
                        CurrentPeerDirectory = P2PRequestPathPacket.RequestedPath;
                        ListView_PathList.Items.Clear();
                        ListView_PathList.Items.Add(P2PPath.GetPreviousPath());

                        for (int i = 0; i < P2PRequestPathPacket.DirectoriesAndFiles.Count; i++)
                            ListView_PathList.Items.Add(P2PRequestPathPacket.DirectoriesAndFiles[i]);
                    }
                    break;
            }
        }

        public void ConnectedToPeer()
        {
            AddChatMessage(new ChatMessage("상대방이 입장하였습니다.",
                 Brushes.LightPink,
                 Brushes.Black,
                 HorizontalAlignment.Center));
            Button_ConnectedStatus_PackIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Wifi;
            Button_ConnectedStatus_PackIcon.Foreground = Brushes.Green;
        }

        public void DisconnectedFromPeer()
        {
            ConnectedClient = null;
            WriteNotifyingMessage("상대방이 방을 나갔습니다.\n(프로그램을 끈게아님)");
            Button_ConnectedStatus_PackIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.WifiOff;
            Button_ConnectedStatus_PackIcon.Foreground = Brushes.LightGray;
        }

        private void WriteNotifyingMessage(string message)
        {
            AddChatMessage(new ChatMessage(message,
                Brushes.LightGray,
                Brushes.Black,
                HorizontalAlignment.Center));
        }

        private void SendMessageToPeer(INetworkPacket packet)
        {
            if (ConnectedClient == null || ConnectedClient.IsP2PConnected == false)
                return;

            m_MasterClient.SendMessageUDP(packet, ConnectedClient.UDPEndPoint);
        }

        private void SerachPath()
        {
            if (ComboBox_InputPath.Text.Length <= 0)
                return;

            SendMessageToPeer(new P2PRequestPath(
                   this.m_MasterClient.MyInfo.ID,
                   this.ConnectedClient.ID,
                   ComboBox_InputPath.Text));
        }

        private void DownloadFile(P2PPath path)
        {
            SendMessageToPeer(new P2PRequestFile(m_MasterClient.MyInfo.ID, path.FullPath));
        }
    }
}
