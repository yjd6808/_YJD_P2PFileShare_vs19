// ===============================
// @AUTHOR      : 윤정도
// @CREATE DATE : 2020-02-08 오후 9:11:49   
// @PURPOSE     : 
// ===============================


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using P2PClient;
using P2PShared;

namespace P2PClient
{
    //TCP는 서버와 통신
    //UDP는 처음에만 서버와 데이터를 주고받고 그다음은 P2P 통신만 하자

    public partial class MasterClient
    {
        static public MasterClient s_Instance;

        public string     FavoritePath   = "Favorite.json";
        public IPEndPoint ServerEndpoint = new IPEndPoint(IPAddress.Parse("221.162.129.150"), 12345);

        private Thread m_ClientTCPListenerThread;
        private Thread m_ClientUDPListenerThread;
        private Thread m_ClientHeartBeatThread;
        private Thread m_ReceivingFileSynchronizingThread;
        private Thread m_SendingFileSynchronizingThread;

        private bool m_IsUDPListened;
        private bool m_IsTCPListened;
        private bool m_IsReceivingFileSynchronizingThreadStart;
        private bool m_IsSendingFileSynchronizingThreadStart;
        private bool m_IsConnected;

        public P2PClientInfo MyInfo;
        public Dictionary<long, P2PClientInfo> OtherClientList;
        public Dictionary<long, P2PClientInfo> ConnectedClientList;
        
        public Dictionary<long, Dictionary<long, SendingFile>>      SendingFileList;      //현재 MyInfo.ID와 파일 <송수신중인 클라이언트 ID, 데이터정보>
        public Dictionary<long, Dictionary<long, ReceivingFile>>    ReceivingFileList;

        private object sendingFileListLocker;
        private object receivingFileListLocker;

        public List<string> FavoritePathList;

        private List<P2PRequestConnectAck> m_SuccessAckResponses;


        /*========================= 이벤트 ================================*/

        public event EventHandler<P2PClientInfo>    OnMyInfoUpdated;
        public event EventHandler<P2PClientInfo>    OnOtherClientAdded;
        public event EventHandler<P2PClientInfo>    OnOtherClientUpdated;
        public event EventHandler<P2PClientInfo>    OnOtherClientDisconnected;

        public event EventHandler                   OnOtherClientP2PConnected;
        public event EventHandler<P2PMessage>       OnOtherClientP2PMessageArrived;
        public event EventHandler<P2PRequestPath>   OnOtherClientP2PRequestPathArrived;
        public event EventHandler                   OnOtherClientP2PDisconnected;

        public event EventHandler<ReceivingFile>            OnStartReceivingFile;
        public event EventHandler<SendingFile>              OnStartSendingFile;
        public event EventHandler<ReceivingFile>            OnSynchronizingReceivingFile;
        public event EventHandler<SendingFile>              OnSynchronizingSendingFile;
        public event EventHandler<ReceivingFile>            OnFinishReceivingFile;
        public event EventHandler<SendingFile>              OnFinishSendingFile;
        public event EventHandler<P2PFileTransferingError>  OnReceiveTransferingError;

        public event EventHandler                   OnServerConnect;
        public event EventHandler                   OnServerDisconnect;

        private MasterClient()
        {
            m_IsUDPListened = false;
            m_IsTCPListened = false;

            m_IsReceivingFileSynchronizingThreadStart = false;
            m_IsSendingFileSynchronizingThreadStart = false;

            MyInfo = new P2PClientInfo();
            MyInfo.TCPClient = new TcpClient();
            MyInfo.UDPClient = new UdpClient();
            m_SuccessAckResponses = new List<P2PRequestConnectAck>();
            OtherClientList = new Dictionary<long, P2PClientInfo>();
            ConnectedClientList = new Dictionary<long, P2PClientInfo>();

            SendingFileList = new Dictionary<long, Dictionary<long, SendingFile>>();
            ReceivingFileList = new Dictionary<long, Dictionary<long, ReceivingFile>>();

            sendingFileListLocker = new object();
            receivingFileListLocker = new object();

            MyInfo.UDPClient.AllowNatTraversal(true);
            MyInfo.UDPClient.Client.SetIPProtectionLevel(IPProtectionLevel.Unrestricted);
            MyInfo.UDPClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            MyInfo.Name = System.Environment.MachineName;
            MyInfo.ConnectionType = ConnectionTypes.Unknown;
            MyInfo.ID = DateTime.Now.Ticks;

            var IPs = Dns.GetHostEntry(Dns.GetHostName()).AddressList.Where(ip => ip.AddressFamily == AddressFamily.InterNetwork);

            foreach (var IP in IPs)
                MyInfo.InternalAddresses.Add(IP);

            FavoritePathList = new List<string>();
            LoadFavoritePaths();
        }

        

