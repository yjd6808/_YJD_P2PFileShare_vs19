// ===============================
// @AUTHOR      : 윤정도
// @CREATE DATE : 2020-02-18 오후 9:42:00   
// @PURPOSE     : 아무거나 다 테스트
// ===============================


using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PTest
{
    [TestClass]
    public class TestForEverything
    {

        string filePath = @"C:\YodaSyndrome - 몽환적인 노래 모음집 (2017-03-09).mp3";
        [TestMethod]
        public void TestMain()
        {
            FileInfo fileInfo = new FileInfo(filePath);
            Console.WriteLine(fileInfo.Length);

            using (FileStream fileStream = File.OpenRead(filePath))
            {
                Console.WriteLine(fileStream.Length);
            }
        }
    }
}
