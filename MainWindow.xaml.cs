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

        // IPC通信で、出力を変えさない
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

            arg_setting.Items.Add(new arg_data { arg = "AllowAnyoneBabyImprintCuddle", detail = "誰もが刻印されている人だけでなく、ベビーディノ（抱擁など）を「ケア」できるようにする場合に使用します。" });
            arg_setting.Items.Add(new arg_data { arg = "AllowCrateSpawnsOnTopOfStructures", detail = "サーバーは、構造物によって防止されるのではなく、空からの供給クレートを構造物の上に表示できるようになりました。" });
            arg_setting.Items.Add(new arg_data { arg = "AllowFlyerCarryPvE", detail = "PvEの飛行生物は野生の恐竜を運ぶことができます。" });
            arg_setting.Items.Add(new arg_data { arg = "AllowFlyingStaminaRecovery", detail = "プレーヤーが乗っている代わりに立っているときに飛行生物がスタミナを回復する必要がある場合は、trueに設定します。" });
            arg_setting.Items.Add(new arg_data { arg = "AllowMultipleAttachedC4", detail = "恐竜ごとに複数のC4を接続できるようにするにはtrueに設定します。" });
            arg_setting.Items.Add(new arg_data { arg = "AutoDestroyDecayedDinos", detail = "要求可能な恐竜をロード時に自動破壊し、要求可能なままにしておきます。\nパッチ255.0で導入" });
            arg_setting.Items.Add(new arg_data { arg = "AppendItemSets", detail = "に設定すると、すべてのアイテムをオーバーライドするのではなく、クレートを提供するアイテムセットを追加します。ConfigOverrideSupplyCrateItemsを参照してください\nパッチ273.7で導入" });
            arg_setting.Items.Add(new arg_data { arg = "AppendPreventIncreasingMinMaxItemSets", detail = "trueに設定すると、ドロップされるアイテムの量が動的に増加します。ConfigOverrideSupplyCrateItemsを参照してください\nパッチ273.7で導入" });
            arg_setting.Items.Add(new arg_data { arg = "ClampItemSpoilingTimes", detail = "すべての腐敗時間をアイテムの最大腐敗時間に固定します。腐敗時間を変更するModで問題を引き起こす可能性があります。\nパッチ254.944で導入" });
            arg_setting.Items.Add(new arg_data { arg = "ClampItemStats", detail = "アイテムの統計クランプを有効/無効にします。 詳細については、ItemStatClampsを参照してください。\nパッチ255.0で導入" });
            arg_setting.Items.Add(new arg_data { arg = "DestroyUnconnectedWaterPipes", detail = "2日間のリアルタイムの後、パイプは非パイプ（直接または間接）に接続されておらず、近くに味方プレイヤーがいない場合、パイプは自動的に破壊されます。" });
            arg_setting.Items.Add(new arg_data { arg = "DisableImprintDinoBuff", detail = "気に入らない場合は、これを使用してDino Imprinting-Player Stat Bonusを無効にします（Dinoに特に刻印され、Imprinting Qualityに上げると、追加のダメージ/耐性バフが得られます）" });
            arg_setting.Items.Add(new arg_data { arg = "EnableExtraStructurePreventionVolumes", detail = "これを使用して、特定のリソースが豊富なエリア、特に主要な山々の周りのTheIslandでのセットアップを完全に無効にします。" });
            arg_setting.Items.Add(new arg_data { arg = "ExtinctionEventTimeInterval", detail = "これを使用して、30日間の絶滅を有効にします。 数値は秒単位の時間です。" });
            arg_setting.Items.Add(new arg_data { arg = "FastDecayUnsnappedCoreStructures", detail = "スナップされていない基礎/支柱を5倍の速度で崩壊させます。\nパッチ245.987で導入" });
            arg_setting.Items.Add(new arg_data { arg = "ForceFlyerExplosives", detail = "飛行（QuetzalとWyvernは、C4が取り付けられた状態では飛べません。このパラメーターをtrueに設定すると、飛ぶことができます。\nパッチ252.83で導入" });
            arg_setting.Items.Add(new arg_data { arg = "MaxPersonalTamedDinos", detail = "部族ごとの恐竜のテイム制限を有効にします。 （公式サーバーでは500）\nパッチ255.0で導入" });
            arg_setting.Items.Add(new arg_data { arg = "MinimumDinoReuploadInterval", detail = "許可された恐竜再アップロード間のクールダウンの秒数（デフォルトは0、オフィシャルサーバーでは43200に設定され、12時間です）。" });
            arg_setting.Items.Add(new arg_data { arg = "NonPermanentDiseases", detail = "これにより病気は永続的ではなくなります（リスポーンすると失われます）。" });
            arg_setting.Items.Add(new arg_data { arg = "OnlyAutoDestroyCoreStructures", detail = "このオプションで自動破壊対応サーバーを起動して、非コア/非基礎構造が自動破壊されないようにすることができます（もちろん、それらが存在するフロアが自動破壊された場合でも自動破壊されます）。 公式PvEサーバーはこのオプションを使用します。\nパッチ245.989で導入" });
            arg_setting.Items.Add(new arg_data { arg = "OnlyDecayUnsnappedCoreStructures", detail = "設定すると、スナップされていないコア構造のみが減衰します。 PvPサーバー上の孤立した柱/基盤スパムを排除するのに役立ちます。\nパッチ245.986で導入" });
            arg_setting.Items.Add(new arg_data { arg = "OverrideOfficialDifficulty", detail = "デフォルトのサーバー難易度レベル4を5で上書きして、新しい公式サーバー難易度レベルに一致させることができます。\nパッチ247.95で導入" });
            arg_setting.Items.Add(new arg_data { arg = "OxygenSwimSpeedStatMultiplier", detail = "これを使用して、泳ぐ速度に酸素の消費レベルを掛ける方法を設定します。 256.0で値が80％減少しました。\nパッチ256.3で導入" });
            arg_setting.Items.Add(new arg_data { arg = "PreventDiseases", detail = "これにより、サーバー上の病気が完全に無効になります(これまでのところ「スワンプフィーバー」)。" });
            arg_setting.Items.Add(new arg_data { arg = "PreventOfflinePvP", detail = "これを使用して、オフラインの襲撃防止オプションを有効にします。" });
            arg_setting.Items.Add(new arg_data { arg = "PreventOfflinePvPInterval", detail = "それは、部族/プレイヤーの恐竜/構造がログオフした後に不死身/非アクティブになる前に、15分間待機します。 （部族の場合、すべての部族メンバーがログオフする必要があります！）" });
            arg_setting.Items.Add(new arg_data { arg = "PreventSpawnAnimations", detail = "trueに設定すると、ウェイクアップアニメーションなしでプレイヤーキャラクターを（再）スポーンできます。\nパッチ261.0で導入" });
            arg_setting.Items.Add(new arg_data { arg = "PvEAllowStructuresAtSupplyDrops", detail = "PvEモードで供給ドロップポイントの近くに構築できるようにするには、trueに設定します。\nパッチ247.999で導入" });
            arg_setting.Items.Add(new arg_data { arg = "PvEDinoDecayPeriodMultiplier", detail = "Dino PvE Auto-Claim time multiplier\nパッチ241.4で導入" });
            arg_setting.Items.Add(new arg_data { arg = "PvPDinoDecay", detail = "trueに設定すると、オフラインレイディング防止がアクティブなときに恐竜が腐敗するのを防ぎます。" });
            arg_setting.Items.Add(new arg_data { arg = "PvPStructureDecay", detail = "trueに設定すると、Offline Raiding Preventionがアクティブなときに構造が崩壊するのを防ぎます。\nパッチ206.0で導入" });
            arg_setting.Items.Add(new arg_data { arg = "RCONPort", detail = "RCOnの動作にRCONの接続ポートが必要であることを指定します。指定されたポートを手動で有効にする必要があります。使用されていないポートは使用できます\nパッチ185.0で導入" });
            arg_setting.Items.Add(new arg_data { arg = "ShowFloatingDamageText", detail = "これを使用して、RPGスタイルのポップアップテキスト統計モードを有効にします。" });
            arg_setting.Items.Add(new arg_data { arg = "ServerAdminPassword", detail = "指定されている場合、プレイヤーはこのパスワードを（ゲーム内コンソールを介して）提供して、サーバー上の管理者コマンドにアクセスする必要があります。 RCON経由のログインにも使用されます。" });
            arg_setting.Items.Add(new arg_data { arg = "TheMaxStructuresInRange", detail = "サーバー上の最大許容構造の新しい値。 NewMaxStructuresInRangeを置き換えました\nパッチ252.1で導入" });
            arg_setting.Items.Add(new arg_data { arg = "TribeLogDestroyedEnemyStructures", detail = "デフォルトでは、（被害者の部族の）敵の構造破壊は部族のログに表示されません。これを有効にするにはtrueに設定します。\nパッチ247.93で導入" });
            arg_setting.Items.Add(new arg_data { arg = "UseOptimizedHarvestingHealth", detail = "HarvestAmountMultiplierが高い「最適化された」サーバー（ただし、まれではないアイテム）。" });
            arg_setting.Items.Add(new arg_data { arg = "bRawSockets", detail = "サーバーネットワークのパフォーマンスを大幅に向上させるため、Steam P2Pではなく直接UDPソケット接続。 サーバーが接続を受け入れるためには、ポート7777および7778を手動で開く必要があります。\nパッチ213.0で導入" });
            arg_setting.Items.Add(new arg_data { arg = "GameModIds", detail = "順序とロードするmodを指定します。ModIDはコンマで区切る必要があります\nパッチ190.0で導入" });
            arg_setting.Items.Add(new arg_data { arg = "MaxTamedDinos", detail = "サーバー上の飼いならされたDinoの最大数を設定します。これはグローバルキャップです。\nパッチ191.0で導入" });
            arg_setting.Items.Add(new arg_data { arg = "SpectatorPassword", detail = "管理者以外の観客を使用するには、サーバーは観客のパスワードを指定する必要があります。 その後、すべてのクライアントがこれらのコンソールコマンドを使用できます: requestspectator <password>およびstopspectating。詳細およびホットキーについては、パッチ191.0を参照してください。" });
            arg_setting.Items.Add(new arg_data { arg = "AllowCaveBuildingPVE", detail = "Trueに設定すると、PvEモードも有効になっているときに洞窟内での構築が可能になります。\nパッチ194.0で導入" });
            arg_setting.Items.Add(new arg_data { arg = "AdminLogging", detail = "すべての管理コマンドをゲーム内チャットに記録します\nパッチ206.0で導入" });
            arg_setting.Items.Add(new arg_data { arg = "ForceAllStructureLocking", detail = "これを有効にすると、デフォルトですべての構造がロックされます\nパッチ222.0で導入" });
            arg_setting.Items.Add(new arg_data { arg = "AutoDestroyOldStructuresMultiplier", detail = "十分な「近くの部族がいない」時間が経過した後、自動破壊構造を許可するサーバーオプション（許可請求期間の乗数として定義）。 サーバーが必要に応じて、時間の経過とともに自動的に放棄された構造をクリアするのに役立ちます\nパッチ222.0で導入" });
            arg_setting.Items.Add(new arg_data { arg = "RCONServerGameLogBuffer", detail = "RCONを介して送信されるゲームログの行数を決定します\nパッチ224.0で導入" });
            arg_setting.Items.Add(new arg_data { arg = "PreventTribeAlliances", detail = "これを有効にすると、部族は同盟を作成できなくなります\nパッチ243.0で導入" });
            arg_setting.Items.Add(new arg_data { arg = "AllowRaidDinoFeeding", detail = "サーバーのTitanosaursを恒久的に飼い慣らす（つまり、餌を与える）\nただし、現在、TheIslandは3つのティタノサウルスしかスポーンしていないことに注意してください。\nパッチ243.0で導入" });
            arg_setting.Items.Add(new arg_data { arg = "AllowHitMarkers", detail = "これを使用して、範囲攻撃のオプションのヒットマーカーを無効にします\nパッチ245.0で導入" });
            arg_setting.Items.Add(new arg_data { arg = "ServerCrosshair", detail = "これを使用して、サーバーのクロスヘアを無効にします\nパッチ245.0で導入" });
            arg_setting.Items.Add(new arg_data { arg = "PreventMateBoost", detail = "Dino Mate Boostを無効にするオプション\nパッチ247.0で導入" });
            arg_setting.Items.Add(new arg_data { arg = "ServerAutoForceRespawnWildDinosInterval", detail = "サーバーの再起動時にWild Dinoを強制的に再スポーンします。 公式サーバーでデフォルトで有効になっているため、すべてのサーバーで毎週ディノが強制的に再スポーンされ、特定のディノタイプ（BasiloやSpinoなど）が長時間実行されるサーバーで過密になるのを防ぎます。\n注：場合によっては、これは複数回機能しない場合があります\nパッチ265.0で導入" });
            arg_setting.Items.Add(new arg_data { arg = "PersonalTamedDinosSaddleStructureCost", detail = "プラットフォームサドルが部族の恐竜の制限に向けて使用するディノスロットの量を決定します\nパッチ265.0で導入" });
            
            
            Console.WriteLine(File.Exists(@"settings.json"));
            if (!File.Exists(@"settings.json"))
            {
                IpcSend("settings first");
            }
            string[] arr = IpcConnect("settings read 1").Split(',');
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
                max_players.IsEnabled = true;
                arg_setting_box.IsEnabled = true;
                string gp = IpcConnect("exec_arg read 1 1 Port");
                string qp = IpcConnect("exec_arg read 1 1 QueryPort");
                string mp = IpcConnect("exec_arg read 1 1 MaxPlayers");
                string jpass = IpcConnect("exec_arg read 1 1 ServerPassword");
                string apass = IpcConnect("exec_arg read 1 1 ServerAdminPassword");
                if (gp == "None")
                {
                    game_port.Text = "7777";
                }
                else
                {
                    game_port.Text = gp;
                }
                
                if(qp == "None")
                {
                    query_port.Text = "27015";
                }
                else
                {
                    query_port.Text = qp;
                }

                if (mp == "None")
                {
                    max_players.Text = "10";
                }
                else
                {
                    max_players.Text = mp;
                }

                if (jpass != "None")
                {
                    join_pass.Password = jpass;
                    join_pass.IsEnabled = true;
                }
                else
                {
                    join_pass.Password = "";
                    join_pass.IsEnabled = false;
                }

                if (apass != "None")
                {
                    admin_pass.Password = apass;
                    admin_pass.IsEnabled = true;
                }
                else
                {
                    admin_pass.Password = "";
                    admin_pass.IsEnabled = false;
                }



            }
            server_name.Text = arr[0];
            Console.WriteLine(arr[1].Contains("Custom/"));
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
            if (File.Exists(@server_dir.Text + @"\\ShooterGame\\Binaries\\Win64\\ShooterGameServer.exe") == true)
            {
                string ret1 = IpcConnect("exec_arg edit "+(server_list.SelectedIndex + 1)+" 1 Port " + game_port.Text);
                string ret2 = IpcConnect("exec_arg edit "+(server_list.SelectedIndex + 1)+" 1 QueryPort " + query_port.Text);
                string ret3 = IpcConnect("exec_arg edit "+(server_list.SelectedIndex + 1)+" 1 MaxPlayers " + max_players.Text);
                string ret4 = IpcConnect("exec_arg edit " + (server_list.SelectedIndex + 1) + " 1 ServerPassword " + max_players.Text);
                string ret5 = IpcConnect("exec_arg edit " + (server_list.SelectedIndex + 1) + " 1 ServerAdminPassword " + max_players.Text);
            }
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
                ProcessStartInfo processStartInfo = new ProcessStartInfo(@"SteamCMD\\steamcmd.exe", "+login anonymous +force_install_dir " + @arr[2] + " +app_update 376030 +quit");
                Process steamcmd_installer = Process.Start(processStartInfo);
                steamcmd_installer.WaitForExit();

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
            max_players.IsEnabled = false;
            server_pass_bool.IsEnabled = false;
            admin_pass.IsEnabled = false;
            del_list.IsEnabled = true;


            map.SelectedValue = "TheIsland";
        }

        private void list_changed(object sender, DependencyPropertyChangedEventArgs e)
        {
            string server = server_list.Text;
            int index = server_list.Items.IndexOf(server);
            Console.WriteLine(index.ToString());
            string[] arr = IpcConnect("settings read " + index).Split(',');
            Console.WriteLine(arr.ToString());
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
            Console.WriteLine((server_list.SelectedIndex + 1));
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
            if (File.Exists(@server_dir.Text + @"\\ShooterGame\\Binaries\\Win64\\ShooterGameServer.exe") == true)
            {

                server_pass_bool.IsEnabled = true;
                admin_pass.IsEnabled = true;
                query_port.IsEnabled = true;
                max_players.IsEnabled = true;
                game_port.IsEnabled = true;
                string gp = IpcConnect("exec_arg read " + (server_list.SelectedIndex + 1) + " 1 Port");
                string qp = IpcConnect("exec_arg read " + (server_list.SelectedIndex + 1) + " 1 QueryPort");
                string mp = IpcConnect("exec_arg read " + (server_list.SelectedIndex + 1) + " 1 MaxPlayers");
                string jpass = IpcConnect("exec_arg read " + (server_list.SelectedIndex + 1) + " 1 ServerPassword");
                string apass = IpcConnect("exec_arg read " + (server_list.SelectedIndex + 1) + " 1 ServerAdminPassword");
                if (gp == "None")
                {
                    game_port.Text = "7777";
                }
                else
                {
                    game_port.Text = gp;
                }

                if (qp == "None")
                {
                    query_port.Text = "27015";
                }
                else
                {
                    query_port.Text = qp;
                }

                if (mp == "None")
                {
                    max_players.Text = "10";
                }
                else
                {
                    max_players.Text = mp;
                }

                if (jpass != "None")
                {
                    join_pass.Password = jpass;
                    join_pass.IsEnabled = true;
                }
                else
                {
                    join_pass.Password = "";
                    join_pass.IsEnabled = false;
                }

                if (apass != "None")
                {
                    admin_pass.Password = apass;
                    admin_pass.IsEnabled= true;
                }
                else
                {
                    admin_pass.Password = "";
                    admin_pass.IsEnabled = false;
                }
            }
            else
            {
                server_pass_bool.IsEnabled = false;
                admin_pass.IsEnabled = false;
                query_port.IsEnabled = false;
                max_players.IsEnabled = false;
                game_port.IsEnabled = false;
            }
        }

        private void del_list_Click(object sender, RoutedEventArgs e)
        {
            if(IpcConnect("settings value") == "2")
            {
                del_list.IsEnabled = false;
            }
            IpcSend("settings del " + (map.SelectedIndex+1));
            map.SelectedIndex = map.SelectedIndex - 1;
        }

        private void start_server_Click(object sender, RoutedEventArgs e)
        {
            ProcessStartInfo ARK_GAME = new ProcessStartInfo(@server_dir.Text + @"\\ShooterGame\\Binaries\\Win64\\ShooterGameServer.exe", map.Text + "?listen?SessionName=" + server_name.Text);
            Process.Start(ARK_GAME);
        }
    }

}
