using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace CGG
{
    public class GraphicParams
    {
        private double YMin;
        private double YMax;
        public int XAxis { get; private set; }
        public int YAxis { get; private set; }
        private double LeftBorder { get; }
        private double RightBorder { get; }
        private readonly Func<double, double> Function;
        public Size Resolution { get; set; }
        public bool ShouldDrawXAxis => LeftBorder < 0 && RightBorder > 0;
        public bool ShouldDrawYAxis => YMax > 0 && YMin < 0;
        
        public GraphicParams(double leftBorder, double rightBorder, 
            Func<double, double> function, Size resolution)
        {
            LeftBorder = leftBorder;
            RightBorder = rightBorder;
            Function = function;
            Resolution = resolution;
        }


        public void CalcExtremum()
        {
            YMax = double.NegativeInfinity;
            YMin = double.PositiveInfinity;
            
            for (var xx = 0; xx < Resolution.Width; xx++)
            {
                var x = CalcMachineCoord(xx);
                var y = Function.Invoke(x);
                YMin = (y is double.NaN) ? YMin : Math.Min(YMin, y);
                YMax = (y is double.NaN) ? YMax : Math.Max(YMax, y);
            }
        }
        
        public void CalcAxisPosition()
        {
            var xAxisPossiblePos = double.PositiveInfinity;
            var yAxisPossiblePos = double.PositiveInfinity;
            
            XAxis = -1;
            YAxis = -1;            
            
            for (var xx = 0; xx < Resolution.Width; xx++)
            {
                var x = CalcMachineCoord(xx);
                var y = Function.Invoke(x);


                if (Math.Abs(x) < xAxisPossiblePos)
                {
                    xAxisPossiblePos = Math.Abs(x);
                    XAxis = xx;
                }

                if (Math.Abs(y) < yAxisPossiblePos)
                {
                    yAxisPossiblePos = Math.Abs(y);
                    YAxis = (int)CalcScreenCoord(x);
                }
            }
        }

        private double CalcScreenCoord(double x)
        {
            var y = Function.Invoke(x);
            if (y is double.NaN)
                return double.NaN;
            return (int)((y - YMax) * (Resolution.Height - 1)/
                                   (YMin - YMax));
            
        }

        private double CalcMachineCoord(double x)
        {
            return LeftBorder+x*(RightBorder-LeftBorder)/(Resolution.Width - 1);
        }

        public IEnumerable<((double, double), (double, double))> GetCoordinates()
        {
            var from = (0, CalcScreenCoord(LeftBorder));
            
            for (var xx = 0; xx < Resolution.Width; xx++)
            {
                var x = CalcMachineCoord(xx);
                var yy = CalcScreenCoord(x);
                yield return (from, (xx, yy));
                from = (xx, yy);
            }
        }
    }
}