using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Threading;

namespace ExplorerManager
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static Arguments cmd;
        private Mutex _instanceMutex = null;
        protected override void OnStartup(StartupEventArgs e)
        {
            // check that there is only one instance of the control panel running...
            bool createdNew;
            _instanceMutex = new Mutex(true, @"Global\ControlPanel", out createdNew);
            if (!createdNew)
            {
                _instanceMutex = null;
                MessageBox.Show("Программа уже запущена.");
                Application.Current.Shutdown();
                return;
            }

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (_instanceMutex != null)
                _instanceMutex.ReleaseMutex();
            base.OnExit(e);
        }
        void App_Startup(object sender, StartupEventArgs e)
        {

            // Application is running
            // Process command line args
            cmd = new Arguments(e.Args);

        }
    }
}
