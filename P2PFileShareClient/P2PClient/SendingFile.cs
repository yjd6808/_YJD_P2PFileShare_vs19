﻿// ===============================
// @AUTHOR      : 윤정도
// @CREATE DATE : 2020-02-17 오후 9:28:36   
// @PURPOSE     : 
// ===============================


using P2PShared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace P2PClient
{
    public class SendingFile
    {
        /*==============  파일 정보 =================*/
        public static readonly int BlockSize = 8192;
        public long UserID { get; }
        public long FileID { get; }
        public long FileSize { get; }
        public string FilePath { get; }

        /*=============== 다이나믹 ================*/ //유동적임
        private int m_SendingBlockCount;
        private long m_LeftByteSize;
        private FileStream m_FileStream;
        private Stopwatch m_Stopwatch;

        public SendingFile(long userId, string filePath)
        {
            this.FilePath = filePath;
            this.UserID = userId;
            this.FileID = DateTime.Now.Ticks;
            this.FileSize = new FileInfo(filePath).Length;
            this.m_Stopwatch = new Stopwatch();
            this.m_Stopwatch.Start();

            this.m_SendingBlockCount = 0;
            this.m_LeftByteSize = FileSize;

            this.m_FileStream = File.OpenRead(filePath);
        }

        ~SendingFile()
        {
            TerminateStream();
        }

        /// <summary>
        /// 데이터 블록을 가져옴
        /// </summary>
        /// <returns>null : 모든 전송이 끝났다.</returns>
        public byte[] GetByteBlock()
        {
            byte[] block = new byte[BlockSize];
            int readByte = m_FileStream.Read(block, 0, BlockSize);

            m_SendingBlockCount++;
            m_LeftByteSize -= readByte;

            if (readByte == 0)
                return null;

            return block;
        }

        public void TerminateStream()
        {
            if (this.m_FileStream != null)
            {
                this.m_FileStream.Dispose();
                this.m_FileStream = null;
                this.m_Stopwatch.Stop();
            }
        }

        /*=================================================*/

        public string GetFileName() => Path.GetFileName(FilePath);
        public long GetLeftByteSize() => m_LeftByteSize;
        public int GetLeftTimeTotalSeconds()
        {
            return 0;
        }

        public int GetSendingSpeedPerKB()
        {
            return 0;
        }

        public float GetPercentage()
        {
            //Math.Round(value, 1)
            return 0;
        }
    }
}
