using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ark_server_utility
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        [System.STAThreadAttribute()]
        static public void Main()
        {
            using (var semaphore = new System.Threading.Semaphore(1, 1, "ark_server_utility", out var created_new))
            {
                if (!created_new)
                {
                    MessageBox.Show("既に実行されています。", "ARK Server Utility", MessageBoxButton.OK, MessageBoxImage.Error);
                    Environment.Exit(1);
                    return;
                }
                var app = new App();
                app.InitializeComponent();
                app.Startup += App_Startup;
                app.Run();
            }
        }
        private static void App_Startup(object sender, StartupEventArgs e)
        {
            ;
        }
    }
}
