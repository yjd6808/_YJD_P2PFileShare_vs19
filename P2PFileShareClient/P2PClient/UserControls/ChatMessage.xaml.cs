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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace P2PClient
{
    /// <summary>
    /// ChatMessage.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ChatMessage : UserControl
    {
        public ChatMessage(string msg, HorizontalAlignment alignment)
        {
            InitializeComponent();
            TextBlock_Message.Text = msg;
            TextBlock_Time.Text = DateTime.Now.ToShortTimeString();
            TextBlock_Time.HorizontalAlignment = alignment;
            TextBlock_Message.HorizontalAlignment = alignment;
            this.HorizontalAlignment = alignment;
        }

        public ChatMessage(string msg, Brush backgroundColor, HorizontalAlignment alignment)
        {
            InitializeComponent();
            TextBlock_Message.Text = msg;
            TextBlock_Time.Text = DateTime.Now.ToShortTimeString();
            Border_Background.Background = backgroundColor;
            TextBlock_Time.HorizontalAlignment = alignment;
            TextBlock_Message.HorizontalAlignment = alignment;
            this.HorizontalAlignment = alignment;
        }

        public ChatMessage(string msg, Brush backgroundColor, Brush foregroundColor, HorizontalAlignment alignment)
        {
            InitializeComponent();
            TextBlock_Message.Text = msg;
            TextBlock_Time.Text = DateTime.Now.ToShortTimeString();
            Border_Background.Background = backgroundColor;
            TextBlock_Message.Foreground = foregroundColor;
            TextBlock_Time.HorizontalAlignment = alignment;
            TextBlock_Message.HorizontalAlignment = alignment;
            this.HorizontalAlignment = alignment;
        }
    }
}
