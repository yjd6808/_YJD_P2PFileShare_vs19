// ===============================
// @AUTHOR      : 윤정도
// @CREATE DATE : 2020-02-17 오후 9:39:11   
// @PURPOSE     : 
// ===============================


using P2PShared;
using System;
using System.Collections.Generic;
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
        public long ID { get; }
        public long FileSize { get; }
        public string FilePath { get; }

        /*=============== 다이나믹 ================*/ //유동적임
        private long m_LeftByteSize;
        private FileStream m_FileStream;
        private bool m_IsWriteOver;

        public ReceivingFile(long Id, long fileSize, string filePath)
        {
            this.ID = Id;
            this.FileSize = fileSize;
            this.FilePath = filePath;

            this.m_LeftByteSize = FileSize;
            this.m_IsWriteOver = false;
            this.m_FileStream = File.OpenWrite(@"C:\_Dev\" + Path.GetFileName(FilePath));
        }

        ~ReceivingFile()
        {
            TerminateStream();
        }

        public void WriteBytes(byte[] block)
        {
            m_FileStream.Write(block, 0, block.Length);
            m_LeftByteSize -= block.Length;

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
                this.m_FileStream.Dispose();
                this.m_FileStream = null;
            }
        }

        public bool IsWriteOver()
        {
            return m_IsWriteOver;
        }
    }
}
 