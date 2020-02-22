// ===============================
// @AUTHOR      : 윤정도
// @CREATE DATE : 2020-02-19 오후 10:06:48   
// @PURPOSE     : 
// ===============================


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace P2PClient
{
    public partial class MasterClient
    {
        public void StartReceivingSynchronizingThread()
        {
            
            m_ReceivingFileSynchronizingThread = new Thread(new ThreadStart(() =>
            {
                while (m_IsReceivingFileSynchronizingThreadStart)
                {
                    /* ====================================================== */
                    /*                      쓰레드 몸체                       */
                    /* ====================================================== */

                    lock (receivingFileListLocker)  
                    {
                        foreach (var SameUserIDFileList in ReceivingFileList.Values)
                        {
                            foreach (var File in SameUserIDFileList.Values)
                            {
                                if (OnSynchronizingReceivingFile != null)
                                    OnSynchronizingReceivingFile.Invoke(null, File);
                            }
                        }
                    }
                    Thread.Sleep(100);
                }

            }));

            m_ReceivingFileSynchronizingThread.IsBackground = true;

            if (m_IsReceivingFileSynchronizingThreadStart == false)
            {
                m_IsReceivingFileSynchronizingThreadStart = true;
                m_ReceivingFileSynchronizingThread.Start();
            }
        }

        public void StartSendingSynchronizingThread()
        {
            
            m_SendingFileSynchronizingThread = new Thread(new ThreadStart(() =>
            {
                while (m_IsSendingFileSynchronizingThreadStart)
                {

                    /* ====================================================== */
                    /*                      쓰레드 몸체                       */
                    /* ====================================================== */

                    lock (sendingFileListLocker)
                    {
                        foreach (var SameUserIDFileList in SendingFileList.Values)
                        {
                            foreach(var File in SameUserIDFileList.Values)
                            { 
                                if (OnSynchronizingSendingFile != null)
                                    OnSynchronizingSendingFile.Invoke(null, File);
                            }
                        }
                    }
                    Thread.Sleep(100);
                }
            }));

            m_SendingFileSynchronizingThread.IsBackground = true;

            if (m_IsSendingFileSynchronizingThreadStart == false)
            {
                m_IsSendingFileSynchronizingThreadStart = true;
                m_SendingFileSynchronizingThread.Start();
            }
        }


    }
}
