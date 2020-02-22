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
            
        }

        public void AddSendingFileMessage(SendingFile fileInfo)
        {
            SendingFileMessage control = new SendingFileMessage(fileInfo);
            AddChatMessage(control);
            m_SendingFileMessageControls.Add(fileInfo.FileID, control);
        }

        public void AddReceivingFileMessage(ReceivingFile fileInfo)
        {
            ReceivingFileMessage control = new ReceivingFileMessage(fileInfo);

            AddChatMessage(control);
            m_ReceivingFileMessageControls.Add(fileInfo.FileID, control);
        }

       
        public void SynchronizeSendingFileMessage(SendingFile fileInfo)
        {
            m_SendingFileMessageControls.TryGetValue(fileInfo.FileID, out SendingFileMessage control);
            if (control == null)
                return;
            control.SynchronizeSendingFile(fileInfo);
        }

        public void SynchronizeReceivingFileMessage(ReceivingFile fileInfo)
        {
            m_ReceivingFileMessageControls.TryGetValue(fileInfo.FileID, out ReceivingFileMessage control);
            if (control == null)
                return;
            control.SynchronizeReceivingFile(fileInfo);
        }

        public void FinishSendingFileMessage(SendingFile fileInfo)
        {
             if (m_SendingFileMessageControls.TryGetValue(fileInfo.FileID, out SendingFileMessage control))
                control.Finish();
        }

        public void FinishReceivingFileMessage(ReceivingFile fileInfo)
        {
            if (m_ReceivingFileMessageControls.TryGetValue(fileInfo.FileID, out ReceivingFileMessage control))
                control.Finish();
        }


        public void AddChatMessage(UserControl userControl)
        {
            if (StackPanel_Chat.Children.Count >= 100)
                StackPanel_Chat.Children.RemoveAt(0);

            #region 사이에 끼워넣기 필요할까?
            //if (StackPanel_Chat.Children.Count == 0)
            //StackPanel_Chat.Children.Add(userControl);
            //else
            //{
            //    int lastIdx = StackPanel_Chat.Children.Count - 1;

            //    if (userControl.GetType() == typeof(ChatMessage))
            //    {
            //        //마지막 요소가 챗매시지 컨트롤이 아닐경우
            //        if (StackPanel_Chat.Children[lastIdx].GetType() != typeof(ChatMessage))
            //            //맨뒤 삽입
            //            StackPanel_Chat.Children.Add(userControl);
            //        else
            //        {

            //            int idx = lastIdx;
            //            for (idx = StackPanel_Chat.Children.Count - 1; idx >= 0; idx--)
            //            {
            //                if (StackPanel_Chat.Children[idx].GetType() == typeof(ChatMessage))
            //                    break;
            //                else if (StackPanel_Chat.Children[idx].GetType() == typeof(ReceivingFileMessage) && 
            //                         ((ReceivingFileMessage)StackPanel_Chat.Children[idx]).IsReceivingOver)
            //                    break;
            //                else if (StackPanel_Chat.Children[idx].GetType() == typeof(SendingFileMessage) &&
            //                         ((SendingFileMessage)StackPanel_Chat.Children[idx]).IsSendingOver)
            //                    break;
            //            }

            //            StackPanel_Chat.Children.Insert(idx + 1, userControl); 
            //        }
            //    }
            //    else //Sending Receiving 관련 메시지는 그냥 추가해주면됨

            //}

            #endregion

            StackPanel_Chat.Children.Add(userControl);
            StackPanel_ScrollBar.ScrollToBottom();
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

            foreach (var control in m_SendingFileMessageControls.Where(x => x.Value.IsSendingOver == false))
                control.Value.DisconnectedFromPeerOrServer();

            foreach (var control in m_ReceivingFileMessageControls.Where(x => x.Value.IsReceivingOver == false))
                control.Value.DisconnectedFromPeerOrServer();
        }

        public void WriteNotifyingMessage(string message)
        {
            AddChatMessage(new ChatMessage(message,
                Brushes.LightGray,
                Brushes.Black,
                HorizontalAlignment.Center));
        }

        public void WriteErrorMessage(string message)
        {
            AddChatMessage(new ChatMessage(message,
                Brushes.LightCoral,
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
            if (path.PathType == P2PPathType.Previous ||
                path.PathType == P2PPathType.Directory)
                return;

            SendMessageToPeer(new P2PRequestFile(m_MasterClient.MyInfo.ID, path.FullPath));
        }

        public void RemoveSendingFileMessage(P2PFileTransferingError p2PFileTransferingError)
        {
            long fileID = p2PFileTransferingError.FileID;

            MasterClient masterClient = MasterClient.GetInstance();
            masterClient.SendingFileList.TryGetValue(p2PFileTransferingError.ID, out Dictionary<long, SendingFile> sendingFilesByID);
            if (sendingFilesByID == null)
                throw new Exception(p2PFileTransferingError.ID + "에 해당하는 리스트를 찾지 못했습니다.");

            sendingFilesByID.TryGetValue(p2PFileTransferingError.FileID, out SendingFile sendingFile);

            if (sendingFile == null)
                throw new Exception(p2PFileTransferingError.FileID + "에 해당하는 샌딩 파일을 찾지 못했습니다.");

            sendingFilesByID.Remove(p2PFileTransferingError.FileID);


            if (m_SendingFileMessageControls.TryGetValue(p2PFileTransferingError.FileID, out SendingFileMessage control))
                StackPanel_Chat.Children.Remove(control);

        }

        public void DisconnectedFromServer()
        {
            foreach (var control in m_SendingFileMessageControls.Where(x => x.Value.IsSendingOver == false))
                control.Value.DisconnectedFromPeerOrServer();

            foreach (var control in m_ReceivingFileMessageControls.Where(x => x.Value.IsReceivingOver == false))
                control.Value.DisconnectedFromPeerOrServer();

            ConnectedClient = null;
            WriteNotifyingMessage("메인서버와 연결이 끊어졌습니다. 모든 연결이 끊어집니다.");
            Button_ConnectedStatus_PackIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.WifiOff;
            Button_ConnectedStatus_PackIcon.Foreground = Brushes.LightGray;
        }

        private void AddFavorite(P2PPath selectedItem)
        {
            if (m_MasterClient.FavoritePathList.Exists(x => x == selectedItem.FullPath))
            {
                WriteNotifyingMessage(selectedItem.FullPath + "경로는 이미 즐겨찾기에 추가되어있습니다.");
                return;
            }

            m_MasterClient.FavoritePathList.Add(selectedItem.FullPath);
            m_MasterClient.SaveFavoritePaths();
            
            RefreshFavroites();
        }

        private void RemoveFavorite(string path)
        {
            if (m_MasterClient.FavoritePathList.Exists(x => x.Equals(path)))
            {
                m_MasterClient.FavoritePathList.Remove(path);
                WriteNotifyingMessage(path + "경로가 즐겨찾기에서 삭제되었습니다.");
                m_MasterClient.SaveFavoritePaths();
                RefreshFavroites();
            }
        }

        private void RefreshFavroites()
        {
            ComboBox_InputPath.Items.Clear();
            m_MasterClient.FavoritePathList.ForEach(x => ComboBox_InputPath.Items.Add(x));
        }

        public void Reconnect(P2PClientInfo reconnectedClient)
        {
            ConnectedClient = reconnectedClient;
            AddChatMessage(new ChatMessage("상대방이 다시 입장하였습니다.",
                 Brushes.Beige,
                 Brushes.Black,
                 HorizontalAlignment.Center));
            Button_ConnectedStatus_PackIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Wifi;
            Button_ConnectedStatus_PackIcon.Foreground = Brushes.Green;
        }
    }
}
