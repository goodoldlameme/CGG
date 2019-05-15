using System;
using System.Drawing;
using System.Windows.Forms;
using Bresenham;

namespace Brezenham
{
    public class MyForm : Form
    {
        const double a = -100;
        const double b = 0.03;
        const double c = 0;    
        const double d = 10;

        private GraphicParams Params { get; set; }
        private IBresenhamDrawer DrawingAlgorithm { get; set; }

        private void PutPixel(Graphics graphicsItem, int x, int y, Brush brush)
        {
            graphicsItem.FillRectangle(brush, x, y, 1, 1);    
        }

        private void DrawMatrix(Graphics graphicsItem, bool[,] matrix)
        {
            for (var i = 0; i < matrix.GetLength(0); i++)
            for (var j = 0; j < matrix.GetLength(1); j++)
            {
                if (matrix[i, j])
                    PutPixel(graphicsItem, j, i, Brushes.Red);
            }
        }

        private void RedrawGraphics()
        {
            var formHeight = ClientSize.Height;
            var formWidth = ClientSize.Width;
            var graphicsItem = CreateGraphics();
            graphicsItem.Clear(Color.White);
            Params = new GraphicParams(a, b, c, d, formHeight, formWidth);
            DrawAxis(graphicsItem);
            var resultMatrix = DrawingAlgorithm.DrawLine(Params);
           DrawMatrix(graphicsItem, resultMatrix);
        }

        private void DrawAxis(Graphics graphics)
        {
            if (Params.YOffset > 0 && Params.YOffset < Params.Height)
                graphics.DrawLine(Pens.Black, 0, Params.YOffset, Params.Width - 1, Params.YOffset);
            graphics.DrawLine(Pens.Black, Params.XMiddle, 0, Params.XMiddle, Params.Height - 1);
  //          DrawNoches(graphics);
        }

        private void DrawNoches(Graphics graphics)
        {
            var yStart = Params.YOffset - 2;
            var font = new Font(FontFamily.GenericSerif, 10f);
            for (var i = Params.XMiddle; i < Params.Width; i+=Params.PixPerUnit)
            {
                var value = (i - Params.XMiddle) / Params.PixPerUnit;
                graphics.DrawString($"{value}", font, Brushes.Black, i, yStart);
                graphics.DrawLine(Pens.Black, i, yStart, i, yStart + 4);
                if (value != 0)
                {
                    graphics.DrawString($"{-value}", font, Brushes.Black, Params.Width - i, yStart);
                    graphics.DrawLine(Pens.Black, Params.Width - i, yStart, Params.Width - i, yStart + 4);
                }
            }

            var xStart = Params.XMiddle - 2;
            for (var i = Params.YOffset; i < Params.Height; i+=Params.PixPerUnit)
            {
                var value = (Params.PixPerYHalf - i) / Params.PixPerUnit + d;
                if (value != 0)
                    graphics.DrawString($"{value}", font, Brushes.Black, xStart, i);
                graphics.DrawLine(Pens.Black, xStart, i, xStart + 4, i);                    
            }
            
            for (var i = Params.YOffset; i > 0; i-=Params.PixPerUnit)
            {
                var value = (Params.PixPerYHalf - i) / Params.PixPerUnit + d;
                if (value != 0)
                    graphics.DrawString($"{value}", font, Brushes.Black, xStart, i);
                graphics.DrawLine(Pens.Black, xStart, i, xStart + 4, i);                    
            }
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
                    DrawingAlgorithm = new CosineDrawer()
                });
        }
    }
}
