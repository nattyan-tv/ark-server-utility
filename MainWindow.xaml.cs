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
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>



    public partial class MainWindow : Window
    {
        private List<arg_data> args = new List<arg_data>();

        /// ARK: Server Utilityのバージョン
        /// v[メジャー].[マイナー].[ビルド]
        public string version = "0.9.1";

        // グローバルでポートを入れる変数
        public int port;

        // 設定が変更された的な変数
        public int set_changed = 0;

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

        // RCON接続(Pythonに移すかも)
        static async Task rcon_command(ushort port, string command, string password)
        {
            var connection = new RCON(host:IPAddress.Parse("127.0.0.1"), port:port,password:password);
            var result = await connection.SendCommandAsync(command);
            Console.WriteLine(result);
        }


        /// <summary>
        /// 起動時に実行されるメインスクリプト
        /// </summary>
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
            Console.WriteLine("IPC通信用のポート:" + port);
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
                    Environment.Exit(1);
                    return;
                }
                try
                {
                    IpcSend("",port);
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


            /// XAMLとかなんとか...
            /// このコードより上に書くGUIの設定項目は基本的に無視される（らしい）
            InitializeComponent();
            server_list.MouseWheel += server_list_wheeled;

            // 起動オプションの引数をぉおお！！！ここにぃいいい！！！つっこむうぅうう！！！ぜんぶうぅうううう！！（地獄）

            // dict.Add("allowansel", new List<string>(){"さけ","サバ","はまち"});
            arg_setting.Items.Add(new arg_data { arg = "MaxPlayers", detail = "サーバーに入れる最大の人数\nint型" });
            arg_setting.Items.Add(new arg_data { arg = "ServerCrosshair", detail = "ゲーム画面で画面中央に十字のクロスヘアを表示するか\nBool型" });
            arg_setting.Items.Add(new arg_data { arg = "MapPlayerLocation", detail = "マップに自分の位置を表示するか\nBool型" });
            arg_setting.Items.Add(new arg_data { arg = "AllowThirdPersonPlayer", detail = "ユーザーが3人称視点を使えるようにするか\nBool型" });
            arg_setting.Items.Add(new arg_data { arg = "TheMaxStructuresInRange", detail = "1チャンクあたりの設置物上限数\nint型" });


            Console.WriteLine(File.Exists(@"settings.json"));
            if (!File.Exists(@"settings.json"))
            {
                IpcSend("settings first", port);
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
            string f_r = IpcConnect("settings read 1", port);
            Console.WriteLine(f_r);
            string[] arr = f_r.Split(',');
            label_name.Text = "サーバー名：" + arr[0];
            label_dir.Text = "ディレクトリ：" + arr[2];


            string value = IpcConnect("settings value", port);
            if (value == "1")
            {
                del_list.IsEnabled = false;
                server_list.Items.Add(arr[0]);
            }
            else
            {
                del_list.IsEnabled = true;
                string[] names = IpcConnect("settings name", port).Split(',');
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
                string version = IpcConnect("webapi version 0", port);
                latest_version.Content = "配信されている最新バージョン：" + version;
                current_version.Content = "インストールされていません。";
                update_bt.Content = "アップデート";
            }
            else
            {
                // サーバーデータがインストールされている場合の処理
                start_server.IsEnabled = true;
                install_server.Content = "アンインストール";
                string version = IpcConnect("webapi version 1 1", port);
                Console.WriteLine("バージョン:[" + version + "]");
                string[] vers = version.Split(',');
                latest_version.Content = "配信されている最新バージョン：" + vers[0];
                current_version.Content = "インストールされているバージョン：" + vers[1].Replace("\r", "").Replace("\n", "");
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
            /// 「ServerName,MapName,Directory」
            /// 「nattyantv,TheIsland,C:\\」
            server_name.Text = arr[0];
            Console.WriteLine(arr[1]);
            if (arr[1].Contains("Custom/") == false)
            {
                map.SelectedValue = arr[1];
                custom_map_name.IsEnabled = false;
                custom_map_name.Text = "";
                map_id.IsEnabled = false;
                map_id.Text = "";
                label_map.Text = "マップ名：" + arr[1];
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
                label_map.Text = "マップ名：" + vs[1];
            }


            server_dir.Text = arr[2];
            server_list.Text = arr[0];
            main_pbar.Value = 100;
            main_ptext.Content = "ARK: Server Utility " + version;
            Console.WriteLine("\n####################\n\nARK: Server Utility \nVersion:" + version + "\nCreated by: nattyan-tv\n\n####################\n");
            Thread status_loader = new Thread(new ThreadStart(() =>
            {
                status_loop();
            }));
 
            status_loader.Start();
        }

        static void status_loop()
        {
            int loop_i = 0;
            for(;;)
            {
                Console.WriteLine(i.ToString());
                Thread.Sleep(1000);
                i++;
            }
        }

        /// <summary>
        /// デバッグ用メニューのコード達
        /// </summary>

        private void start_debug(object sender, RoutedEventArgs e)
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo(@server_dir.Text + @"\\ShooterGame\\Binaries\\Win64\\ShooterGameServer.exe", map.Text + "?listen?SessionName=" + server_name.Text + "?ServerPassword=" + join_pass.Password + "?ServerAdminPassword=" + admin_pass.Password + "?Port=7777?QueryPort=27015?MaxPlayers=3");
            Process.Start(processStartInfo);
        }

        private void connect_pro(object sender, EventArgs e)
        {
            string rt = IpcConnect(server_name.Text, port);
            System.Windows.Forms.MessageBox.Show("・返り値\n" + rt, "ARK Server Utility", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        private void send_pro(object sender, EventArgs e)
        {
            IpcSend(server_name.Text, port);
        }

        private void get_pid(object sender, EventArgs e)
        {
            string pid = IpcConnect("debug pid", port);
            System.Windows.Forms.MessageBox.Show("プロセスID:" + pid, "ARK Server Utility", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        private void get_addr(object sender, EventArgs e)
        {
            string addr = IpcConnect("debug addr", port);
            System.Windows.Forms.MessageBox.Show("アドレス:" + addr, "ARK Server Utility", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        /// <summary>
        /// デバッグメニュー終わり
        /// </summary>
        /// 
        
        private void server_list_wheeled(object sender, EventArgs e)
        {
            Console.WriteLine("Wheeled");
            HandledMouseEventArgs wEventArgs = e as HandledMouseEventArgs;
            wEventArgs.Handled = true;
        }

        private void check_update(object sender, RoutedEventArgs e)
        {
            var uc = new update_checker(port,version);
            uc.Show();
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
            try
            {
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
            catch (Exception err)
            {
                Console.WriteLine(err);
                System.Windows.Forms.MessageBox.Show("SteamCMDのインストール作業中にエラーが発生しました。", "ARK Server Utility", MessageBoxButtons.OK, MessageBoxIcon.Hand);
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
            // set_changed = 1;
        }

        // 設定保存
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (map.SelectedValue.ToString() == "Custom")
            {
                if (custom_map_name.Text == "" || map_id.Text == "")
                {
                    System.Windows.Forms.MessageBox.Show("マップをカスタムマップにする場合は、「マップ名」と「マップID」が必要です。", "ARK Server Utility", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
            }
            Console.WriteLine("スペース:" + server_name.Text.Contains(" "));
            if(server_name.Text.Contains(" ") == true)
            {
                server_name.Text = server_name.Text.Replace(" ", "_");
                Console.WriteLine(server_name.Text);
            }
            Console.WriteLine(server_name.Text);
            label_name.Text = "サーバー名：" + server_name.Text;
            label_dir.Text = "ディレクトリ：" + server_dir.Text;
            server_list.Items.Insert(server_list.SelectedIndex+1, server_name.Text);
            server_list.Items.RemoveAt(server_list.SelectedIndex);

            Console.WriteLine(server_name.Text);
            string rs;
            Console.WriteLine(map.SelectedValue);
            if (map.SelectedValue.ToString() == "Custom")
            {
                label_map.Text = "マップ名：" + custom_map_name.Text;
                rs = IpcConnect("settings edit 1 " + server_name.Text + " Custom/" + custom_map_name.Text + "/" + map_id.Text + " " + server_dir.Text, port);
            }
            else
            {

                Console.WriteLine(server_name.Text);
                label_map.Text = "マップ名：" + map.Text;
                rs = IpcConnect("settings edit 1 " + server_name.Text + " " + map.SelectedValue + " " + server_dir.Text, port);
            }
            Console.WriteLine(rs);

            server_list.Text = server_name.Text;
            Console.WriteLine(server_name.Text);
            if (rs != "OK")
            {
                System.Windows.Forms.MessageBox.Show("設定保存時にエラーが発生しました。", "ARK Server Utility", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("保存しました。", "ARK Server Utility", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                // set_changed = 0;
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
                string[] arr = IpcConnect("settings read 1", port).Split(',');
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
                string[] arr = IpcConnect("settings read 1", port).Split(',');
                main_pbar.Value = 75;
                main_ptext.Content = "インストール処理中...";
                /// SteamCMDよりARKをダウンロード
                ProcessStartInfo processStartInfo = new ProcessStartInfo(@"SteamCMD\\steamcmd.exe", "+login anonymous +force_install_dir " + @arr[2] + " +app_update 376030");
                Process steamcmd_installer = Process.Start(processStartInfo);
                steamcmd_installer.WaitForExit();
                int eCode = steamcmd_installer.ExitCode;
                Console.WriteLine(eCode);
                if (eCode == 1 || eCode == 0)
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
            int new_value = int.Parse(IpcConnect("settings value", port));
            new_value++;

            Console.WriteLine(new_value.ToString());
            Console.WriteLine(IpcConnect("settings write server" + new_value + " TheIsland C:\\", port).ToString());

            server_list.Items.Add("server" + new_value);
            server_list.Text = "server" + new_value;
            server_name.Text = "server" + new_value;
            label_name.Text = "サーバー名：server" + new_value;
            label_map.Text = "マップ名：The Island";
            server_dir.Text = "C:\\";
            label_dir.Text = "ディレクトリ：C:\\";
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
            string[] arr = IpcConnect("settings read " + index, port).Split(',');
            Console.WriteLine(arr.ToString());
            // WebAPIを取得する
            server_name.Text = arr[0];
            label_name.Text = "サーバー名：" + arr[0];
            map.Text = arr[1];
            label_map.Text = "マップ名：" + arr[1];
            server_dir.Text = arr[2];
            label_dir.Text = "ディレクトリ：" + arr[2];
        }

        private void arg_setting_change(object sender, RoutedEventArgs e)
        {
            string rt = IpcConnect("exec_arg edit " + (server_list.SelectedIndex+1, port) + " 2 " + arg_arg.Text + " " + arg_value.Text, port);
            if (rt != "OK")
            {
                System.Windows.Forms.MessageBox.Show("設定保存時にエラーが発生しました。", "ARK Server Utility", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("保存しました。", "ARK Server Utility", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
        }

        private void arg_list_changed(object sender, RoutedEventArgs e)
        {
            if (arg_setting.SelectedItem == null) return;
            arg_data item = (arg_data)arg_setting.SelectedItem;
            arg_arg.Text = item.arg;
            arg_detail.Text = item.detail;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            IpcSend("exit", port);
        }

        private void map_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (map.SelectedValue == null)
            {
                return;
            }
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
            // set_changed = 1;
        }

        // サーバーリスト変更
        private void server_list_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if(server_list.SelectedItem == null)
            {
                return;
            }
            if (set_changed == 1)
            {
                DialogResult save = System.Windows.Forms.MessageBox.Show("設定は変更されています。\n保存しますか？", "ARK Server Utility", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (save == System.Windows.Forms.DialogResult.Yes)
                {
                    if (map.SelectedValue.ToString() == "Custom")
                    {
                        if (custom_map_name.Text == "" || map_id.Text == "")
                        {
                            System.Windows.Forms.MessageBox.Show("マップをカスタムマップにする場合は、「マップ名」と「マップID」が必要です。", "ARK Server Utility", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            return;
                        }
                    }
                    Console.WriteLine("スペース:" + server_name.Text.Contains(" "));
                    if (server_name.Text.Contains(" ") == true)
                    {
                        server_name.Text = server_name.Text.Replace(" ", "_");
                        Console.WriteLine(server_name.Text);
                    }
                    Console.WriteLine(server_name.Text);
                    label_name.Text = "サーバー名：" + server_name.Text;
                    label_dir.Text = "ディレクトリ：" + server_dir.Text;
                    server_list.Items.Insert(server_list.SelectedIndex + 1, server_name.Text);
                    server_list.Items.RemoveAt(server_list.SelectedIndex);

                    Console.WriteLine(server_name.Text);
                    string rs;
                    Console.WriteLine(map.SelectedValue);
                    if (map.SelectedValue.ToString() == "Custom")
                    {
                        label_map.Text = "マップ名：" + custom_map_name.Text;
                        rs = IpcConnect("settings edit 1 " + server_name.Text + " Custom/" + custom_map_name.Text + "/" + map_id.Text + " " + server_dir.Text, port);
                    }
                    else
                    {

                        Console.WriteLine(server_name.Text);
                        label_map.Text = "マップ名：" + map.Text;
                        rs = IpcConnect("settings edit 1 " + server_name.Text + " " + map.SelectedValue + " " + server_dir.Text, port);
                    }
                    Console.WriteLine(rs);

                    server_list.Text = server_name.Text;
                    Console.WriteLine(server_name.Text);
                    if (rs != "OK")
                    {
                        System.Windows.Forms.MessageBox.Show("設定保存時にエラーが発生しました。", "ARK Server Utility", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        // set_changed = 0;
                        System.Windows.Forms.MessageBox.Show("保存しました。", "ARK Server Utility", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                    return;
                }
            }
            Console.WriteLine("list:"+(server_list.SelectedIndex + 1));
            string setting = IpcConnect("settings read " + (server_list.SelectedIndex + 1), port);
            string[] load_settings = setting.Split(',');
            Console.WriteLine(setting);

            if (!File.Exists(@load_settings[2] + @"\\ShooterGame\\Binaries\\Win64\\ShooterGameServer.exe"))
            {
                // サーバーデータがインストールされていない場合の処理
                start_server.IsEnabled = false;
                install_server.Content = "インストール";
                string version = IpcConnect("webapi version 3", port);
                latest_version.Content = "配信されている最新バージョン：" + version;
                current_version.Content = "インストールされていません。";
                update_bt.Content = "アップデート";
            }
            else
            {
                // サーバーデータがインストールされている場合の処理
                start_server.IsEnabled = true;
                install_server.Content = "アンインストール";
                string version = IpcConnect("webapi version 2 " + (server_list.SelectedIndex + 1), port);
                Console.WriteLine("バージョン:[" + version + "]");
                string[] vers = version.Split(',');
                latest_version.Content = "配信されている最新バージョン：" + vers[0];
                current_version.Content = "インストールされているバージョン：" + vers[1].Replace("\r", "").Replace("\n", "");
                update_bt.IsEnabled = true;

                if (float.Parse(vers[0]) > float.Parse(vers[1]))
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

            server_name.Text = load_settings[0];
            label_name.Text = "サーバー名：" + load_settings[0];

            if (load_settings[0].Contains("Custom/") == false)
            {
                map.SelectedValue = load_settings[1];
                label_map.Text = "マップ名：" + load_settings[1];
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
                label_map.Text = "マップ名：" + vs[1];
                custom_map_name.Text = vs[1];
                map_id.Text = vs[2];
                custom_map_name.IsEnabled = true;
                map_id.IsEnabled = true;
            }

            server_dir.Text = load_settings[2];
            label_dir.Text = "ディレクトリ：" + load_settings[2];
            game_port.IsEnabled = false;
            query_port.IsEnabled = false;
            server_pass_bool.IsEnabled = false;
            admin_pass.IsEnabled = false;

        }

        private void del_list_Click(object sender, RoutedEventArgs e)
        {
            int index = server_list.SelectedIndex;
            Console.WriteLine(index);
            if (IpcConnect("settings value", port) == "2")
            {
                del_list.IsEnabled = false;
            }
            IpcSend("settings del " + (index+1), port);
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

        private void server_start(object sender, RoutedEventArgs e)
        {
            bool server_starting = false;
            /// 上は仮置き
            /// サーバーの実行ステータスをboolで取得する
            /// 基本的にはWebAPIとかで取っていいと思う

            /// https://github.com/cyilcode/SteamQueryNet
            /// IServerQuery serverQuery = new SteamQueryNet.ServerQuery("localhost," + query_port.Text);

            if (server_starting == true)
            {
                /// 現在のサーバーをシャットダウンする
            }
            else
            {
                /// 現在のサーバーを実行する
                ProcessStartInfo ark_game = new ProcessStartInfo(@server_dir.Text + @"\\ShooterGame\\Binaries\\Win64\\ShooterGameServer.exe", map.Text + "?listen?RCONEnabled=True?RCONPort=" + rcon_port.Text);
                Process.Start(ark_game);
            }
        }
    }

}
