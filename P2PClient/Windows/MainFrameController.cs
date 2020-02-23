// ===============================
// @AUTHOR      : 윤정도
// @CREATE DATE : 2020-02-15 오후 10:10:20   
// @PURPOSE     : 
// ===============================


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
        private void RefreshDetails()
        {
            Label_ExternalEP.Content = "EEP : ";
            Label_InternalEP.Content = "IEP : ";
            Label_ConnectionType.Content = "CT : ";
            Label_InternalIPs.Content = "IIP : ";

            if (ListBox_ClientList.SelectedItem != null)
            {
                P2PClientInfo CI = (P2PClientInfo)ListBox_ClientList.SelectedItem;

                Label_ExternalEP.Content += (CI.ExternalEndpoint != null ? CI.ExternalEndpoint.ToString() : "");
                Label_InternalEP.Content += (CI.InternalEndpoint != null ? CI.InternalEndpoint.ToString() : "");
                Label_ConnectionType.Content += CI.ConnectionType.ToString();
                for (int i = 0; i < CI.InternalAddresses.Count; i++)
                {
                    if (i > 0)
                        Label_InternalIPs.Content += " / ";
                    Label_InternalIPs.Content += CI.InternalAddresses[i].ToString();

                }
            }
            else
                Button_ConnectToOtherClient.IsEnabled = false;
        }

        private void RefreshMyInfo(P2PClientInfo myInfo)
        {

            Label_MyClientInfo.Content = "EEP : " + myInfo.ExternalEndpoint + " / ";
            Label_MyClientInfo.Content += "IEP : " + myInfo.InternalEndpoint + " / ";
            Label_MyClientInfo.Content += "CT : " + myInfo.ConnectionType + " / ";
            for (int i = 0; i < myInfo.InternalAddresses.Count; i++)
            {
                if (i > 0)
                    Label_InternalIPs.Content += " / ";
                Label_MyClientInfo.Content += myInfo.InternalAddresses[i].ToString();
            }
        }
    }
}
