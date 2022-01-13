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


namespace ark_server_utility
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<arg_data> args = new List<arg_data>();


        /// ARK: Server Utilityのバージョン
        /// v[メジャー].[マイナー].[適当]
        public string version = "v0.9.1";


        // グローバルでポートを入れる変数
        public int port;

        // IPC通信で、出力を返す
        public string IpcConnect(string text)
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
        public void IpcSend(string text)
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

        public MainWindow()
        {

            // 辞書形式を宣言
            var dict = new Dictionary<string, List<string>>();

            // IPC通信を開始するためのポート検索するスクリプトを実行する
            var ipc_port = new Process
            {
                StartInfo = new ProcessStartInfo("python/search_port.exe")
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            ipc_port.Start();
            StreamReader port_num = ipc_port.StandardOutput;
            port = int.Parse(port_num.ReadLine());
            Console.WriteLine("Port for IPC Connection:" + port);
            ipc_port.WaitForExit();
            ipc_port.Close();

            // IPC通信を開始する
            var ipc_main = new Process
            {
                StartInfo = new ProcessStartInfo("python/ipc_main.exe")
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    Arguments = port.ToString(),
                    CreateNoWindow = true
                }
            };

            Console.WriteLine("IPCプログラムを実行します...");
            ipc_main.Start();
            int count = 0;
            while (true)
            {
                count++;
                if (count == 11)
                {
                    System.Windows.Forms.MessageBox.Show("IPC通信が確立されませんでした。\nファイアーウォールの設定などを確認してください。", "ARK Server Utility", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    ipc_main.Close();
                    this.Close();
                }
                try
                {
                    IpcSend("");
                    Console.WriteLine("IPC通信が確立されました。");
                    break;
                }
                catch (Exception ex)
                {
                    // 3,4回までなら許容範囲
                    Console.WriteLine("IPC通信が確立されませんでした。\n\n・エラー\n" + ex.ToString() + "\n試行回数:" + count.ToString() + "/10");
                    Task.Delay(50);
                }
            }


            // XAMLとかなんとか...
            InitializeComponent();

            // 起動オプションの引数をぉおお！！！ここにぃいいい！！！つっこむうぅうう！！！ぜんぶうぅうううう！！（地獄）

            // dict.Add("allowansel", new List<string>(){"さけ","サバ","はまち"});
            arg_setting.Items.Add(new arg_data { arg = "aaaa", detail = "bbb"});
            Console.WriteLine(File.Exists(@"settings.json"));
            if (!File.Exists(@"settings.json"))
            {
                IpcSend("settings first");
                while (true)
                {
                    Thread.Sleep(50);
                    if (File.Exists(@"settings.json"))
                    {
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                Thread.Sleep(1000);
            }
            string f_r = IpcConnect("settings read 1");
            Console.WriteLine(f_r);
            string[] arr = f_r.Split(',');
            label_name.Content = "サーバー名：" + arr[0];
            label_dir.Content = "ディレクトリ：" + arr[2];


            string value = IpcConnect("settings value");
            if (value == "1")
            {
                del_list.IsEnabled = false;
                server_list.Items.Add(arr[0]);
            }
            else
            {
                del_list.IsEnabled = true;
                string[] names = IpcConnect("settings name").Split(',');
                for (int i = 0; i < int.Parse(value); i++)
                {
                    server_list.Items.Add(names[i]);
                }
            }
            if (!File.Exists(@arr[2] + @"\\ShooterGame\\Binaries\\Win64\\ShooterGameServer.exe"))
            {
                // サーバーデータがインストールされていない場合の処理
                start_server.IsEnabled = false;
                install_server.Content = "インストール";
                string version = IpcConnect("webapi version 0");
                latest_version.Content = "配信されている最新バージョン：" + version;
                current_version.Content = "インストールされていません。";
                update_bt.Content = "アップデート";
            }
            else
            {
                // サーバーデータがインストールされている場合の処理
                start_server.IsEnabled = true;
                install_server.Content = "アンインストール";
                string version = IpcConnect("webapi version 1");
                Console.WriteLine(version);
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
            // 「ServerName,MapName,Directory」
            // 「nattyantv,TheIsland,C:\\」
            server_name.Text = arr[0];
            Console.WriteLine(arr[1]);
            if (arr[1].Contains("Custom/") == false)
            {
                map.SelectedValue = arr[1];
                custom_map_name.IsEnabled = false;
                custom_map_name.Text = "";
                map_id.IsEnabled = false;
                map_id.Text = "";
                label_map.Content = "マップ名：" + arr[1];
            }
            else
            {
                string[] vs = arr[1].Split('/');
                map.SelectedValue = vs[0];
                Console.WriteLine(map.SelectedValue.ToString());
                custom_map_name.IsEnabled = true;
                custom_map_name.Text = vs[1];
                map_id.IsEnabled = true;
                map_id.Text = vs[2];
                label_map.Content = "マップ名：" + vs[1];
            }
            
            server_dir.Text = arr[2];
            server_list.Text = arr[0];
            main_pbar.Value = 100;
            main_ptext.Content = "ARK: Server Utility " + version;
            Console.WriteLine("\n####################\n\nARK: Server Utility \nVersion:" + version + "\nCreated by: nattyan-tv\n\n####################\n");
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
            label_dir.Content = "ディレクトリ：" + server_dir.Text;
            
            string rs;
            Console.WriteLine(map.SelectedValue);
            if (map.SelectedValue.ToString() == "Custom")
            {
                label_map.Content = "マップ名：" + custom_map_name.Text;
                rs = IpcConnect("settings edit 1 " + server_name.Text + " Custom/" + custom_map_name.Text + "/" + map_id.Text + " " + server_dir.Text);
            }
            else
            {
                label_map.Content = "マップ名：" + map.Text;
                rs = IpcConnect("settings edit 1 " + server_name.Text + " " + map.SelectedValue + " " + server_dir.Text);
            }
            Console.WriteLine(rs);
            if (rs != "OK")
            {
                System.Windows.Forms.MessageBox.Show("設定保存時にエラーが発生しました。", "ARK Server Utility", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("保存しました。", "ARK Server Utility", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
            
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
                string[] arr = IpcConnect("settings read 1").Split(',');
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
                string[] arr = IpcConnect("settings read 1").Split(',');
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
                Title = "サーバーを保存する場所を選択してください。",
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
            int new_value = int.Parse(IpcConnect("settings value"));
            new_value++;

            Console.WriteLine(new_value.ToString());
            Console.WriteLine(IpcConnect("settings write server" + new_value + " TheIsland C:\\").ToString());

            server_list.Items.Add("server" + new_value);
            server_list.Text = "server" + new_value;
            server_name.Text = "server" + new_value;
            label_name.Content = "サーバー名：server" + new_value;
            label_map.Content = "マップ名：The Island";
            server_dir.Text = "C:\\";
            label_dir.Content = "ディレクトリ：C:\\";
            game_port.IsEnabled = false;
            query_port.IsEnabled = false;
            server_pass_bool.IsEnabled = false;
            admin_pass.IsEnabled = false;
            del_list.IsEnabled = true;
            map.SelectedValue = "TheIsland";
        }

        private void list_changed(object sender, DependencyPropertyChangedEventArgs e)
        {
            string server = server_list.Text;
            int index = server_list.Items.IndexOf(server);
            string[] arr = IpcConnect("settings read " + index).Split(',');
            Console.WriteLine(arr.ToString());
            server_name.Text = arr[0];
            label_name.Content = "サーバー名：" + arr[0];
            map.Text = arr[1];
            label_map.Content = "マップ名：" + arr[1];
            server_dir.Text = arr[2];
            label_dir.Content = "ディレクトリ：" + arr[2];
            Console.WriteLine(server_list.SelectedValue)
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

        private void connect_pro(object sender, EventArgs e)
        {
            string rt = IpcConnect(server_name.Text);
            System.Windows.Forms.MessageBox.Show("・返り値\n" + rt, "ARK Server Utility", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        private void send_pro(object sender, EventArgs e)
        {
            IpcSend(server_name.Text);
        }

        private void get_pid(object sender, EventArgs e)
        {
            string pid = IpcConnect("debug pid");
            System.Windows.Forms.MessageBox.Show("プロセスID:" + pid, "ARK Server Utility", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        private void get_addr(object sender, EventArgs e)
        {
            string addr = IpcConnect("debug addr");
            System.Windows.Forms.MessageBox.Show("アドレス:" + addr, "ARK Server Utility", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            IpcSend("exit");
        }

        private void map_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (map.SelectedValue == null)
            {
                return;
            }
            Console.WriteLine(map.SelectedValue);
            Console.WriteLine(map.SelectedValue == "Custom");
            if (map.SelectedValue.ToString() == "Custom")
            {
                custom_map_name.IsEnabled = true;
                map_id.IsEnabled = true;
            }
            else
            {
                custom_map_name.IsEnabled = false;
                map_id.IsEnabled = false;
            }
        }

        private void server_list_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if(server_list.SelectedItem == null)
            {
                return;
            }
            Console.WriteLine("list:"+(server_list.SelectedIndex + 1));
            string setting = IpcConnect("settings read " + (server_list.SelectedIndex + 1));
            string[] load_settings = setting.Split(',');

            server_name.Text = load_settings[0];
            label_name.Content = "サーバー名：" + load_settings[0];

            if (load_settings[0].Contains("Custom/") == false)
            {
                map.SelectedValue = load_settings[1];
                label_map.Content = "マップ名：" + load_settings[1];
                custom_map_name.Text = "";
                map_id.Text = "";
                custom_map_name.IsEnabled = false;
                map_id.IsEnabled = false;
            }
            else
            {
                // Custom/MapName/MapID
                string[] vs = load_settings[1].Split('/');
                map.SelectedValue = vs[0];
                label_map.Content = "マップ名：" + vs[1];
                custom_map_name.Text = vs[1];
                map_id.Text = vs[2];
                custom_map_name.IsEnabled = true;
                map_id.IsEnabled = true;
            }

            server_dir.Text = load_settings[2];
            label_dir.Content = "ディレクトリ：" + load_settings[2];
            game_port.IsEnabled = false;
            query_port.IsEnabled = false;
            server_pass_bool.IsEnabled = false;
            admin_pass.IsEnabled = false;
        }

        private void del_list_Click(object sender, RoutedEventArgs e)
        {
            int index = server_list.SelectedIndex;
            Console.WriteLine(index);
            if (IpcConnect("settings value") == "2")
            {
                del_list.IsEnabled = false;
            }
            IpcSend("settings del " + (index+1));
            server_list.Items.RemoveAt(index);
            if (index != 0)
            {
                server_list.SelectedIndex = index - 1;
            }
            else
            {
                server_list.SelectedIndex = 0;
            }
        }
    }

}
