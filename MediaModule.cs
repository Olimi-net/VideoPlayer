using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;

/**
 * @Author Lada 
 * @Created 2018.08.22 
 */

namespace VideoPlayer
{
    public class MediaModule
    {
        public VItem Current { get; private set; }
        private MediaElement MVideo;
        private MediaStatus _status;
        private int sound;
        private bool mute;

        public EventHandler<MediaArgs> MediaHandler;

        public MediaModule(MediaElement mVideo)
        {
            MVideo = mVideo;
            MVideo.LoadedBehavior = MediaState.Manual;
            mVideo.Loaded += MVideo_Loaded;
            MVideo.MediaEnded += MVideo_MediaEnded;
        }

        public void PlayFile(VItem video)
        {
            try
            {
                Current = video;                
                MVideo.Source = new Uri(Current.Name);                
                MVideo.Play();
                SetStatus(MediaStatus.Playing);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        public void Stop()
        {
            MVideo.Stop();
            SetStatus(MediaStatus.Stoped);
        }

        private void OnMediaHandler(MediaArgs a)
        {
            if(MediaHandler != null)
                MediaHandler.BeginInvoke(this, a, _ => { }, this);
        }

        private void MVideo_MediaEnded(object sender, System.Windows.RoutedEventArgs e)
        {
            OnMediaHandler(new MediaArgs(Current, MediaStatus.Ended));
        }

        private void MVideo_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                Play();                
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        public void SetPersentPosition(int p)
        {
            if (p < 0) p = 0;
            if (p > 1000) p = 1000;
            var m = GetMaxPosition();
            var pos = p * m / 1000;

            Console.WriteLine("prc:" + p + " max:" + m + " pos:" + pos);
            SetPosition(pos);
        }

        public int GetPercentPosition()
        {
            var position = MVideo.Position.TotalMilliseconds;
            
            long maxPosition = GetMaxPosition();

            if (maxPosition > 0)
            {
                var p = (int)(position / maxPosition * 1000);
                return p;
            }
            return 0;
        }

        public long GetMaxPosition()
        {
            long maxPosition = 0;
            try
            {
                var d = MVideo.NaturalDuration;
                if (d != null && d.GetHashCode() != 0)
                {
                    if (d.TimeSpan != null)
                        maxPosition = (long)d.TimeSpan.TotalMilliseconds;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            return maxPosition;
        }

        internal void SetPosition(long val)
        {
            var s = (int)(val / 1000);
            var t = new TimeSpan(0, 0, 0, s);
            MVideo.Position = t;
        }

        public bool IsPlay { get; set; }

        internal void KeyUp(Key key)
        {
            switch (key)
            {
                case Key.Space:
                    switch (_status)
                    {
                        case MediaStatus.Pausing:
                        case MediaStatus.Stoped:
                            Play();
                            break;
                        case MediaStatus.Playing:
                            Pause();
                            break;
                    }
                    break;
                case Key.Left:
                    PreviewBlock();
                    break;
                case Key.Right:
                    NextBlock();
                    break;                
                case Key.Enter:
                    PlayFile(Current);
                    break;
            }
        }

        public void NextBlock()
        {
            var max = GetMaxPosition();
            var step = max / 100;
            var position = MVideo.Position.TotalSeconds;

            position += step;
            if (position > max)
                position = max;

            MVideo.Position = new TimeSpan(0, 0, (int)position);
        }

        public void PreviewBlock()
        {
            var max = GetMaxPosition();
            var step = max / 100;
            var position = MVideo.Position.TotalSeconds;

            position -= step;
            if (position < 0)
                position = 0;
            
            MVideo.Position = new TimeSpan(0, 0, (int)position);
        }

        public void Play()
        {
            if (_status == MediaStatus.Stoped)
                PlayFile(Current);
            else
                MVideo.Play();

            SetStatus(MediaStatus.Playing);
        }

        public void Pause()
        {
            MVideo.Pause();
            SetStatus(MediaStatus.Pausing);
        }

        private void SetStatus(MediaStatus status)
        {
            _status = status;
            OnMediaHandler(new MediaArgs(Current, status));
        }

        internal void SetSound(double p)
        {
            MVideo.Volume = p * 0.01;
        }

        internal void Mute(bool mute)
        {
            this.mute = mute;
            MVideo.IsMuted = mute;
        }
    }

    public class MediaArgs : EventArgs
    {
        public VItem Current;
        public MediaStatus Status;

        public MediaArgs(VItem current, MediaStatus status)
        {
            Current = current;
            Status = status;
        }
    }

    public enum MediaStatus
    {
        Stoped,
        Playing,
        Pausing,
        Ended
    }
}
