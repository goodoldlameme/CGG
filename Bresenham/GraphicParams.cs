using System;
using MathNet.Numerics;

namespace Bresenham
{
    public class GraphicParams
    {
        public int YStart { get;}
        public double XOffset { get; }
        public int PixPerUnit { get; }
        public int PixPerYHalf { get; }
        public int XStart { get; }
        public int Direction { get; }
        public int YOffset { get; }
        public int Height { get; }
        public int Width { get; }
        public int XMiddle { get; }
        private double StretchY;
        private double StretchX;
        private double XShift;
        private double YShift;
        private double b;

        public GraphicParams(double a, double b, double c, double d, int height, int width)
        {
            StretchY = a;
            StretchX = Math.Abs(b);
            XShift = c;
            YShift = 0;
            Height = height;
            Width = width;
            XOffset = c / b % (2 * Math.PI);
            PixPerYHalf = (int)Math.Floor((double)(Height - 1) / 2);
            XMiddle = (int)Math.Floor((double)(Width - 1) / 2);
            PixPerUnit = GetPixPerUnit();
            YStart = PixPerYHalf - (int) (StretchY * PixPerUnit);
            XStart = GetXStart();
            Direction = Math.Sign(a);
            YOffset = PixPerYHalf + (int) (d * PixPerUnit);
            this.b = b;
        }

        public Func<double, double, double> GetMetric(bool isReverse)
        {
            return
                (x, y) =>
                {
                    y = Math.Abs(y) > Math.Abs(StretchY) ? Math.Sign(y) * Math.Abs(StretchY) : y;
                    var offX = x + XOffset;
                    var offC = XOffset != XShift / b ? XOffset : XShift;
                    int sign; 
                    if (isReverse)
                        sign = Math.Abs(offX % (-2 * Math.PI / StretchX)) < Math.PI / StretchX ? -1 : 1;
                    else 
                        sign = offX % (2 * Math.PI / StretchX) < Math.PI / StretchX ? 1 : -1;
                    
                    var n = Math.Round(offX * StretchX / (2 * Math.PI));
                    Func<double, double> func = u => StretchY * Math.Cos(StretchX * u + offC) + YShift;
                    var bound = (sign * Math.Acos((y - YShift) / StretchY) - offC + 2 * Math.PI * n) / StretchX;
                    var minimaX = x == bound ? x : FindMinimum.OfScalarFunctionConstrained(
                        u => (u - x) * (u - x) + (func(u) - y) * (func(u) - y),
                        Math.Min(x, bound), Math.Max(x, bound));
                    return Math.Abs((minimaX - x) * (minimaX - x) + (func(minimaX) - y) * (func(minimaX) - y));
                };
        }

        private int GetPixPerUnit()
        {
            if (Math.Abs(XOffset) > Math.Abs(StretchY))
            {
                return Math.Abs((int) Math.Truncate(PixPerYHalf / XOffset));
            }

            return 
                Math.Abs(StretchY) < 1 && Math.Abs(StretchY) > 0 
                    ? PixPerYHalf 
                    : Math.Abs((int)Math.Truncate(PixPerYHalf / StretchY));
        }

        private int GetXStart()
        {
            var xStart = (int) (XMiddle - XOffset * PixPerUnit);
            if (xStart < 0)
            {
                xStart += (int)(2 * Math.PI/StretchX * PixPerUnit);
            }else if (xStart > Width)
            {
                xStart -= (int)(2 * Math.PI/StretchX * PixPerUnit);
            }

            return xStart;
        }
    }
}