        public void Init()
        {
            if (OnMyInfoUpdated != null)
                OnMyInfoUpdated.Invoke(this, MyInfo);
        }

        static public MasterClient GetInstance()
        {
            if (s_Instance == null)
                s_Instance = new MasterClient();
            return s_Instance;
        }

        public void LoadFavoritePaths()
        {
            if (File.Exists(FavoritePath) == false)
                return;
            try
            {
                JObject jObject = JObject.Parse(File.ReadAllText(FavoritePath));
                if (jObject.ContainsKey("Favorites"))
                {
                    foreach (var o in jObject["Favorites"] as JArray)
                        FavoritePathList.Add(o.ToString());
                }
            }
            catch
            {
                WindowLogger.WriteLineError("즐겨찾기 파일을 읽어오는데 실패하였습니다. 올바른 데이터가 입력되어있는지 확인부탁드립니다.");
            }
        }

        public void SaveFavoritePaths()
        {
            if (FavoritePathList.Count <= 0)
                return;

            JObject obj = new JObject();
            JArray array = new JArray();
            FavoritePathList.ForEach(x =>  array.Add(x));
            obj.Add("Favorites", array);
            File.WriteAllText(FavoritePath, obj.ToString());
        }

        public bool IsConnectedToP2PServer() => m_IsConnected;
        public Dictionary<long, P2PClientInfo> GetAllClients() {
            Dictionary<long, P2PClientInfo> all = new Dictionary<long, P2PClientInfo>();
            
            foreach (P2PClientInfo info in OtherClientList.Values )
                all.Add(info.ID, info);
            all.Add(MyInfo.ID, MyInfo);
            return all;
        }

        public void ConnectToMainServer()
        {
            try
            {
                MyInfo.TCPClient = new TcpClient();
                IAsyncResult asyncResult = MyInfo.TCPClient.Client.BeginConnect(ServerEndpoint, null, null);

                bool success = asyncResult.AsyncWaitHandle.WaitOne(1500, true);

                if (MyInfo.TCPClient.Client.Connected)
                    MyInfo.TCPClient.Client.EndConnect(asyncResult);
                else
                {
                    MyInfo.TCPClient.Client.Close();
                    throw new ApplicationException("타임 아웃으로 서버에 접속하는데 실패하였습니다.");
                }

                m_IsTCPListened = true;
                m_IsUDPListened = true;

                StartTCPListen();
                StartUDPListen();
                StartHearBeatToKeepUDPHolePunchingState();

                SendMessageUDP(MyInfo.Simplified(), ServerEndpoint);
                MyInfo.InternalEndpoint = (IPEndPoint)MyInfo.UDPClient.Client.LocalEndPoint;

                Thread.Sleep(500);
                SendMessageTCP(MyInfo);

                m_IsConnected = true;
                WindowLogger.WriteLineMessage("정상적으로 접속하였습니다.");

                if (OnServerConnect != null)
                    OnServerConnect.Invoke(this, new EventArgs());

            }
            catch (Exception ex)
            {
                WindowLogger.WriteLineError("서버에 접속하는 동안 오류가 발생했습니다. : " + ex.Message);
            }

        }

