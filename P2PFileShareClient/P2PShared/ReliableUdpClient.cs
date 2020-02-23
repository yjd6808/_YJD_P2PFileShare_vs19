// ===============================
// @AUTHOR      : 윤정도
// @CREATE DATE : 2020-02-23 오후 2:01:27   
// @PURPOSE     : LiteNetLib 활용해서 WLAN도 사용가능하도록 만들자
// ===============================


using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using LiteNetLib;
using LiteNetLib.Utils;


namespace P2PShared
{
    public class ReliableUdpClient
    {
        public delegate void DelegateOnUnconnectedMessageReceived(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType);
        public delegate void DelegateOnConnectedMessageReceived(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType);

        private EventBasedNetListener _listener;
        private NetManager _uppClient;

        public IPEndPoint LocalEndPoint { get {return _uppClient.LocalEndPoint; } }

        public event DelegateOnUnconnectedMessageReceived OnUnconnectedMessageReceived;
        public event DelegateOnConnectedMessageReceived OnMessageReceived;

        public ReliableUdpClient()
        {
            _listener = new EventBasedNetListener();
            _uppClient = new NetManager(_listener)
            {
                UnconnectedMessagesEnabled = true,
                ReuseAddress = true
            };
            _uppClient.Start();
            _uppClient.Connect()
            _listener.NetworkReceiveUnconnectedEvent += _listener_NetworkReceiveUnconnectedEvent;
            _listener.NetworkReceiveEvent += _listener_NetworkReceiveEvent;



        }

        private void _listener_NetworkReceiveEvent(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            Console.WriteLine("메시지 도착 1" + reader.GetByte());
        }

        private void _listener_NetworkReceiveUnconnectedEvent(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {
            Console.WriteLine("메시지 도착 2" + reader.GetByte());
            if (OnUnconnectedMessageReceived != null)
                OnUnconnectedMessageReceived.Invoke(remoteEndPoint, reader, messageType);
        }

        public void Send(byte[] data, IPEndPoint endPoint)
        {
            _uppClient.SendUnconnectedMessage(data, endPoint);
        }
    }
}
