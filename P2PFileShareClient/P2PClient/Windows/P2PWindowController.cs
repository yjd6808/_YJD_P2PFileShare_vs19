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
                StackPanel_Chat.Children.Add(new ChatMessage("상대방과 연결되어있지 않습니다", HorizontalAlignment.Right));
                return;
            }
            SendMessageToPeer(new P2PMessage(m_MasterClient.MyInfo.ID, TextBox_Chat.Text));
            StackPanel_Chat.Children.Add(new ChatMessage(msg, HorizontalAlignment.Right));
            StackPanel_ScrollBar.ScrollToBottom();
        }
        public void ReceivePeerMessage(P2PMessage p2pMessage)
        {
            StackPanel_Chat.Children.Add(new ChatMessage(p2pMessage.Message, Brushes.Crimson, HorizontalAlignment.Left));
            StackPanel_ScrollBar.ScrollToBottom();
        }

        public void ReceivePeerMessage(P2PRequestPath P2PRequestPathPacket)
        {
            switch (P2PRequestPathPacket.Status)
            {
                case P2PRequestPathStatus.Failed:
                    {
                        StackPanel_Chat.Children.Add(
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
            StackPanel_Chat.Children.Add(new ChatMessage("상대방이 입장하였습니다.",
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
            StackPanel_Chat.Children.Add(new ChatMessage(message,
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
            SendMessageToPeer(new P2PFileReuqest(m_MasterClient.MyInfo.ID, path.FullPath));
        }
    }
}
