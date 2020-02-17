// ===============================
// @AUTHOR      : 윤정도
// @CREATE DATE : 2020-02-17 오후 9:39:11   
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
    public class ReceivingFile
    {
        public long ID { get; set; }
        public List<ByteBlock> Data { get; set; }
        public byte[] CheckSum { get; set; }
        public uint FileSize { get; set; }

        private int ReceivePos;

        public ReceivingFile(long Id, byte[] checkSum, uint fileSize)
        {
            this.ID = Id;
            this.Data = new List<ByteBlock>();
            this.CheckSum = checkSum;
            this.FileSize = fileSize;
            this.ReceivePos = 0;
        }


    }
}
 