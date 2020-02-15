// @AUTHOR      : 윤정도
// @CREATE DATE : 2020-02-06 오후 6:50:57   
// @PURPOSE     : 패킷 파서
// ===============================

#define DEBUG

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using P2PShared;
using P2PClient;
using System.IO;

namespace P2PClient
{
    public partial class MasterClient
    {

        //=======================================================================//
        //                                                                       //
        //                         UDP 패킷 처리                                 //
        //                                                                       //
        //=======================================================================//

        public void UdpPacketParse(INetworkPacket packet, IPEndPoint ip)
        {
            if (packet.GetType() == typeof(P2PClientInfo))
                UdpProcessP2PClient((P2PClientInfo)packet);
            else if (packet.GetType() == typeof(TestMessage))
                UdpProcessTestMessage((TestMessage)packet);
            else if (packet.GetType() == typeof(Ack))
                UdpProcessAck((Ack)packet, ip);
            else if (packet.GetType() == typeof(P2PMessage))
                UdpProcessP2PMessage((P2PMessage)packet);
            else if (packet.GetType() == typeof(RequestPath))
                UdpProcessRequestPath((RequestPath)packet, ip);
            else if (packet.GetType() == typeof(P2PNotification))
                UdpProcessP2PNotification((P2PNotification)packet);
            else
                WindowLogger.WriteLineError("다른 종류의 패킷 수신 : " + packet.GetType());
        }

        private void UdpProcessP2PClient(P2PClientInfo clientInfo_packet)
        {
            P2PClientInfo p2pClient = GetAllClients().FirstOrDefault(x => x.ID == clientInfo_packet.ID);

            if (p2pClient == null)
            {
                p2pClient = clientInfo_packet;
                OtherClientList.Add(clientInfo_packet);
                if (OnOtherClientAdded != null)
                    OnOtherClientAdded.Invoke(this, p2pClient);
            }
            else
            {
                if (p2pClient.ID == MyInfo.ID)
                {
                    MyInfo.Update(clientInfo_packet);

                    if (OnMyInfoUpdated != null)
                        OnMyInfoUpdated.Invoke(this, MyInfo);
                }
                else
                {
                    p2pClient.Update(clientInfo_packet);

                    if (OnOtherClientUpdated != null)
                        OnOtherClientUpdated.Invoke(this, p2pClient);
                }
            }
        }

        private void UdpProcessTestMessage(TestMessage testMessage_packet)
        {
            WindowLogger.WriteLineMessage("UDP 테스트메세지 수신 : " + testMessage_packet.Content);
        }

        private void UdpProcessAck(Ack ack_packet, IPEndPoint ip)
        {

            if (ack_packet.Connected)
            {
                m_SuccessAckResponses.Add(ack_packet);
                WindowLogger.WriteLineMessage(ip + "로부터 연결되었다는 메시지를 수신하였습니다.");
            }
            else //수신하였지만 Connected가 false일 경우 이제 통
            {
                P2PClientInfo CI = OtherClientList.FirstOrDefault(x => x.ID == ack_packet.ID);

                //리스트있던 EP와 Ack요청이온 EP가 같으나 포트가 다를경우 포트를 수정해줌
                if (CI.ExternalEndpoint.Address.Equals(ip.Address) && CI.ExternalEndpoint.Port != ip.Port)
                {
                    WindowLogger.WriteLineMessage("다른 포트로부터 응답이 왔습니다. " + CI.ExternalEndpoint.Port + "에서 " + ip.Port + "로 변경하였습니다");

                    CI.ExternalEndpoint.Port = ip.Port;

                    if (OnOtherClientUpdated != null)
                        OnOtherClientUpdated.Invoke(this, CI);
                }

                List<string> IPs = new List<string>();
                CI.InternalAddresses.ForEach(new Action<IPAddress>(delegate (IPAddress IP) { IPs.Add(IP.ToString()); }));

                //리스트있던 EP와 Ack요청이온 EP가 다르고 IIP들중에서 EP가 없을 경우
                if (CI.ExternalEndpoint.Address.Equals(ip.Address) == false && IPs.Contains(ip.Address.ToString()) == false)
                {
                    WindowLogger.WriteLineMessage("다른 IP주소로부터 ACK를 수신하였습니다. 갱신합니다.");
                    //IIP에 Ack요청이온 EP를 더해준다
                    CI.InternalAddresses.Add(ip.Address);
                }


                ack_packet.Connected = true;
                ack_packet.RecipientID = MyInfo.ID;
                SendMessageUDP(ack_packet, ip);
            }
        }

