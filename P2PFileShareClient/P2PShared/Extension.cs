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

        public static INetworkPacket LiteNetLibToP2PBase(this byte[] bytes)
        {
            if (bytes.Length <= 1)
            {
                Console.WriteLine(bytes.Length);
                return null;
            }

            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream Stream = new MemoryStream();

            Stream.Write(bytes, 1, bytes.Length - 1);
            Stream.Seek(0, SeekOrigin.Begin);

            INetworkPacket clientInfo = (INetworkPacket)formatter.Deserialize(Stream);

            return clientInfo;
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




        public static double BytesToKilobytes(this byte[] bytes)
        {
            return bytes.Length / 1024d;
        }

        public static double BytesToMegabytes(this byte[] bytes)
        {
            return BytesToKilobytes(bytes) / 1024d;
        }

        public static double BytesToGigabytes(this byte[] bytes)
        {
            return BytesToKilobytes(bytes) / 1048576D;
        }

        public static double BytesToKilobytes(this uint bytesSize)
        {
            return bytesSize / 1024d;
        }

        public static double BytesToMegabytes(this uint bytesSize)
        {
            return BytesToKilobytes(bytesSize) / 1024d;
        }

        public static double BytesToGigabytes(this uint bytesSize)
        {
            return BytesToKilobytes(bytesSize) / 1048576D;
        }
    }
}
