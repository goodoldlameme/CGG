using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;

namespace PolygonExternalClipping
{
    public class MyForm : Form
    {
        private CircularLinkedList<Vector2> subject = new CircularLinkedList<Vector2>();
        private CircularLinkedList<Vector2> clip = new CircularLinkedList<Vector2>();
        private float Kx;
        private float Ky;
        private float Xmin;
        private float Ymin;
        private float XMax;
        private float YMax;
        
        
        public IPolyginClippingAlgorithm ClippingAlgorithm { get; set; }
        public MyForm()
        {
            
//            subject.AddLast(new Vector2(0, 1));
//            subject.AddLast(new Vector2(1, 4));
//            subject.AddLast(new Vector2(5, 4));
//            clip.AddLast(new Vector2(1, 2));
//            clip.AddLast(new Vector2(3, 4));
//            clip.AddLast(new Vector2(3, 2));

//            subject.AddLast(new Vector2(1, 0));
//            subject.AddLast(new Vector2(1, 4));
//            subject.AddLast(new Vector2(5, 4));
//            clip.AddLast(new Vector2(1, 2));
//            clip.AddLast(new Vector2(3, 4));
//            clip.AddLast(new Vector2(3, 2));
            
//            subject.AddLast(new Vector2(1, 1));
//            subject.AddLast(new Vector2(1, 4));
//            subject.AddLast(new Vector2(4, 4));
//            subject.AddLast(new Vector2(4, 1));
//            clip.AddLast(new Vector2(2, 4));
//            clip.AddLast(new Vector2(5, 2));
//            clip.AddLast(new Vector2(2, 2));
            
//            subject.AddLast(new Vector2(1, 1));
//            subject.AddLast(new Vector2(1, 5));
//            subject.AddLast(new Vector2(5, 5));
//            subject.AddLast(new Vector2(5, 1));
//            clip.AddLast(new Vector2(1, 1));
//            clip.AddLast(new Vector2(2, 2));
//            clip.AddLast(new Vector2(3, 4));

//            subject.AddLast(new Vector2(1, 1));
//            subject.AddLast(new Vector2(1, 5));
//            subject.AddLast(new Vector2(5, 5));
//            subject.AddLast(new Vector2(5, 1));
//            clip.AddLast(new Vector2(2, 1));
//            clip.AddLast(new Vector2(2, 2));
//            clip.AddLast(new Vector2(5, 2));
//            clip.AddLast(new Vector2(5, 1));
            
//            subject.AddLast(new Vector2(1, 1));
//            subject.AddLast(new Vector2(2, 4));
//            subject.AddLast(new Vector2(5, 5));
//            subject.AddLast(new Vector2(7, 3));
//            subject.AddLast(new Vector2(3, 0));
//            clip.AddLast(new Vector2(2, 0));
//            clip.AddLast(new Vector2(1, 3));
//            clip.AddLast(new Vector2(7, 4));
//            clip.AddLast(new Vector2(5, 1));
            
            
//            clip.AddLast(new Vector2(2, 3));
//            clip.AddLast(new Vector2(2, 5));
//            clip.AddLast(new Vector2(5, 5));
//            clip.AddLast(new Vector2(5, 3));
//            subject.AddLast(new Vector2(1, 0));
//            subject.AddLast(new Vector2(1, 4));
//            subject.AddLast(new Vector2(6, 4));
//            subject.AddLast(new Vector2(6, 0));
            
            Xmin = subject.Union(clip).Select(x => x.X).Min() -1;
            Ymin = subject.Union(clip).Select(x => x.Y).Min() - 1;
            XMax = subject.Union(clip).Select(x => x.X).Max() + 1;
            YMax = subject.Union(clip).Select(x => x.Y).Max() + 1;

        }
        