        private void UdpProcessP2PMessage(P2PMessage P2PMessage_packet)
        {
            P2PClientInfo otherClient = ConnectedClientList.FirstOrDefault(x => x.ID == P2PMessage_packet.ID);

            if (otherClient == null)
            {
                WindowLogger.WriteLineMessage("상대방으로부터 메시지가 도착하였지만 상대방의 정보를 찾지 못했습니다");
                return;
            }

            if (OnOtherClientP2PMessageArrived != null)
                OnOtherClientP2PMessageArrived.Invoke(otherClient, P2PMessage_packet);
        }

        private void UdpProcessRequestPath(RequestPath requestPath_packet, IPEndPoint ip)
        {
            P2PClientInfo client = GetAllClients().FirstOrDefault(x => x.ID == requestPath_packet.ID);

            if (client == null)
            {
                WindowLogger.WriteLineMessage("상대방으로부터 메시지가 도착하였지만 상대방과 연결되어있지 않습니다.");
                return;
            }

            if (requestPath_packet.Status == RequestPathStatus.Pending)
            {
                try
                {
                    if (Directory.Exists(requestPath_packet.RequestedPath))
                    {
                        string[] paths = Directory.GetDirectories(requestPath_packet.RequestedPath);
                        string[] directoryPaths = Directory.GetFiles(requestPath_packet.RequestedPath);
                        int pathEndIdx = paths.Length > 50 ? 50 : paths.Length;
                        int directoryEndIdx = directoryPaths.Length > 50 ? 50 : directoryPaths.Length;

                        for (int i = 0; i < pathEndIdx; i++)
                            requestPath_packet.DirectoriesAndFiles.Add(new P2PPath(paths[i]));

                        for (int i = 0; i < directoryEndIdx; i++)
                            requestPath_packet.DirectoriesAndFiles.Add(new P2PPath(directoryPaths[i]));

                        requestPath_packet.Status = RequestPathStatus.Success;
                        requestPath_packet.Message = "정상적으로 경로 수신완료";
                    }
                    else
                    {
                        requestPath_packet.Status = RequestPathStatus.Failed;
                        requestPath_packet.Message = "올바른 경로를 입력해주세요.";
                    }
                }
                catch (Exception e)
                {
                    requestPath_packet.Status = RequestPathStatus.Failed;
                    requestPath_packet.Message = "경로 수신 실패 : " + e.Message;
                    requestPath_packet.DirectoriesAndFiles.Clear();
                }
                finally
                {
                    //디렉토리들 에코해줌
                    SendMessageUDP(requestPath_packet, ip);
                }
            }
            else
            {
                if (OnOtherClientP2PRequestPathArrived != null)
                    OnOtherClientP2PRequestPathArrived.Invoke(null, requestPath_packet);
            }
        }

        private void UdpProcessP2PNotification(P2PNotification p2pNotification_packet)
        {
            P2PClientInfo otherClient = ConnectedClientList.FirstOrDefault(x => x.ID == p2pNotification_packet.ID);

            if (otherClient == null)
            {
                WindowLogger.WriteLineMessage("상대방으로부터 메시지가 도착하였지만 상대방의 정보를 찾지 못했습니다." );
                return;
            }

            switch (p2pNotification_packet.NotificationType)
            {
                case P2PNotificatioyType.Disconnected:
                    {
                        otherClient.IsP2PConnected = false;
                        otherClient.UDPEndPoint = null;

                        if (OnOtherClientP2PDisconnected != null)
                            OnOtherClientP2PDisconnected.Invoke(otherClient, new EventArgs());
                    }
                break;
            }
        }

