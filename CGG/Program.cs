using System;
using System.Drawing;
using System.Windows.Forms;

namespace CGG
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            const double leftBorder = 1.0;
            const double rightBorder = 6.0;
            double Function(double x) => Math.Sin(10*x)*x;
//            double Function(double x) => Math.Pow(x, 2) - 1;
            Application.Run(new GraphicsForm(leftBorder, rightBorder, Function){ClientSize = new Size(800,600)});
        }
    }
}