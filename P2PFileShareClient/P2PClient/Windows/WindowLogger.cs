// ===============================
// @AUTHOR      : 윤정도
// @CREATE DATE : 2020-02-08 오후 9:57:07   
// @PURPOSE     : 윈도우상 로깅 처리
// ===============================


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace P2PClient
{
    public static class WindowLogger
    {
        private static RichTextBox s_LogView;

        static public void SetViewController(RichTextBox textBox)
        {
            s_LogView = textBox;
        }

        static public void WriteLineMessage(string message)
        {
            if (s_LogView == null)
                return;

            try
            {
                s_LogView.Dispatcher.Invoke(() =>
                {
                    Paragraph newParagrph = new Paragraph();

                    Run messageTypeRun = new Run();
                    messageTypeRun.Foreground = Brushes.DarkGreen;
                    messageTypeRun.Text = "[알림] ";

                    Run messageRun = new Run();
                    messageRun.Foreground = Brushes.Black;
                    messageRun.Text = message;

                    newParagrph.Inlines.Add(messageTypeRun);
                    newParagrph.Inlines.Add(messageRun);

                    s_LogView.Document.Blocks.Add(newParagrph);
                    s_LogView.ScrollToEnd();
                });
            }
            catch
            {

            }
        }

        static public void WriteLineError(string message)
        {
            if (s_LogView == null)
                return;

            try
            {
                s_LogView.Dispatcher.Invoke(() =>
                {
                    Paragraph newParagrph = new Paragraph();

                    Run messageTypeRun = new Run();
                    messageTypeRun.Foreground = Brushes.Red;
                    messageTypeRun.Text = "[에러] ";

                    Run messageRun = new Run();
                    messageRun.Foreground = Brushes.Black;
                    messageRun.Text = message;

                    newParagrph.Inlines.Add(messageTypeRun);
                    newParagrph.Inlines.Add(messageRun);

                    s_LogView.Document.Blocks.Add(newParagrph);
                    s_LogView.ScrollToEnd();
                });
            }
            catch
            {
            }
        }

    }
}
