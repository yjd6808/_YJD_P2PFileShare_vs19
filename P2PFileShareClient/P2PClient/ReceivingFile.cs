// ===============================
// @AUTHOR      : 윤정도
// @CREATE DATE : 2020-02-17 오후 9:39:11   
// @PURPOSE     : 
// ===============================


using P2PShared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PClient
{
    public class ReceivingFile
    {
        /*==============  파일 정보 =================*/
        public static readonly int BlockSize = 8192;
        public long UserID { get; }
        public long FileID { get; }
        public long FileSize { get; }
        public string FilePath { get; }

        /*=============== 다이나믹 ================*/ //유동적임
        private long m_LeftByteSize;
        private FileStream m_FileStream;
        private bool m_IsWriteOver;
        private Stopwatch m_Stopwatch;

        private object m_TransferedByteLocker;
        private int m_TransferedByteInOneSecond;
        private int m_SynchronizeTickCount;

        private int m_TransferingRate;

        private const int m_MaximumSynchronizeTickCount = 10;



        public ReceivingFile(long UserId, long FileId, long fileSize, string filePath)
        {
            this.UserID = UserId;
            this.FileID = FileId;
            this.FileSize = fileSize;
            this.FilePath = filePath;
            this.m_Stopwatch = new Stopwatch();
            this.m_Stopwatch.Start();

            this.m_LeftByteSize = FileSize;
            this.m_IsWriteOver = false;
            this.m_FileStream = File.OpenWrite(Path.Combine(Setting.P2PDownloadPath, Path.GetFileName(FilePath)));

            this.m_TransferedByteLocker = new object();
            this.m_TransferedByteInOneSecond = 0;
            this.m_SynchronizeTickCount = 0;
            this.m_TransferingRate = 0;
        }

        ~ReceivingFile()
        {
            TerminateStream();
        }

        public void WriteBytes(byte[] block)
        {
            m_FileStream.Write(block, 0, block.Length);
            m_LeftByteSize -= block.Length;
            AddTransferedByteInOneSecondSafe(block.Length);


            if (m_LeftByteSize <= 0)
            {
                m_IsWriteOver = true;
                TerminateStream();
            }
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

        public bool IsWriteOver()
        {
            return m_IsWriteOver;
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

        public double GetPercentage() =>  (double)(FileSize - m_LeftByteSize) / FileSize * 100D;
    }
}
 