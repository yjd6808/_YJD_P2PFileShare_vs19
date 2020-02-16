// ===============================
// @AUTHOR      : 윤정도
// @CREATE DATE : 2020-02-16 오후 11:07:07   
// @PURPOSE     : 
// ===============================


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using P2PShared;

namespace P2PClient
{
    public class P2PClientTransferingData
    {
        public Dictionary<uint, ByteBlock> Blocks { get; set; }

        public P2PClientTransferingData()
        {
            Blocks = new Dictionary<uint, ByteBlock>();
        }
    }
}
