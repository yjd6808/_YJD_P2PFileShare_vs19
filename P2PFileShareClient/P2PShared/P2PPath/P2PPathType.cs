// ===============================
// @AUTHOR      : 윤정도
// @CREATE DATE : 2020-02-15 오후 1:54:43   
// @PURPOSE     : 
// ===============================


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PShared
{
    public enum P2PPathType : int
    {
        None,
        Directory,
        Audio,
        Video,
        Exe,
        Zip,
        Image,
        Text,
        Ppt,
        Word,
        Excel,
        Pdf,

        //============//예외
        Previous = 999999
    }
}
