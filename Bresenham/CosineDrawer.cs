using System;
using System.Drawing;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using MathNet.Numerics;
using Bresenham;


namespace Brezenham
{
    public class CosineDrawer : IBresenhamDrawer
    {
        private bool[,] GetEmptyMatrix(int height, int width)
        {
            bool[,] matrix = new bool[height, width];
            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                    matrix[i, j] = false;
            return matrix;
        }

        private void DrawCosineLine(bool[,] resultMatrix, int yStart, double xEnd, double offset, int pixPerUnit, int pixPerA, int xStart, Func<double, double, double> metric)
        {
            var width = resultMatrix.GetLength(1);
            var height = resultMatrix.GetLength(0);
            var pixX = xStart;
            var xPixEnd = xStart + pixPerUnit * xEnd;
//            for (var pixY = yStart; pixY <= pixPerA; pixY++)
            for (var pixY = yStart; pixY < height - yStart; pixY++)
            {
                while(pixX < 2*xPixEnd && pixX < width)
                //while(pixX < xPixEnd && pixX < width)
                {  
                    resultMatrix[pixY, pixX] = true;
                    var x = (double)(pixX - xStart) / pixPerUnit - offset;
                    var nextX = (double)(pixX + 1 - xStart) / pixPerUnit - offset;
                    var y = (double)(pixPerA - pixY) / pixPerUnit;
                    var nextY = (double)(pixPerA - pixY - 1) / pixPerUnit;
                    var diagonal = metric(nextX, nextY);
                    var horizontal = metric(nextX, y);
                    var vertical = metric(x, nextY);
                    if (horizontal < vertical)
                    {
                        pixX++;
                        if (diagonal < horizontal)
                            break;
                    }
                    else
                    {
                        if (diagonal < vertical)
                            pixX++;
                        break;
                    }
                }
            }
        }

        public bool[,] DrawLine(double a, double b, double c, double d, int height, int width)
        {
            var resultMatrix = GetEmptyMatrix(height, width);
            var pixPerA = (int)Math.Floor((double)height / 2);
            var pixPerUnit = a < 1 ? pixPerA : (int)Math.Floor(pixPerA / a);
            var offset = c / b % (2 * Math.PI);
            var xMiddle = Math.Round((double) width / 2);
            var xEnd = Math.PI / (2 * Math.Abs(b));
            var xStart = (int) (xMiddle - offset * pixPerUnit);
            if (xStart < 0)
            {
                xStart += (int)(2 * Math.PI/Math.Abs(b) * pixPerUnit);
            }else if (xStart > width)
            {
                xStart -= (int)(2 * Math.PI/Math.Abs(b) * pixPerUnit);
            }

            Func<double, double, double> metric = (x, y) =>
            {
                Func<double, double> func = u => a * Math.Cos(b * u + c) + d;
                var bound = (Math.Acos((y - d) / a) - c) / b;
                var minimaX = FindMinimum.OfScalarFunctionConstrained(
                    u => (u - x) * (u - x) + (func(u) - y) * (func(u) - y),
                    Math.Min(x, bound), Math.Max(x, bound));
                return Math.Abs((minimaX - x) * (minimaX - x) + (func(minimaX) - y) * (func(minimaX) - y));
            };

            DrawCosineLine(
                resultMatrix, 
                a > 1 ? 0 : pixPerA - (int)(a * pixPerUnit), 
                xEnd, 
                offset, 
                pixPerUnit, 
                pixPerA, 
                xStart, 
                metric);
            return resultMatrix;
        }
    }
}
