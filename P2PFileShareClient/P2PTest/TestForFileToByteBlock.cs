// ===============================
// @AUTHOR      : 윤정도
// @CREATE DATE : 2020-02-17 오후 10:23:28   
// @PURPOSE     : 
// ===============================


using Microsoft.VisualStudio.TestTools.UnitTesting;
using P2PShared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PTest
{
    [TestClass]
    public class TestForFileToByteBlock
    {
        List<ByteBlock> _blocks = new List<ByteBlock>();

        const string _testFilePath = @"D:\Downloads\[블랙바컷] KIM.JI.YOUNG, BORN.1982.2019.1080p.FHDRip.H264.AAC.mp4";
        const string _desPath = @"C:\Users\jungdo\Desktop\Excutable Files\MyFile.mp4";
        const long _sepByte = 50000000L;
        const int _maxBlockSize = 8 * 1024;   // 8KB

        [TestMethod]
        public void TestMain()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            using (FileStream writeStream = File.Create(_desPath))
            using (FileStream fileStream = File.OpenRead(_testFilePath))
            {
                long byteToRead = (int)fileStream.Length;
                int writeOffset = 0;

                long sepCount = fileStream.Length / _sepByte;
                long leftByteCount = fileStream.Length % _sepByte;

                List<long> sepList = new List<long>();
                for (int i = 0; i < sepCount; i++)
                    sepList.Add(_sepByte);
                sepList.Add(leftByteCount);

                Console.WriteLine(_sepByte + " 바이트씩 분할 : " + sepCount + " / " + leftByteCount);

                for (int i = 0; i < sepList.Count; i++)
                {
                    byte[] buff = new byte[sepList[i]];
                    int readByteSize = fileStream.Read(buff, (int)0, (int)sepList[i]);

                    if (readByteSize == 0)
                        break;

                    byteToRead -= readByteSize;
                    writeStream.Write(buff, 0, readByteSize);

                    writeOffset += readByteSize;
                }



                //int readByteSize = fileStream.Read(buff, startOffset, (int)leftByteCount);



                //용량 3.8gb는 40억 바이트가 넘는다.. 이걸 어떻게 전송하느냐..





                //int fileSize = Convert.ToInt32(buff.Length);

                //int moveIdx = 0;

                //for (int bytePos = 0; bytePos < fileSize;)
                //{
                //    int moveFileSize = fileSize - bytePos > _maxBlockSize ? _maxBlockSize : fileSize - bytePos;
                //    byte[] chunk = new byte[moveFileSize];
                //    Array.Copy(buff, bytePos, chunk, 0, moveFileSize);

                //    bytePos += moveFileSize;
                //    _blocks.Add(new ByteBlock(moveIdx++, chunk));
                //}
                //stopwatch.Stop();

                //fileSize = 0;
                //for (int i = 0; i < _blocks.Count; i++)
                //    fileSize += _blocks[i].Data.Length;

                //Console.WriteLine("파일 명 : " + _testFilePath);
                //Console.WriteLine("크기 {0:0.###} : " + buff.BytesToGigabytes());
                //Console.WriteLine("파일 -> 바이트블록 리스트로 변환 경과시간 : {0:0.###}", stopwatch.Elapsed.TotalSeconds);
                //Console.WriteLine("나눈 바이트 블록 수 : " + _blocks.Count);
                //Console.WriteLine("다시 합친 크기 {0:0.###} : " + buff.BytesToGigabytes());

            }


            
        }
    }
}
