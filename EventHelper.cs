using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/**
 * @Author Lada 
 * @Created 2018.08.29 
 */

namespace VideoPlayer
{
    public class PercentEventArg : EventArgs
    {
        public PercentEventArg(long length, int percent = 0, int angle = 0, bool isPercent = false, bool isAngle = false)
        {
            Length = length;
            Percent = percent;
            Angle = angle;
            IsPercent = isPercent;
            IsAngle = isAngle;
        }
        public int Percent { get; set; }
        public long Length { get; set; }
        public bool IsPercent { get; set; }
        public int Angle { get; set; }
        public bool IsAngle { get; set; }
    }
    public class PercentEvent
    {
        public EventHandler<PercentEventArg> Event;

        internal void SetPerc(long Length, int value)
        {
            if (Event != null)
                Event.Invoke(this, new PercentEventArg(Length, percent: value, isPercent: true));
        }

        internal void SetAng(long Length, int value)
        {
            if (Event != null)
                Event.Invoke(this, new PercentEventArg(Length, angle: value, isAngle: true));
        }
    }

    public class OnClickEvent : EventArgs
    {
        public VItem Item { get; set; }
        public OnClickEvent(VItem item)
        {
            Item = item;
        }
    }
}
