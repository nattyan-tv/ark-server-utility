﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// update_checker.xaml の相互作用ロジック
    /// </summary>
    public partial class update_checker : Window
    {
        public update_checker()
        {
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
            util_log.Text += "\n[UPDATE - " + DateTime.Now.ToString() + "]チェック開始...";
            MainWindow mw = new MainWindow();
            string latest_version = mw.IpcConnect("webapi system");
            util_log.Text += "\n[UPDATE - " + DateTime.Now.ToString() + "]最新バージョン情報を取得:v" + latest_version;
            string current_version = mw.version;
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