        public void ConnectToOtherClient(P2PClientInfo client)
        {
            if (client.ID == MyInfo.ID)
            {
                WindowLogger.WriteLineError("나 자신과 연결할 수 없습니다. 이 오류는 발생하면 안되는 오류입니다");
                return;
            }

            RequestP2PConnect req = new RequestP2PConnect(MyInfo.ID, client.ID);
            SendMessageTCP(req);

            WindowLogger.WriteLineMessage(client.ToString() + " 에게 P2P 연결요청을 보냈습니다");

            Task.Run((delegate
            {
                IPEndPoint ResponsiveEP = FindReachableEndpoint(client);

                if (ResponsiveEP != null)
                {
                    WindowLogger.WriteLineMessage(client.ToString() + "과 P2P 연결이 성공하였습니다.");

                    client.UDPEndPoint = ResponsiveEP;
                    client.IsP2PConnected = true;

                    if (ConnectedClientList.TryGetValue(client.ID, out P2PClientInfo p2PClientInfo) == false)
                        ConnectedClientList.Add(client.ID, client);

                    if (OnOtherClientP2PConnected != null)
                        OnOtherClientP2PConnected.Invoke(client, new EventArgs());
                }
                else
                    WindowLogger.WriteLineError(client.ToString() + "과 P2P 연결이 실패하였습니다.");
            }));
        }

        public void ReceiveDisconnedtedClient(long disconnectedClientID)
        {
            OtherClientList.TryGetValue(disconnectedClientID, out P2PClientInfo clientInfo);

            if (clientInfo != null)
            {
                lock (sendingFileListLocker)
                {
                    //Exist 함수 대용으로 이 값이 존재하면 접속이 끊긴 유저와 전송,다운중인 파일이 있다는 뜻이니...
                    if (SendingFileList.TryGetValue(clientInfo.ID, out Dictionary<long, SendingFile> sendingFileList))
                        SendingFileList.Remove(clientInfo.ID);
                }

                lock (receivingFileListLocker)
                {
                    if (ReceivingFileList.TryGetValue(clientInfo.ID, out Dictionary<long, ReceivingFile> receivingFileList))
                        ReceivingFileList.Remove(clientInfo.ID);
                }

                if (OnOtherClientDisconnected != null)
                    OnOtherClientDisconnected.Invoke(this, clientInfo);

                OtherClientList.Remove(clientInfo.ID);
            }
        }

        public void ReceiveDisconnedtedClient(IPEndPoint endPoint)
        {

        }


        private IPEndPoint FindReachableEndpoint(P2PClientInfo client)
        {
            WindowLogger.WriteLineMessage(client.ToString() + "과 LAN 연결 시도중...");
            for (int i = 0; i < client.InternalAddresses.Count; i++)
            {
                if (MyInfo.TCPClient.Connected == false)
                {
                    WindowLogger.WriteLineError("서버와 연결이 끊겨있습니다. 다시 중개 서버에 접속 후 P2P 연결요청을 시도해주세요.");
                    break;
                }

                

                IPEndPoint testEndPoint = new IPEndPoint(client.InternalAddresses[i], client.InternalEndpoint.Port);

                for (int j = 1; j < 4; j++)
                {
                    if (MyInfo.TCPClient.Connected == false)
                    {
                        WindowLogger.WriteLineError("서버와 연결이 끊겨있습니다. 다시 중개 서버에 접속 후 P2P 연결요청을 시도해주세요.");
                        break;
                    }

                    WindowLogger.WriteLineMessage(testEndPoint.ToString() + "아이피로 UDP 접속 시도중...");

                    SendMessageUDP(new P2PRequestConnect(MyInfo.ID), testEndPoint);
                    Thread.Sleep(200);

                    P2PRequestConnectAck Responce = m_SuccessAckResponses.FirstOrDefault(a => a.ID == client.ID);
                    if (Responce != null)
                    {
                        WindowLogger.WriteLineMessage(testEndPoint.ToString() + "아이피로부터 ACK를 수신하였습니다.");
                        client.ConnectionType = ConnectionTypes.LAN;
                        m_SuccessAckResponses.Remove(Responce);
                        return testEndPoint;
                    }
                }
            }



            if (client.ExternalEndpoint != null)
            {
                WindowLogger.WriteLineMessage(client.ToString() + "과 WAN 연결 시도중...");

                if (!client.TCPClient.Connected)
                    return null;


                WindowLogger.WriteLineMessage(client.ExternalEndpoint + "아이피로 UDP 접속 시도중...");
                SendMessageUDP(new P2PRequestConnect(MyInfo.ID), client.ExternalEndpoint);
                Thread.Sleep(300);


                P2PRequestConnectAck Responce = m_SuccessAckResponses.FirstOrDefault(a => a.ID == client.ID);

                if (Responce != null)
                {
                    WindowLogger.WriteLineMessage(client.ExternalEndpoint + "아이피로부터 ACK를 수신하였습니다.");
                    client.ConnectionType = ConnectionTypes.WAN;
                    m_SuccessAckResponses.Remove(Responce);

                    return client.ExternalEndpoint;
                }
            }

            return null;
        }


