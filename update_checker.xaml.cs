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
    /// update_checker.xaml の相互作用ロジック
    /// </summary>
    public partial class update_checker : Window
    {
        public int sys_port;
        public string sys_version;
        // IPC通信で、出力を返す
        public string IpcConnect(string text, int port)
        {
            using (Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                client.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), port));
                var data = Encoding.UTF8.GetBytes(text);
                client.Send(BitConverter.GetBytes(data.Length));
                client.Send(data);
                data = new byte[4];
                client.Receive(data, data.Length, SocketFlags.None);
                Array.Reverse(data);
                data = new byte[BitConverter.ToInt32(data, 0)];
                client.Receive(data, data.Length, SocketFlags.None);
                return Encoding.UTF8.GetString(data);
            }
        }

        // IPC通信で、出力を返さない
        public void IpcSend(string text, int port)
        {
            using (Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                client.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), port));
                var data = Encoding.UTF8.GetBytes(text);
                client.Send(BitConverter.GetBytes(data.Length));
                client.Send(data);
                return;
            }
        }

        public update_checker(int port, string version)
        {
            sys_port = port;
            sys_version = version;
            Console.WriteLine("ポート:" + port);
            InitializeComponent();
        }

        private Process OpenUrl(string url)
        {
            ProcessStartInfo pi = new ProcessStartInfo()
            {
                FileName = url,
                UseShellExecute = true,
            };

            return Process.Start(pi);

        }

        private void open_github(object sender, RoutedEventArgs e)
        {
            OpenUrl("https://github.com/nattyan-tv/ark-server-utility");
        }

        private void check_update(object sender, RoutedEventArgs e)
        {
            util_log.Text = "";
            util_log.Text += "[UPDATE - " + DateTime.Now.ToString() + "]チェック開始...";
            InitializeComponent();
            string latest_version = IpcConnect("webapi system", sys_port);
            util_log.Text += "\n[UPDATE - " + DateTime.Now.ToString() + "]最新バージョン情報を取得:v" + latest_version;
            string current_version = sys_version;
            util_log.Text += "\n[UPDATE - " + DateTime.Now.ToString() + "]現行バージョン情報を取得:v" + current_version;
            string err = "";
            if (latest_version.Contains("ERR:"))
            {
                err = latest_version.Substring(4);
                Console.WriteLine("バージョン情報取得時にエラーが発生しました。\n" + err);
                util_log.Text += "\n[UPDATE - " + DateTime.Now.ToString() + "]最新バージョン取得時にエラー発生\n" + err;
                util_log.Text += "\n[UPDATE - " + DateTime.Now.ToString() + "]アップデートを中断します。";
                System.Windows.Forms.MessageBox.Show("バージョン情報取得時にエラーが発生しました。\nインターネットに接続されていることを確認してください。\n「GitHubレポジトリ」からバージョンを確認することもできます。", "ARK Server Utility", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Hand);
                return;
            }
            //major minor build
            string[] lv = latest_version.Split('.');
            string[] cv = current_version.Split('.');
            bool need_update = false;
            if (latest_version == current_version)
            {
                need_update = false;
            }
            else
            {
                if (int.Parse(lv[0]) > int.Parse(cv[0]))
                {
                    need_update = true;
                }
                else if (int.Parse(lv[1]) > int.Parse(cv[1]))
                {
                    need_update = true;
                }
                else if (int.Parse(lv[2]) > int.Parse(cv[2]))
                {
                    need_update = true;
                }
            }
            if (need_update)
            {
                util_log.Text += "\n[UPDATE - " + DateTime.Now.ToString() + "]アップデートが可能です。";
                System.Windows.Forms.MessageBox.Show("アップデートが可能です。\n最新バージョン:" + latest_version, "ARK Server Utility", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Asterisk);
                return;
            }
            else
            {
                util_log.Text += "\n[UPDATE - " + DateTime.Now.ToString() + "]アップデートは不要です。";
                System.Windows.Forms.MessageBox.Show("アップデートは不要です。\n現行バージョン:" + current_version, "ARK Server Utility", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Asterisk);
                return;
            }
        }
    }
}
