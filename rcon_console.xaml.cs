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

namespace ark_server_utility
{
    /// <summary>
    /// rcon_console.xaml の相互作用ロジック
    /// </summary>
    public partial class rcon_console : Window
    {
        public int port;
        public string password;

        public rcon_console(int pt, string pw)
        {
            InitializeComponent();
            port = pt;
            password = pw;
            log_main.Text = "ポート番号:" + port.ToString();
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Visibility = Visibility.Collapsed;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            log_main.Text += "[" + DateTime.Now.ToString() + "] - SEND:" + command.Text;
            string response = MainWindow.IpcConnect("rcon " + port + "," + password + "," + command.Text);
            log_main.Text += "[" + DateTime.Now.ToString() + "] - RECV:" + response;
        }

        private void show_cmd(object sender, RoutedEventArgs e)
        {
            MainWindow.OpenUrl("https://ark.fandom.com/ja/wiki/Console_Commands");
        }

        private void cmd_Broadcast(object sender, RoutedEventArgs e)
        {
            command.Text = "Broadcast メッセージ";
        }

        private void cmd_ServerChat(object sender, RoutedEventArgs e)
        {
            command.Text = "ServerChat メッセージ";
        }

        private void cmd_SaveWorld(object sender, RoutedEventArgs e)
        {
            command.Text = "SaveWorld";
        }

        private void cmd_DoExit(object sender, RoutedEventArgs e)
        {
            command.Text = "DoExit";
        }
    }
}
