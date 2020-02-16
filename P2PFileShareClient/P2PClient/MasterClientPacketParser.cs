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
            else if (packet.GetType() == typeof(P2PRequestConnect))
                UdpProcessP2PRequestConnect((P2PRequestConnect)packet, ip);
            else if (packet.GetType() == typeof(P2PRequestConnectAck))
                UdpProcessP2PRequestConnectAck((P2PRequestConnectAck)packet, ip);
            else if (packet.GetType() == typeof(P2PMessage))
                UdpProcessP2PMessage((P2PMessage)packet);
            else if (packet.GetType() == typeof(P2PRequestPath))
                UdpProcessP2PRequestPath((P2PRequestPath)packet, ip);
            else if (packet.GetType() == typeof(P2PNotification))
                UdpProcessP2PNotification((P2PNotification)packet);
            else if (packet.GetType() == typeof(P2PRequestFile))
                UdpProcessP2PRequestFile((P2PRequestFile)packet, ip);
            else if (packet.GetType() == typeof(P2PRequestFileAck))
                UdpProcessP2PRequestFileAck((P2PRequestFileAck)packet, ip);
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

        private void UdpProcessP2PRequestConnect(P2PRequestConnect p2pRequestConnect_packet, IPEndPoint ip)
        {
            P2PClientInfo CI = OtherClientList.FirstOrDefault(x => x.ID == p2pRequestConnect_packet.ID);

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

            SendMessageUDP(new P2PRequestConnectAck(MyInfo.ID), ip);
        }


        private void UdpProcessP2PRequestConnectAck(P2PRequestConnectAck p2pRequestConnect_packet, IPEndPoint ip)
        {
            P2PClientInfo CI = OtherClientList.FirstOrDefault(x => x.ID == p2pRequestConnect_packet.ID);

            if (CI == null)
                return;

            m_SuccessAckResponses.Add(p2pRequestConnect_packet);
            WindowLogger.WriteLineMessage(ip + "로부터 연결되었다는 메시지를 수신하였습니다.");
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

        private void UdpProcessP2PRequestPath(P2PRequestPath p2pRequestPath_packet, IPEndPoint ip)
        {
            P2PClientInfo client = GetAllClients().FirstOrDefault(x => x.ID == p2pRequestPath_packet.ID);

            if (client == null)
            {
                WindowLogger.WriteLineMessage("상대방으로부터 메시지가 도착하였지만 상대방과 연결되어있지 않습니다.");
                return;
            }

            if (p2pRequestPath_packet.Status == P2PRequestPathStatus.Pending)
            {
                try
                {
                    if (Directory.Exists(p2pRequestPath_packet.RequestedPath))
                    {
                        string[] paths = Directory.GetDirectories(p2pRequestPath_packet.RequestedPath);
                        string[] directoryPaths = Directory.GetFiles(p2pRequestPath_packet.RequestedPath);
                        int pathEndIdx = paths.Length > 50 ? 50 : paths.Length;
                        int directoryEndIdx = directoryPaths.Length > 50 ? 50 : directoryPaths.Length;

                        for (int i = 0; i < pathEndIdx; i++)
                            p2pRequestPath_packet.DirectoriesAndFiles.Add(new P2PPath(paths[i]));

                        for (int i = 0; i < directoryEndIdx; i++)
                            p2pRequestPath_packet.DirectoriesAndFiles.Add(new P2PPath(directoryPaths[i]));

                        p2pRequestPath_packet.Status = P2PRequestPathStatus.Success;
                        p2pRequestPath_packet.Message = "정상적으로 경로 수신완료";
                    }
                    else
                    {
                        p2pRequestPath_packet.Status = P2PRequestPathStatus.Failed;
                        p2pRequestPath_packet.Message = "올바른 경로를 입력해주세요.";
                    }
                }
                catch (Exception e)
                {
                    p2pRequestPath_packet.Status = P2PRequestPathStatus.Failed;
                    p2pRequestPath_packet.Message = "경로 수신 실패 : " + e.Message;
                    p2pRequestPath_packet.DirectoriesAndFiles.Clear();
                }
                finally
                {
                    //디렉토리들 에코해줌
                    SendMessageUDP(p2pRequestPath_packet, ip);
                }
            }
            else
            {
                if (OnOtherClientP2PRequestPathArrived != null)
                    OnOtherClientP2PRequestPathArrived.Invoke(null, p2pRequestPath_packet);
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

        private void UdpProcessP2PRequestFile(P2PRequestFile p2PRequestFile_packet, IPEndPoint ip)
        {
            P2PClientInfo otherClient = ConnectedClientList.FirstOrDefault(x => x.ID == p2PRequestFile_packet.ID);

            if (otherClient == null)
                return;

            P2PRequestFileAck requestAck = new P2PRequestFileAck(MyInfo.ID); 

            try
            {
                if (File.Exists(p2PRequestFile_packet.RequestPath) == false)
                    throw new Exception("해당 파일이 상대방의 경로에 존재하지 않습니다. 새로고침 해주세요.");

                

                P2PClientTransferingData p2PClientTransferingData = new P2PClientTransferingData();
                TransferingDataList.Add(p2PRequestFile_packet.ID, p2PClientTransferingData);



                try
                {
                    // Read it in & compute a checksum of the original file
                    byte[] fileBytes = File.ReadAllBytes(p2PRequestFile_packet.RequestPath);
                    checksum = m_MD5Hasher.ComputeHash(fileBytes);
                    fileSize = Convert.ToUInt32(fileBytes.Length);
                    Console.WriteLine("{0} is {1} bytes large.", p2PRequestFile_packet.RequestPath, fileSize);

                    // Compress it
                    Stopwatch timer = new Stopwatch();
                    using (MemoryStream compressedStream = new MemoryStream())
                    {
                        // Perform the actual compression
                        DeflateStream deflateStream = new DeflateStream(compressedStream, CompressionMode.Compress, true);
                        timer.Start();
                        deflateStream.Write(fileBytes, 0, fileBytes.Length);
                        deflateStream.Close();
                        timer.Stop();

                        // Put it into blocks
                        compressedStream.Position = 0;
                        long compressedSize = compressedStream.Length;
                        UInt32 id = 1;
                        while (compressedStream.Position < compressedSize)
                        {
                            // Grab a chunk
                            long numBytesLeft = compressedSize - compressedStream.Position;
                            long allocationSize = (numBytesLeft > MaxBlockSize) ? MaxBlockSize : numBytesLeft;
                            byte[] data = new byte[allocationSize];
                            compressedStream.Read(data, 0, data.Length);

                            // Create a new block
                            Block b = new Block(id++);
                            b.Data = data;
                            _blocks.Add(b.Number, b);
                        }

                        // Print some info and say we're good
                        Console.WriteLine("{0} compressed is {1} bytes large in {2:0.000}s.", requestedFile, compressedSize, timer.Elapsed.TotalSeconds);
                        Console.WriteLine("Sending the file in {0} blocks, using a max block size of {1} bytes.", _blocks.Count, MaxBlockSize);
                        good = true;
                    }
                }
                catch (Exception e)
                {
                    // Crap...
                    Console.WriteLine("Could not prepare the file for transfer, reason:");
                    Console.WriteLine(e.Message);

                    // Reset a few things
                    _blocks.Clear();
                    checksum = null;
                }

                //requestAck.FileSize = Convert.ToUInt32(readBytes.Length);
                //requestAck.CheckSum = m_MD5Hasher.ComputeHash(readBytes);
                //requestAck.BlockSize =  
                //requestAck.Message = "성공적으로 데이터정보를 가져왔습니다.";
            }
            catch (Exception e)
            {
                requestAck.Message = e.Message;
                requestAck.IsSuccess = false;

                if (TransferingDataList.ContainsKey(p2PRequestFile_packet.ID))
                    TransferingDataList.Remove(p2PRequestFile_packet.ID);
            }
            finally
            {
                SendMessageUDP(requestAck, ip);
            }
        }

        private void UdpProcessP2PRequestFileAck(P2PRequestFileAck p2PRequestFileAck_packet, IPEndPoint ip)
        {

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