        //=======================================================================//
        //                                                                       //
        //                         TCP 패킷 처리                                 //
        //                                                                       //
        //=======================================================================//
        public void TcpPacketParse(INetworkPacket packet)
        {
            if (packet.GetType() == typeof(P2PClientInfo))
                TcpProcessP2PClient((P2PClientInfo)packet);
            else if (packet.GetType() == typeof(TestMessage))
                TcpProcessTestMessage((TestMessage)packet);
            else if (packet.GetType() == typeof(Notification))
                TcpProcessNotification((Notification)packet);
            else if (packet.GetType() == typeof(RequestP2PConnect))
                TcpProcessRequestP2PConnect((RequestP2PConnect)packet);
           
            else
                WindowLogger.WriteLineError("다른 종류의 패킷 수신 : " + packet.GetType());

        }

        private void TcpProcessP2PClient(P2PClientInfo clientInfo_packet)
        {
            if (clientInfo_packet.ID == MyInfo.ID)
            {
                MyInfo.Update(clientInfo_packet);

                if (OnMyInfoUpdated != null)
                    OnMyInfoUpdated.Invoke(this, MyInfo);
                return;
            }



            P2PClientInfo p2pClient = OtherClientList.FirstOrDefault(x => x.ID == clientInfo_packet.ID);

            if (p2pClient == null)
            {
                p2pClient = clientInfo_packet;
                OtherClientList.Add(clientInfo_packet);
                if (OnOtherClientAdded != null)
                    OnOtherClientAdded.Invoke(this, p2pClient);
            }
            else
            {
                p2pClient.Update(clientInfo_packet);
                if (OnOtherClientUpdated != null)
                    OnOtherClientUpdated.Invoke(this, p2pClient);

            }
        }

        private void TcpProcessTestMessage(TestMessage testMessage_packet)
        {
            WindowLogger.WriteLineMessage("TCP 테스트메세지 수신 : " + testMessage_packet.Content);
        }

        private void TcpProcessNotification(Notification notification_packet)
        {
            switch (notification_packet.Type)
            {
                case NotificationType.ServerShutDown:
                    WindowLogger.WriteLineMessage("서버가 꺼졌습니다. 관리자에게 문의해주세요.");
                    DisconnectToMainServer();
                    break;
                case NotificationType.ClientDisconnected:
                    {
                        long  senderID = long.Parse(notification_packet.Tag.ToString());
                        P2PClientInfo clientInfo = OtherClientList.FirstOrDefault(x => x.ID == senderID);

                        if (clientInfo != null)
                        {
                            if (OnOtherClientDisconnected != null)
                                OnOtherClientDisconnected.Invoke(this, clientInfo);

                            OtherClientList.Remove(clientInfo);
                        }

                        P2PClientInfo p2pConnectedClient = ConnectedClientList.FirstOrDefault(x => x.ID == senderID);
                    }
                    break;
                default:
                    break;
            }
        }

        private void TcpProcessRequestP2PConnect(RequestP2PConnect requestp2pconnect_packet)
        {
            P2PClientInfo requesterClient = OtherClientList.FirstOrDefault(x => x.ID == requestp2pconnect_packet.ID);

            if (requesterClient == null)
            {
                WindowLogger.WriteLineError(requestp2pconnect_packet.ID + " 수신자의 클라이언트 정보를 찾지 못했습니다");
                return;
            }

            WindowLogger.WriteLineMessage(requesterClient.ToString() + "이 당신에게 P2P 연결을 시도하였습니다.");

            IPEndPoint ResponsiveEP = FindReachableEndpoint(requesterClient);

            if (ResponsiveEP != null)
            {
                WindowLogger.WriteLineMessage(requesterClient.ToString() + "과 P2P 연결이 성공하였습니다.[수신측]");

                requesterClient.UDPEndPoint = ResponsiveEP;
                requesterClient.IsP2PConnected = true;

                ConnectedClientList.Add(requesterClient);

                if (OnOtherClientP2PConnected != null)
                    OnOtherClientP2PConnected.Invoke(requesterClient, new EventArgs());

                if (OnOtherClientUpdated != null)
                    OnOtherClientUpdated.Invoke(this, requesterClient);
            }
            else
                WindowLogger.WriteLineError(requesterClient.ToString() + "과 P2P 연결이 성공하였습니다.[수신측]");
        }

        
    }
}
