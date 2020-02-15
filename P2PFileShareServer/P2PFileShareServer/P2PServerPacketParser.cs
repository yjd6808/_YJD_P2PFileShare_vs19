// ===============================
// @AUTHOR      : 윤정도
// @CREATE DATE : 2020-02-06 오후 6:50:57   
// @PURPOSE     : 패킷 파서
// ===============================

#undef DEBUG

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using P2PShared;
using P2PServer.Util;

namespace P2PServer
{
    public partial class P2PServer
    {
        //=======================================================================//
        //                         UDP 패킷 처리                                 //
        //=======================================================================//

        public void UdpPacketParse(INetworkPacket packet, IPEndPoint ip)
        {
            ThreadSafeLogger.WriteLine("===============   UDP " + packet.GetType() + "  ================");

            if (packet.GetType() == typeof(P2PClientInfo))
                UdpProcessP2PClient((P2PClientInfo)packet, ip);
            else
                ThreadSafeLogger.WriteLine("다른 종류의 패킷 수신 : " + packet.GetType());
        }

        private void UdpProcessP2PClient(P2PClientInfo packet, IPEndPoint ip)
        {
            P2PClientInfo client = ClientList.FirstOrDefault(x => x.ID == packet.ID);

            if (client == null)
            {
                client = packet;
                ThreadSafeLogger.WriteLineMessage("UDP 클라이언트 접속 IP: " + ip + ", Name: " + client.Name);
                ClientList.Add(client);

                
            }
            else
            {
                client.Update(packet);
                ThreadSafeLogger.WriteLineMessage("UDP 클라이언트 정보가 업데이트 되었습니다: " + ip + ", Name: " + client.Name);
            }

            

            //UDP로 접속한 외부 아이피정보 업데이트
            client.ExternalEndpoint = ip;

            ThreadSafeLogger.WriteLineMessage("UDP에서 TCP 브로드캐스트 작업시작");
            //새로 접속한 유저정보를 다른 유저들에게 알려줌 //처음에는 못알려준다. TCP Client가 null이기 때문
            BroadcastTCP(client);

            ThreadSafeLogger.WriteLineClientInfo(client);

            //첫 접속
            if (client.UDPInitialized == false)
            {
                SendUDP(new TestMessage(client.ID, "정도의 P2P 세상에 오신것을 환영합니다!")); //에코

                foreach (P2PClientInfo otherClient in ClientList.Where(x => x.ID != client.ID))
                    SendUDP(otherClient); //에코

                client.UDPInitialized = true;
            }
        }

        //=======================================================================//
        //                         TCP 패킷 처리                                 //
        //=======================================================================//

        public void TcpPacketParse(INetworkPacket packet, TcpClient sender)
        {
            if (packet.GetType() != typeof(HeartBeat))
                ThreadSafeLogger.WriteLine("===============   TCP " + packet.GetType() + "  ================");

            if (packet.GetType() == typeof(P2PClientInfo))
                TcpProcessP2PClient((P2PClientInfo)packet, sender);
            else if (packet.GetType() == typeof(HeartBeat))
                TcpProcessHeartBeat((HeartBeat)packet);
            else if (packet.GetType() == typeof(RequestP2PConnect))
                TcpProcessRequestP2PConnect((RequestP2PConnect)packet);
            else
                ThreadSafeLogger.WriteLine("다른 종류의 패킷 수신 : " + packet.GetType());
        }

        private void TcpProcessP2PClient(P2PClientInfo packet, TcpClient tcpClient)
        {
            P2PClientInfo client = ClientList.FirstOrDefault(x => x.ID == packet.ID);

            if (client == null)
            {
                ClientList.Add(packet);
                ThreadSafeLogger.WriteLineMessage("클라이언트 접속 IP: " + tcpClient.Client.RemoteEndPoint + ", Name: " + packet.Name);
            }
            else
            {
                client.Update(packet);
                ThreadSafeLogger.WriteLineMessage("TCP 클라이언트 정보가 업데이트 되었습니다: " + tcpClient.Client.RemoteEndPoint + ", Name: " + packet.Name);
            }

            client.TCPClient = tcpClient;
            ThreadSafeLogger.WriteLineMessage("TCP에서 TCP 브로드캐스트 작업시작");
            BroadcastTCP(client);

            ThreadSafeLogger.WriteLineClientInfo(client);

            if (client.TCPInitialized == false)
            {
                SendTCP(new TestMessage(client.ID, "정도의 P2P 세상에 오신것을 환영합니다!"), tcpClient); //에코

                foreach (P2PClientInfo otherClient in ClientList.Where(x => x.ID != client.ID))
                    SendTCP(otherClient, tcpClient); //에코

                client.TCPInitialized = true;
            }
        }

        private void TcpProcessHeartBeat(HeartBeat heartbeat_packet)
        {
            P2PClientInfo p2pClient = ClientList.FirstOrDefault(x => x.ID == heartbeat_packet.ID);

#if (DEBUG)
            if (p2pClient != null)
                ThreadSafeLogger.WriteLineMessage("클라이언트 접속 IP: " + p2pClient.ExternalEndpoint + ", Name: " + p2pClient.Name);
#endif

        }

        private void TcpProcessRequestP2PConnect(RequestP2PConnect requestp2pconnect_packet)
        {
            P2PClientInfo recipient = ClientList.FirstOrDefault(x => x.ID == requestp2pconnect_packet.RecipientID);

            if (recipient == null)
            {
                ThreadSafeLogger.WriteLineWarning(requestp2pconnect_packet.ID + "로부터 " + requestp2pconnect_packet.RecipientID + "에게 P2P 연결요청을 수신하였습니다. 하지만 수신자가 현재 서버에 존재하지 않습니다.");
                return;
            }

            if (recipient.TCPClient == null)
            {
                ThreadSafeLogger.WriteLineError(requestp2pconnect_packet.RecipientID + "의 TCP 클라이언트가 설정되어있지 않습니다");
                return;
            }

            SendTCP(requestp2pconnect_packet, recipient.TCPClient);
        }
        
    }
}
