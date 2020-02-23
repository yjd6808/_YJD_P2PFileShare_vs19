// ===============================
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
        private object m_TransferedByteLocker;
        private int m_TransferedByteInOneSecond;
        private int m_SynchronizeTickCount;

        private int m_TransferingRate;

        private const int m_MaximumSynchronizeTickCount = 10;

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
            this.m_TransferedByteLocker = new object();
            this.m_TransferedByteInOneSecond = 0;
            this.m_SynchronizeTickCount = 0;
            this.m_TransferingRate = 0;
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
                this.m_FileStream.Close();
                this.m_FileStream.Dispose();
                this.m_FileStream = null;
                this.m_Stopwatch.Stop();
            }
        }

        /*=================================================*/

        public void AddTransferedByteInOneSecondSafe(int bytes)
        {
            lock (m_TransferedByteLocker)
                m_TransferedByteInOneSecond += bytes;
        }

        public int GetTransferedByteInOneSecondSafe()
        {
            int safeValue = 0;
            lock (m_TransferedByteLocker)
                safeValue = m_TransferedByteInOneSecond;
            return safeValue;
        }

        public void AddSynchronizeTickCount(int tick)
        {
            m_SynchronizeTickCount += tick;

            if (m_SynchronizeTickCount >= m_MaximumSynchronizeTickCount)
            {
                m_SynchronizeTickCount = 0;

                lock (m_TransferedByteLocker)
                {
                    m_TransferingRate = m_TransferedByteInOneSecond;
                    m_TransferedByteInOneSecond = 0;
                }
            }
        }

        public string GetFileName() => Path.GetFileName(FilePath);
        public long GetLeftByteSize() => m_LeftByteSize;
        public int GetLeftTimeTotalSeconds()
        {
            if (m_TransferingRate <= 0)
                return 0;

            return (int)(m_LeftByteSize / m_TransferingRate);
        }

       
        public (TransferUnit unit, float speed) GetSendingSpeed()
        {
            if (m_TransferingRate <= 0)
                return (TransferUnit.B, 0.0f);

            float speed = m_TransferingRate / 1048576.0f;
            if (speed < 1.0f)
                return (TransferUnit.KB, m_TransferingRate / 1024f);
            return (TransferUnit.MB, speed);
        }

        public double GetPercentage() => (double)(FileSize - m_LeftByteSize) / FileSize * 100D;
    }
}
