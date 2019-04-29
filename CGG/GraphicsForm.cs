using System;
using System.Drawing;
using System.Windows.Forms;

namespace CGG
{
    public class GraphicsForm : Form
    {
        private GraphicParams GraphicParams { get; }

        public GraphicsForm(double leftBorder, double rightBorder,
            Func<double, double> function)
        {
            DoubleBuffered = true;
            GraphicParams = new GraphicParams(leftBorder, rightBorder, function, ClientSize);
            SizeChanged += ResizeGraph;
            
            GraphicParams.CalcExtremum();
            GraphicParams.CalcAxisPosition();
        }
        
        private void ResizeGraph(object sender, EventArgs args)
        {
            GraphicParams.Resolution = ClientSize;
            GraphicParams.CalcExtremum();
            GraphicParams.CalcAxisPosition();
            
            Invalidate();
        }

        private void DrawAxis(Graphics g)
        {
            if (GraphicParams.ShouldDrawXAxis)
                g.DrawLine(Pens.Black, GraphicParams.XAxis, 0, GraphicParams.XAxis, GraphicParams.Resolution.Height - 1);
            if (GraphicParams.ShouldDrawYAxis)
                g.DrawLine(Pens.Black, 0, GraphicParams.YAxis, ClientSize.Width, GraphicParams.YAxis);
        }

        private void DrawGraphic(Graphics g)
        {
            DrawAxis(g);
            foreach (var points in GraphicParams.GetCoordinates())
            {
                if (!(points.Item1.Item2 is double.NaN || points.Item2.Item2 is double.NaN))
                {
                    var point1 = new Point((int) points.Item1.Item1, (int) points.Item1.Item2);
                    var point2 = new Point((int) points.Item2.Item1, (int) points.Item2.Item2);
                    g.DrawLine(Pens.DimGray, point1, point2);
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            DrawGraphic(g);
        }
    }
}