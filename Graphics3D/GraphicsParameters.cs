using System;

namespace FunctionGraph3D
{
    public class Params
    {
        public double MinX, MinY, MaxX, MaxY;
        public Func<double, double, double> Function;
        public Func<double, double, double, double> XScreenCoordinate;
        public Func<double, double, double, double> YScreenCoordinate;
        public int Height, Width;
        public int n, m;
    }
}
