using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Text.Json;
using System.Collections.Generic;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace ark_server_utility
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<arg_data> args = new List<arg_data>();
        public MainWindow()
        {
            InitializeComponent();
            var dict = new Dictionary<string, List<string>>();

            // 起動オプションの引数をぉおお！！！ここにぃいいい！！！つっこむうぅうう！！！ぜんぶうぅうううう！！（地獄）

            // dict.Add("allowansel", new List<string>(){"さけ","サバ","はまち"});
            arg_setting.Items.Add(new arg_data { arg = "aaaa", detail = "bbb"});

            if (!File.Exists(@"settings.json"))
            {
                string myPythonApp = "python/settings.py";
                var myProcess = new Process
                {
                    StartInfo = new ProcessStartInfo("python")
                    {
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        Arguments = myPythonApp + " first",
                        CreateNoWindow = true
                    }
                };
                myProcess.Start();
                myProcess.WaitForExit();
                myProcess.Close();
                // string[,] settings_data = new string[99, 3];
            }
            string read_set = "python/settings.py";
            var read_pro = new Process
            {
                StartInfo = new ProcessStartInfo("python")
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    Arguments = read_set + " read 1",
                    CreateNoWindow = true
                }
            };
            read_pro.Start();
            StreamReader read_str = read_pro.StandardOutput;
            string data_1 = read_str.ReadLine();
            read_pro.WaitForExit();
            read_pro.Close();
            string[] arr = data_1.Split(',');
            label_name.Content = "サーバー名：" + arr[0];
            label_map.Content = "マップ名：" + arr[1];
            label_dir.Content = "ディレクトリ：" + arr[2];
            var value_pro = new Process
            {
                StartInfo = new ProcessStartInfo("python")
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    Arguments = "python/settings.py value",
                    CreateNoWindow = true
                }
            };
            value_pro.Start();
            StreamReader val_read = value_pro.StandardOutput;
            string value = val_read.ReadLine();
            value_pro.WaitForExit();
            value_pro.Close();
            if (value == "1")
            {
                del_list.IsEnabled = false;
            }
            if (!File.Exists(@arr[2] + @"\\ShooterGame\\Binaries\\Win64\\ShooterGameServer.exe"))
            {
                // サーバーデータがインストールされていない場合の処理
                start_server.IsEnabled = false;
                install_server.Content = "インストール";
                var version_pro = new Process
                {
                    StartInfo = new ProcessStartInfo("python")
                    {
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        Arguments = "python/version.py version 0",
                        CreateNoWindow = true
                    }
                };
                version_pro.Start();
                StreamReader ver_read = version_pro.StandardOutput;
                string version = ver_read.ReadLine();
                version_pro.WaitForExit();
                version_pro.Close();
                latest_version.Content = "配信されている最新バージョン：" + version;
            }
            else
            {
                // サーバーデータがインストールされている場合の処理
                start_server.IsEnabled = true;
                install_server.Content = "アンインストール";
                var version_pro = new Process
                {
                    StartInfo = new ProcessStartInfo("python")
                    {
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        Arguments = "python/webapi.py version 1",
                        CreateNoWindow = true
                    }
                };
                version_pro.Start();
                StreamReader ver_read = version_pro.StandardOutput;
                string version = ver_read.ReadLine();
                Console.WriteLine(version);
                version_pro.WaitForExit();
                version_pro.Close();
                string[] vers = version.Split(',');
                latest_version.Content = "配信されている最新バージョン：" + vers[0];
                current_version.Content = "インストールされているバージョン：" + vers[1];
                update_bt.IsEnabled = true;

                if(float.Parse(vers[0]) > float.Parse(vers[1]))
                {
                    update_bt.Content = "アップデート";
                }
                else
                {
                    update_bt.Content = "ファイルのチェック";
                }
                server_pass_bool.IsEnabled = true;
                admin_pass.IsEnabled = true;
                game_port.IsEnabled = true;
                query_port.IsEnabled = true;
                arg_setting_box.IsEnabled = true;
            }
            server_name.Text = arr[0];
            map.Text = arr[1];
            server_dir.Text = arr[2];
            server_list.Items.Add(arr[0]);
            server_list.Text = arr[0];
            main_pbar.Value = 100;
            main_ptext.Content = "ARK: Server Utility";
        }
        private void start_debug(object sender, RoutedEventArgs e)
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo(@server_dir.Text + @"\\ShooterGame\\Binaries\\Win64\\ShooterGameServer.exe", map.Text + "?listen?SessionName=" + server_name.Text + "?ServerPassword=" + join_pass.Password + "?ServerAdminPassword=" + admin_pass.Password + "?Port=7777?QueryPort=27015?MaxPlayers=3");
            Process.Start(processStartInfo);
        }
        private void exit_app(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private async void install_steamCMD(object sender, RoutedEventArgs e)
        {
            DialogResult dr = System.Windows.Forms.MessageBox.Show("SteamCMDのインストールには時間がかかります。\n実行してもよろしいですか？", "ARK Server Utility", MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk);
            if (dr == System.Windows.Forms.DialogResult.Cancel)
            {
                return;
            }
            if (Directory.Exists(@"SteamCMD") == false)
            {
                Directory.CreateDirectory(@"SteamCMD");
                Console.WriteLine("ディレクトリ「SteamCMD」がありませんでした。");
            }
            else if (Directory.Exists(@"SteamCMD") == true)
            {
                Console.WriteLine("ディレクトリ「SteamCMD」がありました。");
                System.Windows.Forms.MessageBox.Show("既にSteamCMDがインストールされています。", "ARK Server Utility", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return;
            }
            WebClient mywebClient = new WebClient();
            mywebClient.DownloadFile("https://steamcdn-a.akamaihd.net/client/installer/steamcmd.zip", @"SteamCMD\\steamcmd.zip");
            try
            {
                ZipFile.ExtractToDirectory(@"SteamCMD\\steamcmd.zip", @"SteamCMD");
            }
            catch (IOException)
            {
                ;
            }
            await Task.Delay(1000);
            File.Delete(@"SteamCMD\\steamcmd.zip");
            ProcessStartInfo processStartInfo = new ProcessStartInfo(@"SteamCMD\\steamcmd.exe", "+quit");
            Process steamcmd_installer = Process.Start(processStartInfo);
            steamcmd_installer.WaitForExit();
            int exitCode = steamcmd_installer.ExitCode;
            steamcmd_installer.Close();
            Console.WriteLine("SteamCMDのインストールステータス：" + exitCode);
            if (exitCode == 7)
            {
                System.Windows.Forms.MessageBox.Show("SteamCMDのインストールが完了しました。", "ARK Server Utility", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("SteamCMDのインストール作業が中断されたか、正常にインストールできませんでした。", "ARK Server Utility", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
        private void launch_steamCMD(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(@"SteamCMD") == false)
            {
                System.Windows.Forms.MessageBox.Show("SteamCMDがインストールされていません。\n「SteamCMD」より「SteamCMDのインストール」を選択してください。", "ARK Server Utility", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return;
            }
            if (File.Exists(@"SteamCMD\\steamcmd.exe") == false)
            {
                System.Windows.Forms.MessageBox.Show("SteamCMDがインストールされていません。\n「SteamCMD」より「SteamCMDのインストール」を選択してください。", "ARK Server Utility", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return;
            }
            ProcessStartInfo processStartInfo = new ProcessStartInfo(@"SteamCMD\\steamcmd.exe");
            Process.Start(processStartInfo);
        }
        private void uninstall_steamCMD(object sender, RoutedEventArgs e)
        {
            DialogResult dr = System.Windows.Forms.MessageBox.Show("SteamCMDをアンインストールしてもよろしいですか？", "ARK Server Utility", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation);
            if (dr == System.Windows.Forms.DialogResult.Cancel)
            {
                return;
            }
            if (Directory.Exists(@"SteamCMD") == false)
            {
                System.Windows.Forms.MessageBox.Show("SteamCMDはインストールされていません。", "ARK Server Utility", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return;
            }
            System.IO.DirectoryInfo steamcmd_del = new System.IO.DirectoryInfo(@"SteamCMD");
            try
            {
                steamcmd_del.Delete(true);
            }
            catch (IOException err)
            {
                System.Windows.Forms.MessageBox.Show("SteamCMDのアンインストール中にエラーが発生しました。再度やり直してください。\n" + err, "ARK Server Utility", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return;
            }

            System.Windows.Forms.MessageBox.Show("SteamCMDのアンインストールが完了しました。", "ARK Server Utility", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if(join_pass.IsEnabled == true)
            {
                join_pass.IsEnabled = false;
            }
            else
            {
                join_pass.IsEnabled = true;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            label_name.Content = "サーバー名：" + server_name.Text;
            label_map.Content = "マップ名：" + map.Text;
            label_dir.Content = "ディレクトリ：" + server_dir.Text;
            string edit_set = "python/settings.py";
            var edit_pro = new Process
            {
                StartInfo = new ProcessStartInfo("python")
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    Arguments = edit_set + " edit 1 " + server_name.Text + " " + map.Text + " " + server_dir.Text,
                    CreateNoWindow = true
                }
            };
            edit_pro.Start();
            edit_pro.WaitForExit();
            edit_pro.Close();
            System.Windows.Forms.MessageBox.Show("保存しました。", "ARK Server Utility", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        private void install_server_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(@server_dir.Text + @"/ShooterGameServer.exe"))
            {
                DialogResult dr = System.Windows.Forms.MessageBox.Show("ARKのサーバーデータをアンインストールしてもよろしいですか？\n再度インストールするまでサーバーは起動できません。\nアンインストールには時間がかかります。", "ARK Server Utility", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation);
                if (dr == System.Windows.Forms.DialogResult.Cancel)
                {
                    return;
                }
                /// サーバーが起動している場合は終了する的な処理を。
                string read_set = "python/settings.py";
                var read_pro = new Process
                {
                    StartInfo = new ProcessStartInfo("python")
                    {
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        Arguments = read_set + " read 1",
                        CreateNoWindow = true
                    }
                };
                read_pro.Start();
                StreamReader read_str = read_pro.StandardOutput;
                string data = read_str.ReadLine();
                read_pro.WaitForExit();
                read_pro.Close();
                string[] arr = data.Split(',');
                main_pbar.Value = 0;
                main_ptext.Content = "データアンインストール処理中...";
                try
                {
                    Directory.Delete(@arr[2]);
                }
                catch
                {
                    System.Windows.Forms.MessageBox.Show("データの削除中にエラーが発生しました。", "ARK Server Utility", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    main_pbar.Value = 100;
                    main_ptext.Content = "ARK: Server Utility";
                    return;
                }
                main_pbar.Value = 100;
                main_ptext.Content = "データの削除が完了しました。";
                System.Windows.Forms.MessageBox.Show("データの削除が完了しました。", "ARK Server Utility", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                start_server.IsEnabled = false;
                install_server.Content = "インストール";
                main_ptext.Content = "ARK: Server Utility";
                return;
            }
            else
            {
                DialogResult dr = System.Windows.Forms.MessageBox.Show("ARKのサーバーデータをインストールしてもよろしいですか？\nインストールには時間がかかります。", "ARK Server Utility", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation);
                if (dr == System.Windows.Forms.DialogResult.Cancel)
                {
                    return;
                }
                main_pbar.Value = 0;
                main_ptext.Content = "SteamCMDチェック中...";
                if (Directory.Exists(@"SteamCMD") == false)
                {
                    System.Windows.Forms.MessageBox.Show("SteamCMDがインストールされていません。\n「SteamCMD」より「SteamCMDのインストール」を選択してください。", "ARK Server Utility", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    return;
                }
                if (File.Exists(@"SteamCMD\\steamcmd.exe") == false)
                {
                    System.Windows.Forms.MessageBox.Show("SteamCMDがインストールされていません。\n「SteamCMD」より「SteamCMDのインストール」を選択してください。", "ARK Server Utility", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    return;
                }
                
                main_pbar.Value = 25;
                main_ptext.Content = "SteamCMDチェック完了...";
                /// 設定読み込み
                
                main_pbar.Value = 50;
                main_ptext.Content = "インストール処理中...";
                string read_set = "python/settings.py";
                var read_pro = new Process
                {
                    StartInfo = new ProcessStartInfo("python")
                    {
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        Arguments = read_set + " read 1",
                        CreateNoWindow = true
                    }
                };
                read_pro.Start();
                StreamReader read_str = read_pro.StandardOutput;
                string data = read_str.ReadLine();
                read_pro.WaitForExit();
                read_pro.Close();
                string[] arr = data.Split(',');

                main_pbar.Value = 75;
                main_ptext.Content = "インストール処理中...";
                /// SteamCMDよりARKをダウンロード
                ProcessStartInfo processStartInfo = new ProcessStartInfo(@"SteamCMD\\steamcmd.exe", "+login anonymous +force_install_dir " + @arr[2] + " +app_update 376030");
                Process steamcmd_installer = Process.Start(processStartInfo);

                int exitCode = steamcmd_installer.ExitCode;
                Console.WriteLine(exitCode);
                if (exitCode == 1)
                {
                    main_pbar.Value = 100;
                    main_ptext.Content = "インストールに成功しました。";
                    System.Windows.Forms.MessageBox.Show("インストールに成功しました。", "ARK Server Utility", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    start_server.IsEnabled = true;
                    install_server.Content = "アンインストール";
                    main_ptext.Content = "ARK: Server Utility";
                    return;
                }
                else
                {
                    main_pbar.Value = 100;
                    main_ptext.Content = "インストールに失敗しました。";
                    System.Windows.Forms.MessageBox.Show("正常にインストールが終了しませんでした。\n再度やり直してください。", "ARK Server Utility", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    main_ptext.Content = "ARK: Server Utility";
                    return;
                }
            }
        }
        private void install_dir_bt_Click(object sender, RoutedEventArgs e)
        {
            using (var cofd = new CommonOpenFileDialog()
            {
                Title = "データを保存する場所を選択してください。",
                InitialDirectory = @"C:",
                // フォルダ選択モードにする
                RestoreDirectory = true,
                IsFolderPicker = true,
            })
            {
                if (cofd.ShowDialog() != CommonFileDialogResult.Ok)
                {
                    return;
                }
                server_dir.Text = cofd.FileName;
            }
        }

        private void add_list_bt(object sender, RoutedEventArgs e)
        {
            var value_pro = new Process
            {
                StartInfo = new ProcessStartInfo("python")
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    Arguments = "python/settings.py value",
                    CreateNoWindow = true
                }
            };
            value_pro.Start();
            StreamReader val_read = value_pro.StandardOutput;
            string value = val_read.ReadLine();
            value_pro.WaitForExit();
            value_pro.Close();
            int new_value = int.Parse(value);
            new_value++;
            server_list.Items.Add("server" + new_value);
            server_list.Text = "server" + new_value;
            server_name.Text = "server" + new_value;
            label_name.Content = "サーバー名：server" + new_value;
            map.Text = "TheIsland";
            label_map.Content = "マップ名：TheIsland";
            server_dir.Text = "C:\\";
            label_dir.Content = "ディレクトリ：C:\\";
            game_port.IsEnabled = false;
            query_port.IsEnabled = false;
            server_pass_bool.IsEnabled = false;
            admin_pass.IsEnabled = false;
            var list_pro = new Process
            {
                StartInfo = new ProcessStartInfo("python")
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    Arguments = "python/settings.py write server" + new_value + " TheIsland C:\\",
                    CreateNoWindow = true
                }
            };
            list_pro.Start();
            list_pro.WaitForExit();
            list_pro.Close();
        }

        private void list_changed(object sender, DependencyPropertyChangedEventArgs e)
        {
            string server = server_list.Text;
            int index = server_list.Items.IndexOf(server);
            var list_pro = new Process
            {
                StartInfo = new ProcessStartInfo("python")
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    Arguments = "python/settings.py read " + index,
                    CreateNoWindow = true
                }
            };
            list_pro.Start();
            StreamReader list_read = list_pro.StandardOutput;
            string list_string = list_read.ReadLine();
            Console.WriteLine(list_string);
            list_pro.WaitForExit();
            list_pro.Close();
            string[] arr = list_string.Split(',');
            server_name.Text = arr[0];
            label_name.Content = "サーバー名：" + arr[0];
            map.Text = arr[1];
            label_map.Content = "マップ名：" + arr[1];
            server_dir.Text = arr[2];
            label_dir.Content = "ディレクトリ：" + arr[2];
        }

        private void arg_setting_change(object sender, RoutedEventArgs e)
        {
            ;
        }
        private void arg_list_changed(object sender, RoutedEventArgs e)
        {
            if (arg_setting.SelectedItem == null) return;
            arg_data item = (arg_data)arg_setting.SelectedItem;
            arg_arg.Text = item.arg;
            arg_detail.Text = item.detail;
        }
    }

}
