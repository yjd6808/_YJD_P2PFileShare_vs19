﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using P2PShared;
using P2PClient;
using System.IO;
using System.Diagnostics;

namespace P2PClient
{
    /// <summary>
    /// P2PWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class P2PWindow : Window
    {
        private MasterClient m_MasterClient;

        private Dictionary<long, SendingFileMessage>    m_SendingFileMessageControls;
        private Dictionary<long, ReceivingFileMessage>  m_ReceivingFileMessageControls;

        public P2PClientInfo ConnectedClient;
        public string CurrentPeerDirectory;
        public long ID;


        public P2PWindow(P2PClientInfo p2PClientInfo)
        {
            this.ConnectedClient = p2PClientInfo;
            this.ID = p2PClientInfo.ID;
            this.m_MasterClient = MasterClient.GetInstance();
            this.CurrentPeerDirectory = Setting.P2PStartPath;
            this.m_SendingFileMessageControls = new Dictionary<long, SendingFileMessage>();
            this.m_ReceivingFileMessageControls = new Dictionary<long, ReceivingFileMessage>();


            InitializeComponent();

            if (Environment.CurrentDirectory.Contains("Debug"))
                Label_Version.Content = "디버그 (1.0v)";
            else
                Label_Version.Content = "릴리즈 (1.0v)";

            RefreshFavroites();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ConnectedToPeer();
            SendMessageToPeer(new P2PRequestPath(
                this.m_MasterClient.MyInfo.ID,
                this.ConnectedClient.ID,
                Setting.P2PStartPath));
        }



        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SendMessageToPeer(new P2PNotification(m_MasterClient.MyInfo.ID, P2PNotificatioyType.Disconnected));
            P2PWindow find = MainFrame.Get().P2PWindows.FirstOrDefault(x => x == this);
            if (find != null)
                MainFrame.Get().P2PWindows.Remove(this);
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

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (TextBox_Chat.IsFocused)
                    SendText();
                else
                    TextBox_Chat.Focus();
            }
        }

        private void Button_Send_Click(object sender, RoutedEventArgs e)
        {
            SendText();
        }


      
        private void Button_Download_Click(object sender, RoutedEventArgs e)
        {
            P2PPath selectedItem = ListView_PathList.SelectedItem as P2PPath;

            if (selectedItem == null)
                return;

            if (ConnectedClient == null || ConnectedClient.IsP2PConnected == false)
            {
                WriteNotifyingMessage("상대방과 연결되어있지 않습니다.");
                return;
            }

            DownloadFile(selectedItem);
        }

        private void ListView_ContextMemu_Download_Click(object sender, RoutedEventArgs e)
        {
            P2PPath selectedItem = ListView_PathList.SelectedItem as P2PPath;

            if (selectedItem == null)
                return;

            if (ConnectedClient == null || ConnectedClient.IsP2PConnected == false)
            {
                WriteNotifyingMessage("상대방과 연결되어있지 않습니다.");
                return;
            }

            DownloadFile(selectedItem);
        }

        private void ListView_ContextMemu_Favorite_Click(object sender, RoutedEventArgs e)
        {
            P2PPath selectedItem = ListView_PathList.SelectedItem as P2PPath;

            if (selectedItem == null)
                return;

            if (selectedItem.PathType != P2PPathType.Directory)
                return;

            AddFavorite(selectedItem);
        }

        private void Button_Favorite_Click(object sender, RoutedEventArgs e)
        {
            P2PPath selectedItem = ListView_PathList.SelectedItem as P2PPath;

            if (selectedItem == null)
                return;

            if (selectedItem.PathType != P2PPathType.Directory)
                return;

            AddFavorite(selectedItem);
        }

       

        private void Button_Refresh_Click(object sender, RoutedEventArgs e)
        {
            if (ConnectedClient == null || ConnectedClient.IsP2PConnected == false)
            {
                WriteNotifyingMessage("상대방과 연결되어있지 않습니다.");
                return;
            }

            SendMessageToPeer(new P2PRequestPath(
               this.m_MasterClient.MyInfo.ID,
               this.ConnectedClient.ID,
               this.CurrentPeerDirectory));
        }

        private void Button_Home_Click(object sender, RoutedEventArgs e)
        {
            if (ConnectedClient == null || ConnectedClient.IsP2PConnected == false)
            {
                WriteNotifyingMessage("상대방과 연결되어있지 않습니다.");
                return;
            }

            SendMessageToPeer(new P2PRequestPath(
               this.m_MasterClient.MyInfo.ID,
               this.ConnectedClient.ID,
               Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)));
        }

        private void ListView_PathList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            P2PPath selectedItem = ListView_PathList.SelectedItem as P2PPath;
            if (selectedItem == null)
                return;

            if (ConnectedClient == null || ConnectedClient.IsP2PConnected == false)
            {
                WriteNotifyingMessage("상대방과 연결되어있지 않습니다.");
                return;
            }

            if (selectedItem.PathType == P2PPathType.Previous)
            {
                SendMessageToPeer(new P2PRequestPath(
                    this.m_MasterClient.MyInfo.ID,
                    this.ConnectedClient.ID,
                    Directory.GetParent(CurrentPeerDirectory).FullName));
            }
            else if (selectedItem.PathType == P2PPathType.Directory)
            {
                SendMessageToPeer(new P2PRequestPath(
                    this.m_MasterClient.MyInfo.ID,
                    this.ConnectedClient.ID,
                    selectedItem.FullPath));
            }

        }

        private void ListView_PathList_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            P2PPath selectedItem = ListView_PathList.SelectedItem as P2PPath;

            if (selectedItem == null)
                return;

            switch (selectedItem.PathType)
            {
                case P2PPathType.Previous:
                    ListView_ContextMenu_Download.IsEnabled = false;
                    ListView_ContextMenu_SetFavorite.IsEnabled = false;
                    break;
                case P2PPathType.Directory:
                    ListView_ContextMenu_Download.IsEnabled = false;
                    ListView_ContextMenu_SetFavorite.IsEnabled = true;
                    break;
                default:
                    ListView_ContextMenu_Download.IsEnabled = true;
                    ListView_ContextMenu_SetFavorite.IsEnabled = false;
                    break;
            }
        }

        private void Button_Search_Click(object sender, RoutedEventArgs e)
        {
            SerachPath();
        }

        private void ComboBox_InputPath_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                SerachPath(); 
        }

        private void Button_RemoveFavorite_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            //부모가 같은 그리드 객체이고 버튼은 1번째 인덱스이므로 텍스트블록은 당연히 0번째 인덱스이다.
            var btn =  sender as Button;
            Grid parent = btn.Parent as Grid;

            if (parent == null)
                return;

            TextBlock tb = parent.Children[0] as TextBlock;

            if (tb == null)
                return;

            RemoveFavorite(tb.Text);
        }

        private void ListView_PathList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            P2PPath selectedItem = ListView_PathList.SelectedItem as P2PPath;

            if (selectedItem == null)
                return;

            switch (selectedItem.PathType)
            {
                case P2PPathType.Previous:
                case P2PPathType.Directory:
                    Button_Download.IsEnabled = false;
                    break;
                default:
                    Button_Download.IsEnabled = true;
                    break;
            }
        }

        private void Button_DownloadFolderOpen_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(Setting.P2PDownloadPath);
        }

        private void Button_Setting_Click(object sender, RoutedEventArgs e)
        {
            SettingWindow settingWindow = SettingWindow.Get();
            if (settingWindow != null)
            {
                settingWindow.Activate();
                settingWindow.Focus();
                settingWindow.BringIntoView();
            }
            else
            {
                settingWindow = new SettingWindow();
                settingWindow.Show();
            }
        }


    }
}