        //UDP 홀펀칭을 유지하기위해 하트비트 시전
        private void StartHearBeatToKeepUDPHolePunchingState()
        {
            m_ClientHeartBeatThread = new Thread(new ThreadStart(delegate
            {
                while (m_IsTCPListened && MyInfo.TCPClient.Connected)
                {
                    SendMessageTCP(new HeartBeat(MyInfo.ID));
                    Thread.Sleep(5000);
                }
            }));

            m_ClientHeartBeatThread.IsBackground = true;

            if (m_IsTCPListened)
                m_ClientHeartBeatThread.Start();
        }

        public void DisconnectToMainServer()
        {
            if (MyInfo.TCPClient.Connected)
            {
                MyInfo.TCPClient.Client.Disconnect(true);

                m_IsConnected = false;
                m_IsTCPListened = false;
                m_IsUDPListened = false;

                OtherClientList.Clear();
                ConnectedClientList.Clear();

                if (OnServerDisconnect != null)
                    OnServerDisconnect.Invoke(this, new EventArgs());
                
                WindowLogger.WriteLineMessage("서버와 연결이 끊어졌습니다.");
            }
        }
        
        public void SendMessageTCP(INetworkPacket packet)
        {
            if (MyInfo.TCPClient.Connected)
            {
                byte[] Data = packet.ToByteArray();

                try
                {
                    NetworkStream NetStream = MyInfo.TCPClient.GetStream();
                    NetStream.Write(Data, 0, Data.Length);
                }
                catch (Exception e)
                {
                    WindowLogger.WriteLineError("TCP 메시지를 보내는데 오류가 발생했습니다 : " + e.Message);
                }
            }
        }

        public void SendMessageUDP(INetworkPacket packet, IPEndPoint EP)
        {
            byte[] data = packet.ToByteArray();

            try
            {
                if (data != null)
                    MyInfo.UDPClient.Send(data, data.Length, EP);
            }
            catch (Exception e)
            {
                WindowLogger.WriteLineError("UDP 메시지를 보내는데 오류가 발생했습니다 : " + e.Message);
            }
        }

        private void StartTCPListen()
        {
            m_ClientTCPListenerThread = new Thread(new ThreadStart(delegate
            {
                byte[] receivedBytes = new byte[4096];
                int bytesRead = 0;

                while (m_IsTCPListened)
                {
                    try
                    {
                        bytesRead = MyInfo.TCPClient.GetStream().Read(receivedBytes, 0, receivedBytes.Length);
                        if (bytesRead == 0)
                            break;
                        else
                        {
                            INetworkPacket packet = receivedBytes.ToP2PBase();
                            TcpPacketParse(packet);
                        }
                    }
                    catch (Exception e)
                    {
                        WindowLogger.WriteLineError("TCP 메시지 수신중 오류가 발생했습니다 : " + e.Message);
                    }
                }

            }));

            m_ClientTCPListenerThread.IsBackground = true;
            
            if (m_IsTCPListened)
                m_ClientTCPListenerThread.Start();

        }

