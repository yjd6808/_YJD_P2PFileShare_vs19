// ===============================
// @AUTHOR      : 윤정도
// @CREATE DATE : 2020-02-08 오후 6:08:35   
// @PURPOSE     : 쓰레드 세이프 로거
// ===============================


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PServer.Util
{
    public class ThreadSafeLogger
    {
        private static object lockObject = new object();

        static public void WriteLine(string message)
        {
            lock (lockObject)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(message);
            }
        }

        static public void WriteLineMessage(string message)
        {
            lock(lockObject)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("[알림] ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(message);
            }
        }

        static public void WriteLineWarning(string message)
        {
            lock (lockObject)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("[경고] ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(message);
            }
        }

        static public void WriteLineError(string message)
        {
            lock (lockObject)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("[오류] ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(message);
            }
        }

        static public void WriteLineClientInfo(P2PShared.P2PClientInfo p2PClientInfo)
        {
            lock (lockObject)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("[정보] ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("ID : " + p2PClientInfo.ID);

                Console.ForegroundColor = ConsoleColor.Cyan; 
                Console.Write("[정보] ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("Name : " + p2PClientInfo.Name);

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("[정보] ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("EEP : " + p2PClientInfo.ExternalEndpoint);

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("[정보] ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("IEP : " + p2PClientInfo.InternalEndpoint);

                
                if (p2PClientInfo.InternalAddresses.Count > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write("[정보] ");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("IIP : ");

                    for (int i =0; i < p2PClientInfo.InternalAddresses.Count; i++)
                    {
                        if (i > 0)
                            Console.Write(" / ");
                        Console.Write(p2PClientInfo.InternalAddresses[i].ToString());
                    }
                    Console.WriteLine();
                }
            }
        }
    }
}
