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

        
        public update_checker()
        {
            InitializeComponent();
        }

        private void open_github(object sender, RoutedEventArgs e)
        {
            MainWindow.OpenUrl("https://github.com/nattyan-tv/ark-server-utility");
        }

        private void check_update(object sender, RoutedEventArgs e)
        {
            util_log.Text = "";
            util_log.Text += "[UPDATE - " + DateTime.Now.ToString() + "]チェック開始...";
            InitializeComponent();
            string latest_version = MainWindow.IpcConnect("webapi system");
            util_log.Text += "\n[UPDATE - " + DateTime.Now.ToString() + "]最新バージョン情報を取得:v" + latest_version;
            string current_version = MainWindow.version;
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
                DialogResult rt = System.Windows.Forms.MessageBox.Show("アップデートが可能です。\n最新バージョン:" + latest_version + "\nアップデートしますか？", "ARK Server Utility", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Asterisk);
                if (rt == System.Windows.Forms.DialogResult.Yes)
                {
                    DialogResult rt2 = System.Windows.Forms.MessageBox.Show("保存されていない項目は破棄されます。\n本当によろしいですか？", "ARK Server Utility", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Asterisk);
                    if (rt2 == System.Windows.Forms.DialogResult.Yes)
                    {
                        var updater = new Process
                        {
                            StartInfo = new ProcessStartInfo("python/updater.exe")
                            {
                                Arguments = "true",
                                UseShellExecute = false,
                                RedirectStandardOutput = true,
                                CreateNoWindow = true
                            }
                        };
                        updater.Start();
                        Environment.Exit(0);
                        return;
                    }

                }
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
