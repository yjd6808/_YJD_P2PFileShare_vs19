﻿// ===============================
// @AUTHOR      : 윤정도
// @CREATE DATE : 2020-02-06 오후 6:55:25   
// @PURPOSE     : 네트워크 패킷들 정의함
// ===============================


using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace P2PShared
{
    #region P2PClient [Client <-> Server]
    public enum ConnectionTypes { Unknown, LAN, WAN }

    [Serializable]
    public class P2PClientInfo : INetworkPacket
    {
        public string Name { get; set; }
        public long ID { get; set; }
        public IPEndPoint ExternalEndpoint { get; set; }
        public IPEndPoint InternalEndpoint { get; set; }
        
        public ConnectionTypes ConnectionType { get; set; }
        public List<IPAddress> InternalAddresses = new List<IPAddress>();
        private object m_Guard = new object();

        [NonSerialized] //둘다 씀
        public TcpClient TCPClient = new TcpClient();
        [NonSerialized] //클라만 씀
        public UdpClient UDPClient = new UdpClient();

        [NonSerialized] //서버만 씀
        public bool TCPInitialized = false;

        [NonSerialized] //서버만 씀
        public bool UDPInitialized = false;

        [NonSerialized] //클라만 씀
        public IPEndPoint UDPEndPoint = null;

        [NonSerialized]
        public bool IsP2PConnected = false;

        


        public P2PClientInfo()
        {
        }

        public bool Update(P2PClientInfo updatedClient)
        {
            lock (m_Guard)
            {
                if (ID == updatedClient.ID)
                {
                

                    foreach (PropertyInfo P in updatedClient.GetType().GetProperties())
                        if (P.GetValue(updatedClient) != null)
                            P.SetValue(this, P.GetValue(updatedClient));

                    this.InternalAddresses.Clear();
                    this.InternalAddresses.AddRange(updatedClient.InternalAddresses);
                }
            }

            return (ID == updatedClient.ID); //제대로 업데이트 됬다
        }

        public override string ToString()
        {
            if (ExternalEndpoint != null)
                return Name + " (" + ExternalEndpoint.Address + ")";
            else
                return Name + " (UDP Endpoint Unknown)";
        }

        public P2PClientInfo Simplified()
        {
            return new P2PClientInfo()
            {
                Name = this.Name,
                ID = this.ID,
                InternalEndpoint = this.InternalEndpoint,
                ExternalEndpoint = this.ExternalEndpoint,
                m_Guard = new object()
            };
        }

        public P2PClientInfo Clone()
        {
            P2PClientInfo clone = new P2PClientInfo()
            {
                Name = this.Name,
                ID = this.ID,
                InternalEndpoint = this.InternalEndpoint,
                ExternalEndpoint = this.ExternalEndpoint,
                ConnectionType = this.ConnectionType,
                TCPClient = this.TCPClient,
                UDPClient = this.UDPClient,
                TCPInitialized = this.TCPInitialized,
                UDPInitialized = this.UDPInitialized,
                m_Guard = new object()
            };
            clone.InternalAddresses.AddRange(this.InternalAddresses);
            return clone;
        }

    }
    #endregion

    #region Notification [Client <-> Server]
    public enum NotificationType { ServerShutDown, ClientDisconnected }

    [Serializable]
    public class Notification : INetworkPacket
    {
        public long ID { get; set; }

        public NotificationType Type { get; set; }
        public object Tag { get; set; }

        public Notification(NotificationType type, object tag)
        {
            this.ID = 0; //이 값을 안쓴다.
            this.Type = type;
            this.Tag = tag;
        }
    }

    #endregion

    #region HeartBeat [Client -> Server]
    [Serializable]
    public class HeartBeat : INetworkPacket
    {
        public long ID { get; set; }
        public HeartBeat(long id)
        {
            this.ID = id;
        }
    }
    #endregion

    #region RequestP2PConnect [Client <-> Server]

    [Serializable]
    public class RequestP2PConnect : INetworkPacket
    {
        public long ID { get; set; }
        public long RecipientID { get; set; }

        public RequestP2PConnect(long id, long recipientId)
        {
            this.ID = id;
            this.RecipientID = recipientId;
        }
    }

    #endregion

    #region TestMessage [P2P]

    [Serializable]
    public class TestMessage : INetworkPacket
    {
        public long ID { get; set; }
        public string Content { get; set; }


        public TestMessage(long id, string content)
        {
            this.ID = id;
            this.Content = content;
        }


    }

    #endregion

    #region Ack [P2P]

    [Serializable]
    public class Ack : INetworkPacket
    {
        public long ID { get; set; }
        public long RecipientID { get; set; }
        public bool Connected { get; set; }

        public Ack(long id)
        {
            this.ID = id;

            this.RecipientID = 0; //수신이 성공하게되면 수신자의 ID가 담기게 된다.
            this.Connected = false;
        }

    }
    #endregion

    #region ChatMessage [P2P]

    [Serializable]
    public class P2PMessage : INetworkPacket
    {
        public long ID { get; set; }
        public string Message { get; set; }

        public P2PMessage(long id, string msg)
        {
            this.ID = id;
            this.Message = msg;
        }
    }

    #endregion

    #region RequestPath [P2P]

    public enum RequestPathStatus { Pending, Failed, Success }

    [Serializable]
    public class RequestPath : INetworkPacket
    {
        public long ID { get; set; }
        public long RecipientID { get; set; }
        public string RequestedPath { get; set; }
        public List<P2PPath> DirectoriesAndFiles = new List<P2PPath>();
        public RequestPathStatus Status { get; set; }
        public string Message;

        public RequestPath(long id, long recipientId, string requestPath)
        {
            this.ID = id;
            this.RecipientID = recipientId;
            this.RequestedPath = requestPath;
            this.Status = RequestPathStatus.Pending;
            this.Message = "현재 요청 진행중입니다.";
        }
    }



    #endregion

    #region P2PNotification [P2P]

    public enum P2PNotificatioyType { Disconnected }

    [Serializable]
    public class P2PNotification : INetworkPacket
    {
        public long ID { get; set; }
        public P2PNotificatioyType NotificationType { get; set; }

        public P2PNotification(long id, P2PNotificatioyType notiType)
        {
            this.ID = id;
            this.NotificationType = notiType;
        }

    }

    #endregion
}
