// ===============================
// @AUTHOR      : 윤정도
// @CREATE DATE : 2020-02-06 오후 7:00:32   
// @PURPOSE     : 패킷 -> 바이트[] / 바이트[] -> 패킷 시리얼라이징
// ===============================


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace P2PShared
{
    public static class ByteConverter
    {
        public static byte[] ToByteArray(this INetworkPacket packet)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream Stream = new MemoryStream();

            formatter.Serialize(Stream, packet);
            return Stream.ToArray();
        }

        public static INetworkPacket ToP2PBase(this byte[] bytes)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream Stream = new MemoryStream();

            Stream.Write(bytes, 0, bytes.Length);
            Stream.Seek(0, SeekOrigin.Begin);

            INetworkPacket clientInfo = (INetworkPacket)formatter.Deserialize(Stream);

            return clientInfo;
        }
    }
}
