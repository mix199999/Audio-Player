using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace testMAUI
{
    internal class Theme
    {
        public bool Gradient { get; set; }
        public string PrimaryColor { get; set; }
        public string SecondaryColor { get; set; }
        public string GradientColor { get; set; }
        public bool Flip { get; set; }
        public bool HtoV { get; set; }
        public bool DarkButtons { get; set; }

        public Theme() { }

        public LinearGradientBrush GetGradient()
        {
            LinearGradientBrush lgb = new();

            GradientStop start = new GradientStop(Color.FromArgb(PrimaryColor), 0);
            GradientStop end;

            // normal: 0, 0.5 // flip: 1, 0.5 // vert: 0.5, 0 // flip + vert: 0.5, 1
            // flip + vert => flip => vert => normal
            Point startPoint = Flip ? (HtoV ? new Point(0.5, 1) : new Point(1, 0.5)) : (HtoV ? new Point(0.5, 0) : new Point(0, 0.5));

            // normal: 1, 0.5 // flip: 0, 0.5 // vert: 0.5, 1 // flip + vert: 0.5, 0
            Point endPoint = Flip ? (HtoV ? new Point(0.5, 0) : new Point(0, 0.5)) : (HtoV ? new Point(0.5, 1) : new Point(1, 0.5));

            if (GradientColor == string.Empty || GradientColor == null)
            {
                end = new GradientStop(Color.FromArgb("0"), 1);
            }
            else
            {
                end = new GradientStop(Color.FromArgb(GradientColor), (float)1.0);
            }

            lgb = new();
            lgb.StartPoint = startPoint;
            lgb.EndPoint = endPoint;
            lgb.GradientStops.Add(start);
            lgb.GradientStops.Add(end);

            return lgb;
        }

        public List<string> GetButtons()
        {
            List<string> buttonsList = new();

            switch (DarkButtons)
            {
                case true:
                    buttonsList.Add("backwardsolid.png");
                    buttonsList.Add("backward.png");
                    buttonsList.Add("playsolid.png");
                    buttonsList.Add("pausesolid.png");
                    buttonsList.Add("forward.png");
                    buttonsList.Add("forwardsolid.png");
                    buttonsList.Add("plussolid.png");
                    buttonsList.Add("listsolid.png");
                    buttonsList.Add("downloadsolid.png");
                    buttonsList.Add("replaysolid.png");
                    buttonsList.Add("shufflesolid.png");
                    buttonsList.Add("homesolid.png");
                    buttonsList.Add("playlistreturnsolid.png");
                    break;
                case false:
                    buttonsList.Add("backwardsolidwhite.png");
                    buttonsList.Add("backwardwhite.png");
                    buttonsList.Add("playsolidwhite.png");
                    buttonsList.Add("pausesolidwhite.png");
                    buttonsList.Add("forwardwhite.png");
                    buttonsList.Add("forwardsolidwhite.png");
                    buttonsList.Add("plussolidwhite.png");
                    buttonsList.Add("listsolidwhite.png");
                    buttonsList.Add("downloadsolidwhite.png");
                    buttonsList.Add("replaysolidwhite.png");
                    buttonsList.Add("shufflesolidwhite.png");
                    buttonsList.Add("homesolid_white.png");
                    buttonsList.Add("playlistreturnsolidwhite.png");
                    break;
            }

            return buttonsList;
        }
    }
}
