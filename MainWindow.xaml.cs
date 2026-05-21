using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Linq;
using System.Windows.Input;
using System.Reflection;
using System.Collections.ObjectModel;
using Microsoft.Win32;

/**
 * @Author Lada 
 * @Created 2018.08.22 
 */

namespace VideoPlayer
{
    public partial class MainWindow : Window
    {
        private RotateVideoModule _rotateModule;
        private InfoModule _infoModule;
        private bool _controlPanelVisible;
        private Thread _thread;
        private object lockObject = new object();
        private DateTime _time;
        private TimeSpan _threadTimeSpan = new TimeSpan(0, 0, 0, 20);
        private MediaModule _mediaModule;
        private int _position;
        private bool _updatePosition;
        private int _userPotition;

        EventHandler hideEvent;
        EventHandler progressEvent;
        private VItem Current;
        private bool _mute;
        private bool _hideVideoList;

        public ObservableCollection<VItem> VideoList { get; private set; }

        public MainWindow()
        {
            InitializeComponent();
            AssemblyName assemblyName = new AssemblyName(Assembly.GetExecutingAssembly().FullName);
            versionTxt.Text = "Video Player version: " + assemblyName.Version.ToString() + " by Olimi";
            var manager = App.GetManager();
            this.hideEvent += MainWindows_HideEvent;
            this.progressEvent += MainWindows_ProgressEvent;
            _mediaModule = new MediaModule(MVideo);
            _mediaModule.MediaHandler += MainWindow_MediaHandler;
            this.SoundSlider.ValueChanged += SoundSlider_ValueChanged;
            this.KeyUp += MainWindow_KeyUp;
            _time = DateTime.Now;
            UserPanel.Visibility = Visibility.Visible;
            _controlPanelVisible = true;
            
            _thread = new Thread(() =>
            {
                while (true)
                {
                    if (_controlPanelVisible)
                    {
                        lock (lockObject)
                        {
                            if (DateTime.Now > _time + _threadTimeSpan)
                            {
                                hideEvent(null, EventArgs.Empty);
                            }
                        }
                    }
                    progressEvent(null, EventArgs.Empty);
                    Thread.Sleep(1000);
                }
            });
            _thread.IsBackground = true;
            _thread.Start();
            
            _infoModule = manager.GetInfoModule();
            _infoModule.VItemOnClick += VItemOnClick;
            VideoList = new ObservableCollection<VItem>();
            
            this.Loaded += MainWindow_Loaded;
            this.SizeChanged += MainWindow_SizeChanged;
            _rotateModule = new RotateVideoModule();
            Present.Drop += File_Drop;
            Present.MouseMove += UserPanel_MouseMove;
            _rotateModule.RotateEvent += Rotate_Complite;
            PlayButton.Command = new RelayCommand(PlayCommand);
            PauseButton.Command = new RelayCommand(PauseCommand);
            StopButton.Command = new RelayCommand(StopCommand);
            SoundOn.Command = new RelayCommand(_ => Mute(false));
            SoundOff.Command = new RelayCommand(_ => Mute(true));
            SoundOn.Visibility = System.Windows.Visibility.Collapsed;
            ShowList.Command = new RelayCommand(_ => ListShow(false));
            HideList.Command = new RelayCommand(_ => ListShow(true));
            ProgressSlider.ValueChanged += ProgressSlider_ValueChanged;
            VideoListView.Items.Clear();
            VideoListView.ItemsSource = VideoList;
            foreach (var v in _infoModule.GetList())
                VideoList.Add(v);
            VideoListView.SelectionChanged += VideoListView_SelectionChanged;
        }

        private void VItemOnClick(object sender, OnClickEvent e)
        {
            if (string.IsNullOrEmpty(e.Item.Name))
            {
                Dispatcher.BeginInvoke((Action)(() => {
                    OpenNewFile();
                }));
                return;
            }

            if (File.Exists(e.Item.Name))
            {
                Dispatcher.BeginInvoke((Action)(() => {
                    PlayFile(e.Item.Name);
                }));
            }
        }

