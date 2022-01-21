using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Text.Json;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Text;
using System.Net.Sockets;
using System.Management;
using System.Threading;
using CoreRCON;
using SteamQueryNet.Interfaces;

namespace ark_server_utility
{
    /// <summary>
    /// can_use_port.xaml の相互作用ロジック
    /// </summary>
    public partial class can_use_port : Window
    {
        public int port = MainWindow.port;

        public can_use_port()
        {
            InitializeComponent();
        }



        private void open_help_port(object sender, RoutedEventArgs e)
        {
            MainWindow.OpenUrl("https://nattyan-tv.github.io/ark-server-utility/notes/port");
        }
        private void check_port(object sender, RoutedEventArgs e)
        {
            // IPC通信を開始するためのポート検索するスクリプトを実行する
            var ipc_port = new Process
            {
                StartInfo = new ProcessStartInfo("python/search_port.exe")
                {
                    Arguments = "2",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            ipc_port.Start();
            StreamReader ports = ipc_port.StandardOutput;
            util_log.Text = ports.ReadLine();
            ipc_port.WaitForExit();
            ipc_port.Close();
        }
    }
}
