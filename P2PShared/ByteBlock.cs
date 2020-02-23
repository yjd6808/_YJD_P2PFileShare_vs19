// ===============================
// @AUTHOR      : 윤정도
// @CREATE DATE : 2020-02-16 오후 11:20:24   
// @PURPOSE     : 파일의 부분 덩어리
// ===============================


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PShared
{
    [Serializable]
    public class ByteBlock
    {
        public int Number { get; set; }
        public byte[] Data { get; set; }

        public ByteBlock(int number, byte[] data)
        {
            this.Number = number;
            this.Data = data;
        }
    }
}
