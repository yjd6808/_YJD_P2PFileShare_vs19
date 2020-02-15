using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using P2PShared;

namespace P2PServer
{
    class Program
    {
        private enum CtrlType
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }

        private delegate bool SetConsoleCtrlEventHandler(CtrlType sig);
        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(SetConsoleCtrlEventHandler handler, bool add);

        static P2PServer s_Server;
        static void Main(string[] args)
        {
            SetConsoleCtrlHandler(Handler, true);
            s_Server = P2PServer.GetInstance();

            Task.Run(() =>
            {
                s_Server.Open();
            });
            e: Console.WriteLine("'종료' 입력시 서버가 꺼집니다.");
            if (Console.ReadLine() == "종료")
            {
                s_Server.BroadcastTCP(new Notification(NotificationType.ServerShutDown, null));
                Environment.Exit(0);
            }
            else
            {
                goto e;
            }
        }

        private static bool Handler(CtrlType signal)
        {
            switch (signal)
            {
                case CtrlType.CTRL_BREAK_EVENT:
                case CtrlType.CTRL_C_EVENT:
                case CtrlType.CTRL_LOGOFF_EVENT:
                case CtrlType.CTRL_SHUTDOWN_EVENT:
                case CtrlType.CTRL_CLOSE_EVENT:
                    s_Server.BroadcastTCP(new Notification(NotificationType.ServerShutDown, null));
                    Environment.Exit(0);
                    return false;

                default:
                    return false;
            }
        }
    }
}