        private void StartUDPListen()
        {
            m_ClientUDPListenerThread = new Thread(new ThreadStart(delegate
            {
                while (m_IsUDPListened)
                {
                    try
                    {
                        IPEndPoint EP = MyInfo.InternalEndpoint;

                        if (EP != null)
                        {
                            byte[] ReceivedBytes = MyInfo.UDPClient.Receive(ref EP);
                            INetworkPacket packet = ReceivedBytes.ToP2PBase();
                            UdpPacketParse(packet, EP);
                        }
                    }
                    catch (Exception e)
                    {
                        WindowLogger.WriteLineError( "UDP 메시지 수신중 오류가 발생했습니다 : " + e.Message);
                    }
                }
            }));

            m_ClientUDPListenerThread.IsBackground = true;

            if (m_IsUDPListened)
                m_ClientUDPListenerThread.Start();
        }

        
        /// <param name="Id">파일 다운로드를 요청한 유저의 ID</param>
        private void AddSendingFile(long Id, SendingFile sendingFile)
        {
            
            try
            {
                lock (sendingFileListLocker)
                {
                    if (SendingFileList.ContainsKey(Id))
                        SendingFileList[Id].Add(sendingFile.FileID, sendingFile);
                    else
                    {
                        SendingFileList.Add(Id, new Dictionary<long, SendingFile>());
                        SendingFileList[Id].Add(sendingFile.FileID, sendingFile);
                    }
                }
            }
            catch (Exception e)
            {
                WindowLogger.WriteLineError("AddSendingFile : " + e.Message);
            }
        }

        /// <param name="Id">파일 다운로드를 요청한 유저의 ID</param>
        private void RemoveSendingFile(long Id, long fileId)
        {
            try
            {
                lock (sendingFileListLocker)
                {
                    if (SendingFileList.ContainsKey(Id))
                        SendingFileList[Id].Remove(fileId);
                }
            }
            catch (Exception e)
            {
                WindowLogger.WriteLineError("RemoveSendingFile : " + e.Message);
            }
        }

        private SendingFile GetSendingFile(long Id, long fileId)
        {
            try
            {
                lock (sendingFileListLocker)
                {
                    if (SendingFileList.ContainsKey(Id) && SendingFileList[Id].ContainsKey(fileId))
                        return SendingFileList[Id][fileId];
                }
            }

            catch (Exception e)
            {
                WindowLogger.WriteLineError("GetSendingFile : " + e.Message);
            }

            return null;
        }

        private void AddReceivingFile(long Id, ReceivingFile receivingFile)
        {
            try
            {
                lock (receivingFileListLocker)
                {
                    if (ReceivingFileList.ContainsKey(Id))
                        ReceivingFileList[Id].Add(receivingFile.FileID, receivingFile);
                    else
                    {
                        ReceivingFileList.Add(Id, new Dictionary<long, ReceivingFile>());
                        ReceivingFileList[Id].Add(receivingFile.FileID, receivingFile);
                    }
                }
            }
            catch (Exception e)
            {
                WindowLogger.WriteLineError("AddReceivingFile : " + e.Message);
            }
        }

        private void RemoveReceivingFile(long Id, long fileId)
        {
            try
            {
                lock (receivingFileListLocker)
                {
                    if (ReceivingFileList.ContainsKey(Id))
                        ReceivingFileList[Id].Remove(fileId);
                }
            }
            catch (Exception e)
            {
                WindowLogger.WriteLineError("RemoveReceivingFile : " + e.Message);
            }
        }

        private ReceivingFile GetReceivingFile(long Id, long fileId)
        {
            try
            {
                lock (receivingFileListLocker)
                {
                    if (ReceivingFileList.ContainsKey(Id) && ReceivingFileList[Id].ContainsKey(fileId))
                        return ReceivingFileList[Id][fileId];
                }
            }
            catch (Exception e)
            {
                WindowLogger.WriteLineError("GetReceivingFile : " + e.Message);
            }

            return null;
        }


    }
}
