// ===============================
// @AUTHOR      : 윤정도
// @CREATE DATE : 2020-02-06 오후 5:02:27   
// @PURPOSE     : 마스터 서버
// ===============================


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using P2PShared;

using P2PServer.Util;



namespace P2PServer
{
    public partial class P2PServer
    {
        private static P2PServer s_P2PServer = null;

        private TcpListener         m_TcpListener;
        private IPEndPoint          m_TcpEndPoint;

        private UdpClient           m_UdpClient;
        private IPEndPoint          m_UdpEndPoint;

        private Thread              m_ClientTCPListenerThread;
        private Thread              m_ClientUDPListenerThread;
        
        public List<P2PClientInfo>  ClientList;
        public bool                 IsServerOpened;

        private P2PServer()
        {
            m_TcpEndPoint       = new IPEndPoint(IPAddress.Any, 12345);
            m_UdpEndPoint       = new IPEndPoint(IPAddress.Any, 12345);
            m_TcpListener       = new TcpListener(m_TcpEndPoint);
            m_UdpClient         = new UdpClient(m_UdpEndPoint);
            ClientList          = new List<P2PClientInfo>();
            IsServerOpened      = false;
        }

        static public P2PServer GetInstance()
        {
            if (s_P2PServer == null)
                s_P2PServer = new P2PServer();

            return s_P2PServer;
        }

        public void Open()
        {
            m_TcpListener.Start();
            IsServerOpened = true;
            m_ClientTCPListenerThread = new Thread(_thread_TCPListenerThread);
            m_ClientTCPListenerThread.Start();

            m_ClientUDPListenerThread = new Thread(_thread_UDPListenerThread);
            m_ClientUDPListenerThread.Start();
        }

        public void Close()
        {
            m_TcpListener.Stop();
            IsServerOpened = false;
        }

        private void _thread_UDPListenerThread()
        {
            ThreadSafeLogger.WriteLineMessage("UDP 수신 대기중...");

            while (true)
            {
                byte[] ReceivedBytes = null;

                try
                {
                    ReceivedBytes = m_UdpClient.Receive(ref m_UdpEndPoint);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("UDP Error: {0}", ex.Message);
                }

                if (ReceivedBytes != null)
                {
                    INetworkPacket packet = ReceivedBytes.ToP2PBase();
                    UdpPacketParse(packet, m_UdpEndPoint);
                }
            }
        }

        private void _thread_TCPListenerThread()
        {
            ThreadSafeLogger.WriteLineMessage("TCP 수신 대기중...");

            while (IsServerOpened)
            {
                 TcpClient accepted = m_TcpListener.AcceptTcpClient();

                Action<object> ProcessData = new Action<object>(delegate (object _Client)
                {
                    TcpClient client = (TcpClient)_Client;
                    client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

                    byte[] Data = new byte[4096];
                    int BytesRead = 0;

                    while (client.Connected)
                    {
                        try
                        {
                            BytesRead = client.GetStream().Read(Data, 0, Data.Length);
                        }
                        catch
                        {
                            Disconnect(client);
                        }

                        if (BytesRead == 0)
                            break;
                        else if (client.Connected)
                        {
                            INetworkPacket packet = Data.ToP2PBase();
                            TcpPacketParse(packet, client);
                        }
                    }

                    Disconnect(client);
                });

                Thread ThreadProcessData = new Thread(new ParameterizedThreadStart(ProcessData));
                ThreadProcessData.Start(accepted);
            }
        }

        private void Disconnect(TcpClient client)
        {
            P2PClientInfo p2pClient = ClientList.FirstOrDefault(x => x.TCPClient == client);

            if (p2pClient != null)
            {
                ClientList.Remove(p2pClient);
                ThreadSafeLogger.WriteLineMessage(p2pClient.ExternalEndpoint + " / " + p2pClient.InternalEndpoint + "연결이 끊어졌습니다. / 남은 클라이언트 수 : " + ClientList.Count);
                client.Close();

                BroadcastTCP(new Notification(NotificationType.ClientDisconnected, p2pClient.ID));
            }
        }


        

        public bool IsOpen()
        {
            return IsServerOpened;
        }

        public void BroadcastTCP(INetworkPacket packet)
        {
            foreach (P2PClientInfo client in ClientList.Where(x => x.TCPClient != null))
            {
                ThreadSafeLogger.WriteLineMessage("TCP 브로드 캐스트 : " + client.ExternalEndpoint);
                SendTCP(packet, client.TCPClient);
            }
        }

        
        public void SendTCP(INetworkPacket packet, TcpClient Client)
        {
            if (Client != null && Client.Connected)
            {
                byte[] Data = packet.ToByteArray();
                Client.GetStream().Write(Data, 0, Data.Length);
            }
        }

        public void SendUDP(INetworkPacket packet)
        {
            byte[] Bytes = packet.ToByteArray();
            m_UdpClient.Send(Bytes, Bytes.Length, m_UdpEndPoint);
        }
    }
}
