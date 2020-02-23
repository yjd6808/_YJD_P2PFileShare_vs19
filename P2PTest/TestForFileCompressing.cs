using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using P2PShared;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace P2PTest
{
    public class Block
    {
        public UInt32 Number { get; set; }
        public byte[] Data { get; set; } = new byte[0];

        #region Constructors
        // Creates a new block of data w/ the supplied number
        public Block(UInt32 number = 0)
        {
            Number = number;
        }

        // Creates a Block from a byte array
        public Block(byte[] bytes)
        {
            // First four bytes are the number
            Number = BitConverter.ToUInt32(bytes, 0);

            // Data starts at byte 4
            Data = bytes.Skip(4).ToArray();
        }
        #endregion // Constructors

        public override string ToString()
        {
            // Take some of the first few bits of data and turn that into a string
            String dataStr;
            if (Data.Length > 8)
                dataStr = Encoding.ASCII.GetString(Data, 0, 8) + "...";
            else
                dataStr = Encoding.ASCII.GetString(Data, 0, Data.Length);

            return string.Format(
                "[Block:\n" +
                "  Number={0},\n" +
                "  Size={1},\n" +
                "  Data=`{2}`]",
                Number, Data.Length, dataStr);
        }

        // Returns the data in the block as a byte array
        public byte[] GetBytes()
        {
            // Convert meta-data
            byte[] numberBytes = BitConverter.GetBytes(Number);

            // Join all the data into one bigger array
            byte[] bytes = new byte[numberBytes.Length + Data.Length];
            numberBytes.CopyTo(bytes, 0);
            Data.CopyTo(bytes, 4);

            return bytes;
        }
    }



    [TestClass]
    public class TestForFileCompressing
    {
        

        MD5                                 _hasher;
        string                              _fileName;
        string                              _directory;
        Dictionary<uint, Block>             _blocks;
        byte[]                              _checkSum;
        uint                                _fileSize;


        const uint                          _maxBlockSize = 8 * 1024;   // 8KB
        [TestMethod, Ignore]
        public void TestMain()
        {
            _hasher = MD5.Create();
            _directory = @"C:\";
            _fileName = @"YodaSyndrome - 몽환적인 노래 모음집 (2017-03-09).mp3";
            _blocks = new Dictionary<UInt32, Block>();


            //파일 압축 테스트
            CompressedFile(_fileName);
        }

        private void CompressedFile(string requestedFile)
        {
            Console.WriteLine("Preparing the file to send...");
            _fileSize = 0;

            try
            {
                // Read it in & compute a checksum of the original file
                byte[] fileBytes = File.ReadAllBytes(Path.Combine(_directory, requestedFile));
                _checkSum = _hasher.ComputeHash(fileBytes);
                _fileSize = Convert.ToUInt32(fileBytes.Length);
                Console.WriteLine("{0} is {1} bytes.{2:0.####} kilo bytes {3:0.####} mega bytes {4:0.####} giga bytes", requestedFile, _fileSize, _fileSize.BytesToKilobytes(), _fileSize.BytesToMegabytes(), _fileSize.BytesToGigabytes());

                // Compress it
                Stopwatch timer = new Stopwatch();
                using (MemoryStream compressedStream = new MemoryStream())
                {
                    // Perform the actual compression
                    DeflateStream deflateStream = new DeflateStream(compressedStream, CompressionMode.Compress, true);
                    timer.Start();
                    deflateStream.Write(fileBytes, 0, fileBytes.Length);
                    deflateStream.Close();
                    timer.Stop();

                    // Put it into blocks
                    compressedStream.Position = 0;
                    long compressedSize = compressedStream.Length;
                    UInt32 id = 1;
                    while (compressedStream.Position < compressedSize)
                    {
                        // Grab a chunk
                        long numBytesLeft = compressedSize - compressedStream.Position;
                        long allocationSize = (numBytesLeft > _maxBlockSize) ? _maxBlockSize : numBytesLeft;
                        byte[] data = new byte[allocationSize];
                        compressedStream.Read(data, 0, data.Length);

                        // Create a new block
                        Block b = new Block(id++);
                        b.Data = data;
                        _blocks.Add(b.Number, b);
                    }

                    // Print some info and say we're good
                    Console.WriteLine("{0} compressed is {1} bytes large in {2:0.000}s.", requestedFile, compressedSize, timer.Elapsed.TotalSeconds);
                    Console.WriteLine("Sending the file in {0} blocks, using a max block size of {1} bytes.", _blocks.Count, _maxBlockSize);
                }
            }
            catch (Exception e)
            {
                // Crap...
                Console.WriteLine("Could not prepare the file for transfer, reason:");
                Console.WriteLine(e.Message);

                // Reset a few things
                _blocks.Clear();
                _checkSum = null;
            }
        }


        //Preparing the file to send...
        //YodaSyndrome - 몽환적인 노래 모음집(2017-03-09).mp3 is 42336621 bytes.
        //YodaSyndrome - 몽환적인 노래 모음집(2017-03-09).mp3 compressed is 41894838 bytes large in 1.990s.
        //Sending the file in 5115 blocks, using a max block size of 8192 bytes.
    }
}
