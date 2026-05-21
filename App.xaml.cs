using System;
using System.IO;
using System.Threading;
using System.Windows;

namespace VideoPlayer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private const string MutexName = "OlimiSoftVideoPlayer";
        private const string PipeExtension = ".vpt";

        private static string[] args;
        private static Mutex _mutex = null;
        private static InfoManager _manager;

        private Thread thread;
        private string workDir;
        private bool aIsNewInstance;
        

        protected override void OnStartup(StartupEventArgs e)
        {
            workDir = InfoModule.GetWorkFolder();
            aIsNewInstance = false;
            _mutex = new Mutex(true, MutexName, out aIsNewInstance);
            args = e.Args;
            if (!aIsNewInstance)
            {
                string word = "";
                var arg = GetArgs();
                if (arg != null && arg.Length > 0)
                {
                    word = arg[0];
                    var fname = Path.Combine(workDir, Guid.NewGuid().ToString() + PipeExtension);
                    PipeModule.WritePipe(fname, word);
                }
                

                App.Current.Shutdown();
            }
            else
            {
                StartTread();
                base.OnStartup(e);
                
            }
        }

        public static InfoManager GetManager()
        {
            if (_manager == null)
            {
                _manager = new InfoManager();
            }
            return _manager;
        }

        public static string[] GetArgs()
        {
            return args;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (aIsNewInstance)
            {
                var m = GetManager();
                m.Exit();
            }

            base.OnExit(e);
        }

        private void StartTread()
        {
            try
            {
                thread = new Thread(
                    () =>
                    {
                        while (true)
                        {
                            var d = Directory.GetFiles(workDir, "*" + PipeExtension, SearchOption.TopDirectoryOnly);
                            if (d != null && d.Length > 0)
                            {
                                var res = PipeModule.SortPipe(d);
                                Current.Dispatcher.BeginInvoke(
                                    (Action)(() => ((MainWindow)Current.MainWindow).BringToForeground(res)));
                            }
                            Thread.Sleep(50);
                        }
                    });
                thread.IsBackground = true;
                thread.Start();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }
        
    }
}
