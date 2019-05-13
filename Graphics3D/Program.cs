using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FunctionGraph3D
{
    public class MyForm : Form
    {
        Brush[] defaultBrushes = new Brush[] { Brushes.White, Brushes.Silver, Brushes.SeaGreen, Brushes.Blue};
        Func<double, double, double> F = (x, y) => Math.Cos(x*y);
        Func<double, double, double, double> XScreenCoordinate = (x, y, z) => (y - x) * Math.Sqrt(3) / 2;
        Func<double, double, double, double> YScreenCoordinate = (x, y, z) => (x + y) / 2 - z;

        //Func<double, double, double, double> XScreenCoordinate = (x, y, z) => -x / (2 * Math.Sqrt(2)) + y;
        //Func<double, double, double, double> YScreenCoordinate = (x, y, z) => x / (2 * Math.Sqrt(2)) - z;
        const double MinX = -3;
        const double MaxX = 3; 
        const double MinY = -3;
        const double MaxY = 3;
        public IGraphicsDrawer GraphicsDrawer;
        private void PutPixel(Graphics g, int x, int y, int col)
        {
            if (col > 0)
                g.FillRectangle(defaultBrushes[col], x, y, 1, 1);
        }
        private void DrawMatrix(int[,] matrix)
        {
            var g = CreateGraphics();
            g.Clear(Color.White);
            var height = matrix.GetLength(0);
            var width = matrix.GetLength(1);
            for (var i = 0; i < height; i++)
                for (var j = 0; j < width; j++)
                    PutPixel(g, j, i, matrix[i, j]);
        }
        private void RedrawGraphics()
        {
            var parameters = new Params
            {
                MaxX = MaxX,
                MaxY = MaxY,
                MinX = MinX,
                MinY = MinY,
                Function = F,
                XScreenCoordinate = XScreenCoordinate,
                YScreenCoordinate = YScreenCoordinate,
                Height = ClientSize.Height,
                Width = ClientSize.Width,
                n = 100,
                m = ClientSize.Width * 2
            };
            var matrix = GraphicsDrawer.DrawGraph(parameters);
            DrawMatrix(matrix);
        }

        protected override void OnResizeEnd(EventArgs e)
        {
            RedrawGraphics();
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            RedrawGraphics();
        }
        public static void Main()
        {
            Application.Run(
                new MyForm
                {
                    ClientSize = new Size(1000, 600),
                    GraphicsDrawer = new GraphicsDrawer()
                });
        }
    }
}
