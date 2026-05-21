using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

/**
 * @Author Lada 
 * @Created 2018.08.22 
 */

namespace VideoPlayer
{
    public class VItem : IDisposable, IComparable<VItem>
    {
        private int _percent;
        private PercentEvent _percentEvent;
        private int _angle;
        public VItem(string fname)
        {
            OnCommand = new RelayCommand(OnClick);
            Length = Init(fname);
        }

        private void OnClick(object obj)
        {
            if (EventOnClick != null)
                EventOnClick.BeginInvoke(this, new EventArgs(), _ => { }, this);
        }

        public VItem(string s, PercentEvent p)
        {
            OnCommand = new RelayCommand(OnClick);
            var p1 = s.Split('?');

            if (p1.Length > 1)
            {
                var ss = p1[0].Split('.');
                long size = 0;
                int val = 0;
                int percent = 0;
                if (ss.Length > 2 && Int64.TryParse(ss[0], out size)
                    && Int32.TryParse(ss[1], out val) && Int32.TryParse(ss[2], out percent))
                {
                    Length = size;
                    Angle = val;
                    Percent = percent;
                }

                var length = Init(p1[1]);
                if (length != Length)
                    Length = 0;
                else
                {
                    p.Event += OnPercentEvent;
                    _percentEvent = p;
                }
            }
        }

        private void OnPercentEvent(object sender, PercentEventArg e)
        {
            if (e.Length != Length) return;
            if(e.IsPercent && e.Percent != Percent)
                _percent = e.Percent;
            if (e.IsAngle && e.Angle != Angle)
                _angle = e.Angle;
        }

        public string Title { get; set; }
        public string Name { get; set; }
        public long Length { get; set; }
        public int Angle { get { return _angle; } set { if (_angle != value && _percentEvent != null) { _angle = value; _percentEvent.SetAng(Length, value); } else _angle = value; } }
        public int Percent { get { return _percent; } set { if (_percent != value && _percentEvent != null) { _percent = value; _percentEvent.SetPerc(Length, value); } else _percent = value; } }
        public RelayCommand OnCommand { get; set; }
        public EventHandler EventOnClick;

        public override string ToString()
        {
            return "" + Length + "." + Angle + "." + Percent + "?" + Name;
        }

        private long Init(string fname)
        {
            if (!File.Exists(fname)) return 0;

            var fi = new FileInfo(fname);
            Name = fi.FullName;
            Title = fi.Name;

            if (!fi.Exists)
                return 0;

            return fi.Length;
        }

        public int CompareTo(VItem other)
        {
            return Name.CompareTo(other.Name);
        }

        internal void SetPercentEvent(PercentEvent percentEvent)
        {
            _percentEvent = percentEvent;
            percentEvent.Event += OnPercentEvent;
        }

        public void Dispose()
        {
            if (_percentEvent != null)
                _percentEvent.Event -= OnPercentEvent;
        }

    }
}
