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
    class RotateVideoModule
    {
        public EventHandler<RotateEvent> RotateEvent;

        public void RotateAll(int a, double pw, double ph)
        {
            string Margin;
            double Width;
            double Height;

            var w = pw;
            var h = ph;
            if (a == 90)
            {
                Width = ph;
                Height = pw;

                int r = (int)((w - h) / 2 + h);
                int n = (int)((w - h) / 2);

                Margin = string.Format("{0},{1},{2},{3}", r, n, -r, -n);
            }
            else if (a == 180)
            {
                Width = pw;
                Height = ph;
                Margin = string.Format("{0},{1},{2},{3}", (int)w, (int)h, (int)-w, (int)-h);
            }
            else if (a == 270)
            {
                Width = ph;
                Height = pw;

                int r = (int)((w - h) / 2);
                int n = (int)((w - h) / 2 + h);
                Margin = string.Format("{0},{1},{2},{3}", -r, n, r, -n);
            }
            else
            {
                Width = pw;
                Height = ph;
                Margin = "0,0,0,0";
            }

            if (RotateEvent != null)
                RotateEvent(this, new RotateEvent(Margin, Width, Height, a));
        }
    }

    public class RotateEvent : EventArgs
    {
        public string Margin { get; private set; }
        public double Width { get; private set; }
        public double Height { get; private set; }
        public int Angle { get; private set; }
        public RotateEvent(string margin, double w, double h, int a)
        {
            Margin = margin;
            Width = w;
            Height = h;
            Angle = a;
        }

        
    }
}
