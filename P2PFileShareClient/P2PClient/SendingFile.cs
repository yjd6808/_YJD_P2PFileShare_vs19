// ===============================
// @AUTHOR      : 윤정도
// @CREATE DATE : 2020-02-17 오후 9:28:36   
// @PURPOSE     : 
// ===============================


using P2PShared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace P2PClient
{
    public class SendingFile
    {
        public long ID { get; set; }
        public List<ByteBlock> Data { get; set; }
        public byte[] CheckSum { get; set; }
        public uint FileSize { get; set; }
        public string FilePath { get; set; }
        private MD5 m_MD5Hasher;

        private bool m_IsAnalyzed;


        public SendingFile(string filePath)
        {
            this.ID = DateTime.Now.Ticks;
            this.m_MD5Hasher = MD5.Create();
            this.Data = new List<ByteBlock>();
            this.m_IsAnalyzed = false;
            AnalyzeFile(filePath);
        }

        private void AnalyzeFile(string filePath)
        {
            byte[] fileBytes = File.ReadAllBytes(filePath);
            CheckSum = m_MD5Hasher.ComputeHash(fileBytes);
            FileSize = Convert.ToUInt32(fileBytes.Length);

            uint leftByte = FileSize;
            int blockIdx = 0;
            for (long pos = 0; pos < fileBytes.Length;)
            {

                //byte[] chunkByte = new byte[0];
                //Array.Copy(fileBytes, 0, chunkByte, 0,  )

                //ByteBlock block = new ByteBlock(blockIdx++, )
            }
        }
    }
}
//                    using (MemoryStream compressedStream = new MemoryStream())
//                    {
//                        // Perform the actual compression
//                        DeflateStream deflateStream = new DeflateStream(compressedStream, CompressionMode.Compress, true);
//timer.Start();
//                        deflateStream.Write(fileBytes, 0, fileBytes.Length);
//                        deflateStream.Close();
//                        timer.Stop();

//                        // Put it into blocks
//                        compressedStream.Position = 0;
//                        long compressedSize = compressedStream.Length;
//UInt32 id = 1;
//                        while (compressedStream.Position<compressedSize)
//                        {
//                            // Grab a chunk
//                            long numBytesLeft = compressedSize - compressedStream.Position;
//long allocationSize = (numBytesLeft > MaxBlockSize) ? MaxBlockSize : numBytesLeft;
//byte[] data = new byte[allocationSize];
//compressedStream.Read(data, 0, data.Length);

//                            // Create a new block
//                            Block b = new Block(id++);
//b.Data = data;
//                            _blocks.Add(b.Number, b);
//                        }

//                        // Print some info and say we're good
//                        Console.WriteLine("{0} compressed is {1} bytes large in {2:0.000}s.", requestedFile, compressedSize, timer.Elapsed.TotalSeconds);
//                        Console.WriteLine("Sending the file in {0} blocks, using a max block size of {1} bytes.", _blocks.Count, MaxBlockSize);
//                        good = true;
//                    }
//                }
//                catch (Exception e)
//                {
//                    // Crap...
//                    Console.WriteLine("Could not prepare the file for transfer, reason:");
//                    Console.WriteLine(e.Message);

//                    // Reset a few things
//                    _blocks.Clear();
//                    checksum = null;
//                }