using Microsoft.Win32;
using System;
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

namespace P2PClient
{
    /// <summary>
    /// Window1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SettingWindow : Window
    {
        private static SettingWindow s_SettingWindow;

        public SettingWindow()
        {
            InitializeComponent();
            s_SettingWindow = this;
            TextBox_DownloadDirectory.Text =  Setting.P2PDownloadPath;
            TextBox_StartDirectory.Text = Setting.P2PStartPath;
        }
        public static SettingWindow Get() => s_SettingWindow;
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void Button_StartDirectorySelect_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                    TextBox_StartDirectory.Text = dialog.SelectedPath;
            }
        }

        private void Button_DownloadDirectorySelect_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                    TextBox_DownloadDirectory.Text = dialog.SelectedPath;
            }
        }

        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Button_Save_Click(object sender, RoutedEventArgs e)
        {
            if (TextBox_DownloadDirectory.Text.Length <= 0)
                MessageBox.Show("다운로드 경로를 입력해주세요.");
            
            if (TextBox_StartDirectory.Text.Length <= 0)
                MessageBox.Show("처음 경로를 입력해주세요.");


            Setting.P2PDownloadPath = TextBox_DownloadDirectory.Text;
            Setting.P2PStartPath = TextBox_StartDirectory.Text;
            Setting.Save();
            this.Close();
        }

        private void Button_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            s_SettingWindow = null;
        }
    }
}
