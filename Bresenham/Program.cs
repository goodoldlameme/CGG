using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bresenham;

namespace Brezenham
{
    //TODO крышечки на отрицательных
    //TODO когда на маленьком рисунке из-за сдвига ничо не видно
    //TODO вторую половину
    public class MyForm : Form
    {
        const double a = 20;
        const double b = 1;
        const double c = 10;    
        const double d = 0;
        public IBresenhamDrawer DrawingAlgorithm;
        // рисуем на экране
        private void PutPixel(Graphics graphicsItem, int x, int y, Brush brush)
        {
            graphicsItem.FillRectangle(brush, x, y, 1, 1);    
        }
        // рисуем матрицу
        private void DrawMatrix(Graphics graphicsItem, bool[,] matrix)
        {
            for (int i = 0; i < matrix.GetLength(0); i++)
            for (int j = 0; j < matrix.GetLength(1); j++)
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
            DrawAxis(graphicsItem);
            var resultMatrix = DrawingAlgorithm.DrawLine(a, b, c, 0, formHeight, formWidth);
            DrawMatrix(graphicsItem, resultMatrix);
        }

        private void DrawAxis(Graphics graphics)
        {
            var height = ClientSize.Height;
            var width = ClientSize.Width;
            var pixPerA = (int)Math.Floor((double)height / 2);
            var xStart = (int)Math.Round((double) width / 2);
            var pixPerUnit = Math.Abs(a) < 1 && Math.Abs(a) > 0 ? pixPerA : Math.Abs((int)Math.Truncate(pixPerA / a));
            var yOffset = pixPerA + (int) (d * pixPerUnit);
            if (yOffset > 0 && yOffset < height)
                graphics.DrawLine(Pens.Black, 0, yOffset, width - 1, yOffset);
            graphics.DrawLine(Pens.Black, xStart, 0, xStart, height - 1);
            DrawNoches(graphics, yOffset - 2, xStart - 2, pixPerUnit);
        }

        private void DrawNoches(Graphics graphics, int yStart, int xStart, int unitStep)
        {
            var height = ClientSize.Height;
            var width = ClientSize.Width;
            var pixPerA = (int)Math.Floor((double)height / 2);
            
            for (var i = xStart + 2; i < width; i+=unitStep)
            {
                graphics.DrawString($"{(i - xStart - 2) / unitStep}", new Font(FontFamily.GenericSerif, 10f), Brushes.Black, i, yStart);
                graphics.DrawLine(Pens.Black, i, yStart, i, yStart + 4);   
                graphics.DrawString($"{-(i - xStart - 2) / unitStep}", new Font(FontFamily.GenericSerif, 10f), Brushes.Black, width - i, yStart);
                graphics.DrawLine(Pens.Black, width - i, yStart, width - i, yStart + 4);                      
            }
            
            for (var i = pixPerA + (int)d*unitStep; i < height; i+=unitStep)
            {
                if ((pixPerA - i)  / unitStep + d != 0)
                    graphics.DrawString($"{(pixPerA - i) / unitStep + d}", new Font(FontFamily.GenericSerif, 10f), Brushes.Black, xStart, i);
                graphics.DrawLine(Pens.Black, xStart, i, xStart + 4, i);                    
            }
            
            for (var i = pixPerA + (int)d*unitStep; i > 0; i-=unitStep)
            {
                if ((pixPerA - i)  / unitStep + d != 0)
                    graphics.DrawString($"{(pixPerA - i) / unitStep + d}", new Font(FontFamily.GenericSerif, 10f), Brushes.Black, xStart, i);
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