        private bool IsInsideWindow(Vector2 point, CircularLinkedList<Vector2> list)
        {
            var intersectionsNum = 0;
            var prev = list.Last;
            var cur = list.First;
            var prevUnder = prev.Value.Y < point.Y;

            for (var i = 0; i < list.Count; ++i)
            {
                var curUnder = cur.Value.Y < point.Y;

                var a = prev.Value - point;
                var b = cur.Value - point;

                var t = (a.X*(b.Y - a.Y) - a.Y*(b.X - a.X));
                if (curUnder && !prevUnder)
                {
                    if (t > 0)
                        intersectionsNum += 1;
                }
                if (!curUnder && prevUnder)
                {
                    if (t < 0)
                        intersectionsNum += 1;
                }

                prev = cur;
                cur = list.NextOrFirst(cur);
                prevUnder = curUnder;        
            }

            return (intersectionsNum&1) != 0;
        }
        
        private bool IsBelongsLine(Vector2 point1, Vector2 point2, Vector2 pointToCheck) =>
            (point1.Y - point2.Y) * pointToCheck.X + (point2.X - point1.X) * pointToCheck.Y == point2.X * point1.Y - point1.X * point2.Y ;

        
        private bool IsOnWindow(CircularLinkedList<Vector2> list, Vector2 point)
        {
            var cur = list.First;
            for (var i = 0; i < list.Count; i++)
            {
                var prev = cur;
                cur = list.NextOrFirst(cur);
                if (IsBelongsLine(prev.Value, cur.Value, point))
                    return true;
            }
            return false;
        }


        private void Draw(CircularLinkedList<Vector2> subject, CircularLinkedList<Vector2> clip)
        {
            var graphics = CreateGraphics();
            graphics.Clear(Color.White);

            var resultPolygons = ClippingAlgorithm.Process(subject, clip);
            if (resultPolygons.Count == 0)
            {
                if (subject.Any(p => !IsInsideWindow(p, clip) && !IsOnWindow(clip, p))) 
                {
                    if (clip.Any(p => !IsInsideWindow(p, subject) && !IsOnWindow(subject, p)))
                    {
                        graphics.FillPolygon(Brushes.Gold, GetPointArray(subject));
                        graphics.DrawPolygon(Pens.Black, GetPointArray(subject));
                    }
                    else
                    {
                        graphics.FillPolygon(Brushes.Gold, GetPointArray(subject));
                        graphics.FillPolygon(Brushes.White, GetPointArray(clip));
                        graphics.DrawPolygon(Pens.Black, GetPointArray(subject));
                        graphics.DrawPolygon(Pens.Black, GetPointArray(clip));
                    }
                }
                
            }

            foreach (var polygon in resultPolygons)
            {
                graphics.FillPolygon(Brushes.Gold, GetPointArray(polygon));
                graphics.DrawPolygon(Pens.Black, GetPointArray(polygon));
            }
        }

        private PointF[] GetPointArray(CircularLinkedList<Vector2> list)
        {
            var currentNode = list.First;
            var result = new PointF[list.Count];
            for (var i = 0; i < list.Count; i++)
            {
                result[i] = ConvertToScreenCoord(currentNode.Value.X, currentNode.Value.Y);
                currentNode = list.NextOrFirst(currentNode);
            }

            return result;
        }

        private PointF ConvertToScreenCoord(float x, float y)
        {
            var height = ClientSize.Height;
            var width = ClientSize.Width;
            
            var coordHorDelta = XMax - Xmin;
            var coordVertDelta = YMax - Ymin;
            Kx = width / coordHorDelta;
            Ky = height / coordVertDelta;
            var screenX = Kx*(x - Xmin);
            var screenY = height - Ky*(y - Ymin);
            return new PointF(screenX, screenY);
        }

        protected override void OnResizeEnd(EventArgs e)
        {
            Draw(subject, clip);
        }
        
        protected override void OnPaint(PaintEventArgs e)
        {
            Draw(subject, clip);
        }
        
        public static void Main(string[] args)
        {
            Application.Run(new MyForm{ClientSize = new Size(1000, 600), ClippingAlgorithm = new WeilerAtherton()});
        }
    }
}