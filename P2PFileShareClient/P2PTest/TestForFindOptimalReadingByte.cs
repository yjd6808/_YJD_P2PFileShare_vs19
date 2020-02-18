// ===============================
// @AUTHOR      : 윤정도
// @CREATE DATE : 2020-02-18 오후 9:25:54   
// @PURPOSE     : 
// ===============================


using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace P2PTest
{
    /// <summary>
    /// 파일을 읽을때 최적의 바이트 크기
    /// </summary>
    [TestClass]
    public class TestForFindOptimalReadingByte
    {

        string filePath = @"C:\Users\jdyun\Downloads\Downloads.7z"; //임의적으로 만든 1GB 용량 파일
        [TestMethod, Ignore]

        void TestMain()
        {
            for (int j = 0; j < 4; j++)
            {
                int startReadByte = 1024;
                for (int i = 0; i < 10; i++)
                {
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();

                    using (FileStream fileStream = File.OpenRead(filePath))
                    {
                        long fileSize = fileStream.Length;
                        long readSize = 0;

                        while (true)
                        {
                            byte[] readBuff = new byte[startReadByte];
                            int readBytes = fileStream.Read(readBuff, 0, startReadByte);

                            if (readBytes == 0)
                                break;

                            readSize += readBytes;
                        }
                    }
                    stopwatch.Start();
                    Console.WriteLine(startReadByte + "바이트씩 읽어서 파일을 모두 읽었습니다. " + stopwatch.Elapsed.TotalSeconds + "초 걸렸습니다");
                    startReadByte *= 2;
                }
                Console.WriteLine("==============================================");
            }

            Thread.Sleep(50000);
        }


        //262144 바이트 = 256킬로바이트(2의 18승 바이트 = 2의 8승 킬로바이트) 씩 읽을때가 속도가 가장 빨랏다.
    }
}
