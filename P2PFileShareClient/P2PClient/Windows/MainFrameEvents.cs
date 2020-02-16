// ===============================
// @AUTHOR      : 윤정도
// @CREATE DATE : 2020-02-15 오후 10:07:49   
// @PURPOSE     : 메인프레임 이벤트 관리
// ===============================


using P2PClient;
using P2PShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PClient
{
    public partial class MainFrame
    {
        private void M_MasterClient_OnOtherClientP2PDisconnected(object sender, EventArgs e)
        {
            P2PClientInfo connetedClient = sender as P2PClientInfo;
            P2PWindow p2pWindow = P2PWindows.FirstOrDefault(x => x.ConnectedClient.ID == connetedClient.ID);

            if (p2pWindow == null)
                return;

            Dispatcher.Invoke(() =>
            {
                p2pWindow.Activate();
                p2pWindow.Focus();
                p2pWindow.BringIntoView();
                p2pWindow.DisconnectedFromPeer();
            });
        }

        private void M_MasterClient_OnOtherClientP2PRequestPathArrived(object sender, P2PRequestPath e)
        {
            Dispatcher.Invoke(() =>
            {

                //주의 : 에코이므로 상대방 정보를 클래스에 담고있어야함
                P2PWindow p2pWindow = P2PWindows.FirstOrDefault(x => x.ConnectedClient.ID == e.RecipientID);

                if (p2pWindow == null)
                    return;


                p2pWindow.Activate();
                p2pWindow.Focus();
                p2pWindow.BringIntoView();
                p2pWindow.ReceivePeerMessage(e);
            });
        }


        private void M_MasterClient_OnOtherClientP2PMessageArrived(object sender, P2PMessage e)
        {
            P2PClientInfo connetedClient = sender as P2PClientInfo;
            P2PWindow p2pWindow = P2PWindows.FirstOrDefault(x => x.ConnectedClient.ID == connetedClient.ID);
            if (p2pWindow == null)
                return;

            Dispatcher.Invoke(() =>
            {
                p2pWindow.Activate();
                p2pWindow.Focus();
                p2pWindow.BringIntoView();
                p2pWindow.ReceivePeerMessage(e);
            });
        }

        private void M_MasterClient_OnOtherClientP2PConnected(object sender, EventArgs e)
        {
            P2PClientInfo connetedClient = sender as P2PClientInfo;
            P2PWindow p2pWindow = P2PWindows.FirstOrDefault(x => x.ConnectedClient.ID == connetedClient.ID);

            Dispatcher.Invoke(() =>
            {
                if (p2pWindow == null)
                {
                    p2pWindow = new P2PWindow(connetedClient);
                    P2PWindows.Add(p2pWindow);
                    p2pWindow.Show();
                }
                else
                {
                    p2pWindow.Activate();
                    p2pWindow.Focus();
                    p2pWindow.BringIntoView();
                }
            });
        }



        private void M_MasterClient_OnMyInfoUpdated(object sender, P2PClientInfo e)
        {
            Dispatcher.Invoke(() =>
            {
                RefreshMyInfo(e);
            });
        }



        private void M_MasterClient_OnServerConnect(object sender, EventArgs e)
        {
            Button_ConnectToMainServer.Content = "접속 해제";
        }

        private void M_MasterClient_OnServerDisconnect(object sender, EventArgs e)
        {

            Dispatcher.Invoke(delegate
            {
                Button_ConnectToMainServer.Content = "접속";
                ListBox_ClientList.Items.Clear();
                P2PWindows.ForEach(x => x.Close());
            });
        }

        private void M_MasterClient_OnOtherClientAdded(object sender, P2PClientInfo e)
        {
            Dispatcher.Invoke(delegate
            {
                ListBox_ClientList.Items.Add(e.Clone());
            });
        }

        private void M_MasterClient_OnOtherClientUpdated(object sender, P2PClientInfo e)
        {
            Dispatcher.Invoke(delegate
            {
                foreach (P2PClientInfo CI in ListBox_ClientList.Items)
                    if (CI.ID == e.ID)
                        CI.Update(e);

                RefreshDetails();
            });
        }


        private void M_MasterClient_OnOtherClientDisconnected(object sender, P2PClientInfo disconnectedClient)
        {
            P2PClientInfo findDisconnectedClient = null;
            P2PWindow p2pWindow = P2PWindows.FirstOrDefault(x => x.ConnectedClient.ID == disconnectedClient.ID);

            foreach (P2PClientInfo otherClinet in ListBox_ClientList.Items)
                if (otherClinet.ID == disconnectedClient.ID)
                    findDisconnectedClient = otherClinet;

            Dispatcher.Invoke(delegate
            {
                if (findDisconnectedClient != null)
                    ListBox_ClientList.Items.Remove(findDisconnectedClient);

                if (p2pWindow != null)
                {
                    p2pWindow.Close();
                    WindowLogger.WriteLineMessage(findDisconnectedClient.ToString() + "과 연결이 끊어졌습니다");
                }

                RefreshDetails();
            });
        }

    }
}
