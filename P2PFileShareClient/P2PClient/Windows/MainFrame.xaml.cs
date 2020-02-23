using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using P2PClient;
using P2PShared;


namespace P2PClient
{
    /// <summary>
    /// MainFrame.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainFrame : Window
    {
        private static MainFrame s_MainFrame;
        public static MainFrame Get() => s_MainFrame;
        public static Dispatcher GetDispatcher() => s_MainFrame.Dispatcher;

        private MasterClient m_MasterClient;
        public List<P2PWindow> P2PWindows;

        public MainFrame()
        {
            InitializeComponent();

            WindowLogger.SetViewController(RichTextBox_Log);

            s_MainFrame = this;
            m_MasterClient = MasterClient.GetInstance();
            P2PWindows = new List<P2PWindow>();

            m_MasterClient.OnOtherClientAdded += M_MasterClient_OnOtherClientAdded;
            m_MasterClient.OnOtherClientUpdated += M_MasterClient_OnOtherClientUpdated;
            m_MasterClient.OnOtherClientDisconnected += M_MasterClient_OnOtherClientDisconnected; 
            m_MasterClient.OnServerConnect += M_MasterClient_OnServerConnect;
            m_MasterClient.OnServerDisconnect += M_MasterClient_OnServerDisconnect;
            m_MasterClient.OnMyInfoUpdated += M_MasterClient_OnMyInfoUpdated;
            m_MasterClient.OnOtherClientP2PConnected += M_MasterClient_OnOtherClientP2PConnected;
            m_MasterClient.OnOtherClientP2PMessageArrived += M_MasterClient_OnOtherClientP2PMessageArrived;
            m_MasterClient.OnOtherClientP2PRequestPathArrived += M_MasterClient_OnOtherClientP2PRequestPathArrived;
            m_MasterClient.OnOtherClientP2PDisconnected += M_MasterClient_OnOtherClientP2PDisconnected;

            m_MasterClient.OnStartSendingFile += M_MasterClient_OnStartSendingFile;
            m_MasterClient.OnStartReceivingFile += M_MasterClient_OnStartReceivingFile;
            m_MasterClient.OnSynchronizingReceivingFile += M_MasterClient_OnSynchronizingReceivingFile;
            m_MasterClient.OnSynchronizingSendingFile += M_MasterClient_OnSynchronizingSendingFile;
            m_MasterClient.OnFinishSendingFile += M_MasterClient_OnFinishSendingFile;
            m_MasterClient.OnFinishReceivingFile += M_MasterClient_OnFinishReceivingFile;
            m_MasterClient.OnReceiveTransferingError += M_MasterClient_OnReceiveTransferingError;

            if (Environment.CurrentDirectory.Contains("Debug"))
                Label_Version.Content = "디버그 (1.0v)";
            else
                Label_Version.Content = "릴리즈 (1.0v)";

            m_MasterClient.Init();
            Setting.Load();
        }

      

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            m_MasterClient.DisconnectToMainServer();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void Button_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
                if (m_MasterClient.IsConnectedToP2PServer())
                    m_MasterClient.DisconnectToMainServer();
                else
                    m_MasterClient.ConnectToMainServer();
        }

        private void ListBox_ClientList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            P2PClientInfo selectedP2PClient = ListBox_ClientList.SelectedItem as P2PClientInfo;
            if (selectedP2PClient != null)
                Button_ConnectToOtherClient.IsEnabled = true;
            RefreshDetails();
        }

        private void Button_ConnectToOtherClient_Click(object sender, RoutedEventArgs e)
        {
            P2PClientInfo selectedP2PClient = ListBox_ClientList.SelectedItem as P2PClientInfo;
            if (selectedP2PClient != null)
                m_MasterClient.ConnectToOtherClient(selectedP2PClient);
        }


    }
}