        private void OpenNewFile()
        {
            var ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == true)
            {
                PlayFile(ofd.FileName);
            }
        }

        private void VideoListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }

        private void UpdateDonateText()
        {
            //todo сделать текст по размеру окна
            //так же сделать несколько разных и давать им ссылку с разным ид
        }

        private void ListShow(bool p)
        {
            _hideVideoList = p;
            if (p)
            {
                ShowList.Visibility = System.Windows.Visibility.Visible;
                HideList.Visibility = System.Windows.Visibility.Collapsed;
                VideoListView.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                ShowList.Visibility = System.Windows.Visibility.Collapsed;
                HideList.Visibility = System.Windows.Visibility.Visible;
                VideoListView.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void Mute(bool mute)
        {
            if (mute)
            {
                SoundOn.Visibility = System.Windows.Visibility.Visible;
                SoundOff.Visibility = System.Windows.Visibility.Collapsed;
                _mute = mute;
                _mediaModule.Mute(mute);                
            }
            else
            {
                SoundOn.Visibility = System.Windows.Visibility.Collapsed;
                SoundOff.Visibility = System.Windows.Visibility.Visible;
                _mute = mute;
                _mediaModule.Mute(mute);                
            }
        }

        void SoundSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _mediaModule.SetSound(e.NewValue);
        }

        private void MainWindow_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    if(_controlPanelVisible)
                    {
                        _controlPanelVisible = false;
                        UserPanel.Visibility = Visibility.Hidden;
                        TopPanel.Visibility = System.Windows.Visibility.Hidden;
                    }
                    else
                    {
                        lock (lockObject)
                        {
                            _time = DateTime.Now;
                        }
                        _controlPanelVisible = true;
                        UserPanel.Visibility = Visibility.Visible;
                        TopPanel.Visibility = System.Windows.Visibility.Visible;
                    }
                    break;
                case Key.M:
                    Mute(!_mute);
                    break;
                case Key.L:
                    ListShow(!_hideVideoList);
                    break;
                case Key.Delete:
                    if(!_hideVideoList && _mediaModule.Current != this.VideoListView.SelectedItem)
                    {
                        RemoveItem(VideoListView.SelectedItem as VItem);                        
                    } break;
                case Key.Left:
                    _updatePosition = true;
                    _userPotition = _position - 10; 
                    break;
                case Key.Right:
                    _updatePosition = true;
                    _userPotition = _position + 10;
                    break;
                case Key.LeftShift:
                case Key.RightShift:
                    OnRotate();
                    break;
                case Key.Add:
                case Key.OemPlus:
                    SoundSlider.Value += 2;
                    break;
                case Key.OemMinus:
                case Key.Subtract:
                    SoundSlider.Value -= 2;
                    break;
                case Key.Up:
                    PlayPreview();
                    break;
                case Key.Down:
                    PlayNext();
                    break;
                default:
                    _mediaModule.KeyUp(e.Key);
                    break;
            }
        }

        private void RemoveItem(VItem item)
        {
            _infoModule.Remove(item);
            VideoList.Remove(item);
        }

        private void ProgressSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int val = (int)e.NewValue;
            if (val == _position)
                return;
            
            _updatePosition = true;
            _userPotition = val;
        }

        private void MainWindows_ProgressEvent(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() => OnTick()));            
        }

        private void OnTick()
        {
            if (_updatePosition)
            {
                _updatePosition = false;
                _mediaModule.SetPersentPosition(_userPotition);
            }

            var result = _mediaModule.GetPercentPosition();

            if (result != this.ProgressSlider.Value)
            {
                _position = result;
                Current.Percent = result;
                this.ProgressSlider.Value = _position;
                return;
            }
        }

        private void MainWindow_MediaHandler(object sender, MediaArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                if (e.Status == MediaStatus.Ended)
                {
                    PlayNext();
                    return;
                }
                Current = e.Current;
                VideoListView.SelectedItem = Current;

                this.Title = "VP " + e.Current.Title;
                RotateAll(e.Current.Angle);
            }));
        }

        private void PlayNext()
        {
            if (VideoListView.IsFocused) return;
            //todo if next found play next or play current
            try
            {
                for (int i = 0; i < VideoListView.Items.Count; i++)
                {
                    if (VideoListView.Items[i] == Current)
                    {
                        if (i + 1 < VideoListView.Items.Count)
                        {
                            PlayFile(((VItem)VideoListView.Items[i + 1]).Name);
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void PlayPreview()
        {
            if (VideoListView.IsFocused) return;

            try
            {
                for (int i = 0; i < VideoListView.Items.Count; i++)
                {
                    if (VideoListView.Items[i] == Current)
                    {
                        if (i - 1 >= 0)
                        {
                            PlayFile(((VItem)VideoListView.Items[i - 1]).Name);
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void MainWindows_HideEvent(object sender, EventArgs e)
        {
            _controlPanelVisible = false;

            Dispatcher.BeginInvoke((Action)(() =>{
                UserPanel.Visibility = Visibility.Hidden;
                TopPanel.Visibility = System.Windows.Visibility.Hidden;
            }));
        }

        void UserPanel_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var cursor = e.MouseDevice.GetPosition(Present);

            if (cursor.Y > Present.ActualHeight - 50)
            {
                lock (lockObject)
                {
                    _time = DateTime.Now;
                }

                if (_controlPanelVisible)
                    return;

                _controlPanelVisible = true;
                UserPanel.Visibility = Visibility.Visible;
                TopPanel.Visibility = System.Windows.Visibility.Visible;
            }
        }

        void File_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach(var file in files)
                {
                    AppendFile(file);
                }
                PlayFile(files[0]);
            }
        }

        private void StopCommand(object obj)
        {
            _mediaModule.Stop();
        }

        private void PauseCommand(object obj)
        {
            _mediaModule.Pause();
        }

        private void PlayCommand(object obj)
        {
            _mediaModule.Play();
        }

        void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (Current == null) return;
            RotateAll(Current.Angle);
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            RotateButton.Command = new RelayCommand(x => OnRotate());

            var p = App.GetArgs();
            string t = "";
            if (p != null && p.Length > 0)
            {
                t = p[0];
            }
            string fname = t;
            PlayFile(fname);
        }

        private void AppendFile(string fname)
        {
            var vi = new VItem(fname);
            var res = VideoList.FirstOrDefault(x => x.Name == vi.Name && x.Length == vi.Length);
            if (res != null)
            {
                return;
            }
            else
            {
                VideoList.Add(vi);
                _infoModule.Append(vi);
            }
        }

        private void PlayFile(string fname)
        {
            var vi = new VItem(fname);
            var res = VideoList.FirstOrDefault(x => x.Name == vi.Name && x.Length == vi.Length);
            if (res != null)
            {
                RotateAll(res.Angle);
                _mediaModule.PlayFile(res);
            }
            else
            {
                VideoList.Add(vi);
                _infoModule.Append(vi);
                RotateAll(vi.Angle);
                vi.SetPercentEvent(_infoModule.PercentEvent);
                _mediaModule.PlayFile(vi);
            }
        }

        private void OnRotate()
        {
            if (Current == null) return;
            var angle = Current.Angle;
            angle += 90;
            if (angle > 359)
                angle = 0;
            RotateAll(angle);
            Current.Angle = angle;
        }

        private void Rotate_Complite(object sender, RotateEvent e)
        {
            TBAngle.Value = e.Angle;
            GridRotate.Width = e.Width;
            GridRotate.Height = e.Height;
            TBMargin.Text = e.Margin;
        }

        private void RotateAll(int a)
        {
            var pw = Present.ActualWidth - 4;
            var ph = Present.ActualHeight - 4;

            _rotateModule.RotateAll(a, pw, ph);
        }

        public void BringToForeground(string newFile)
        {
            PlayFile(newFile);

            if (this.WindowState == WindowState.Minimized || this.Visibility == Visibility.Hidden)
            {
                this.Show();
                this.WindowState = WindowState.Normal;
            }

            this.Activate();
            this.Topmost = true;
            this.Topmost = false;
            this.Focus();
        }
    }
}
