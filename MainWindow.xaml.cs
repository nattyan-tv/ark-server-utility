using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Text.Json;

namespace ark_server_utility
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            if (!File.Exists(@"settings.json"))
            {
                string myPythonApp = "make_file.py";
                var myProcess = new Process
                {
                    StartInfo = new ProcessStartInfo("python")
                    {
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        Arguments = myPythonApp + "first"
                    }
                };
                myProcess.CreateNoWindow = true;
                myProcess.UseShellExecute = false;
                myProcess.Start();
                StreamReader myStreamReader = myProcess.StandardOutput;
                string myString = myStreamReader.ReadLine();
                myProcess.WaitForExit();
                myProcess.Close();
                string[,] settings_data = new string[99, 3];
                Console.WriteLine(myString)
            }
            InitializeComponent();
            main_pbar.Value = 100;
            main_ptext.Content = "ARK: Server Utility";
            label_name.Content = "サーバー名：" + "a";
            label_map.Content = "マップ名：" + map.Text;
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
                System.Windows.Forms.MessageBox.Show("SteamCMDがインストールされていません。\n「ゲームデータ」より「SteamCMD」のインストールを選択してください。", "ARK Server Utility", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return;
            }
            if (File.Exists(@"SteamCMD\\steamcmd.exe") == false)
            {
                System.Windows.Forms.MessageBox.Show("SteamCMDがインストールされていません。\n「ゲームデータ」より「SteamCMD」のインストールを選択してください。", "ARK Server Utility", MessageBoxButtons.OK, MessageBoxIcon.Hand);
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
        private void install_arkgame(object sender, RoutedEventArgs e)
        {
            DialogResult dr = System.Windows.Forms.MessageBox.Show("ARKサーバーのデータをインストールを開始してもよろしいですか？\n時間がかかる場合があります。", "ARK Server Utility", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation);
            if (dr == System.Windows.Forms.DialogResult.Cancel)
            {
                return;
            }
            if (Directory.Exists(@"SteamCMD") == false)
            {
                Console.WriteLine("ディレクトリ「SteamCMD」がありませんでした。");
                System.Windows.Forms.MessageBox.Show("SteamCMDがインストールされていません。\n「ゲームデータ」より「SteamCMD」のインストールを選択してください。", "ARK Server Utility", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return;
            }
            else if (Directory.Exists(@"ARK") == true)
            {
                Console.WriteLine("ディレクトリ「ARK」がありました。");
                System.Windows.Forms.MessageBox.Show("ゲームサーバーが既にあります。", "ARK Server Utility", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return;
            }
            ProcessStartInfo processStartInfo = new ProcessStartInfo(@"SteamCMD\\steamcmd.exe", "+login anonymous +force_install_dir \"ARK\" +app_update 376030 +quit");
            Process steamcmd_installer = Process.Start(processStartInfo);
            steamcmd_installer.WaitForExit();
            int exitCode = steamcmd_installer.ExitCode;
            steamcmd_installer.Close();
            Console.WriteLine(exitCode);
        }
        private void uninstall_arkgame(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(@"ARK") == false)
            {
                System.Windows.Forms.MessageBox.Show("ゲームデータがありませんでした。\nインストールするには「ゲームデータ」から「ARKサーバーのダウンロード」を押してください。", "ARK Server Utility", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                return;
            }
            DialogResult dr = System.Windows.Forms.MessageBox.Show("ゲームサーバーのデータを削除してもよろしいですか？", "ARK Server Utility", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation);
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    System.IO.DirectoryInfo ark_del = new System.IO.DirectoryInfo(@"ARK");
                    ark_del.Delete(true);
                }
                catch (IOException err)
                {
                    System.Windows.Forms.MessageBox.Show("ゲームデータの削除中にエラーが発生しました。サーバーが実行されている場合は停止してください。\n" + err.Message, "ARK Server Utility", MessageBoxButtons.YesNo, MessageBoxIcon.Hand);
                    return;
                }
                System.Windows.Forms.MessageBox.Show("データを削除しました。", "ARK Server Utility", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
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
            System.Windows.Forms.MessageBox.Show("保存しました。", "ARK Server Utility", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);

        }
    }

}
