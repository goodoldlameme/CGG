using System;
using System.Collections.Generic;
using System.Linq;

namespace FunctionGraph3D
{
    public class GraphicsDrawer : IGraphicsDrawer
    {
        private const int TopColor = 1;
        private const int BottomColor = 2;
        private class PointDouble
        {
            public double X, Y;
            public PointDouble() { }
            public PointDouble(double x, double y, double z, Params parameters)
            {
                X = parameters.XScreenCoordinate(x, y, z);
                Y = parameters.YScreenCoordinate(x, y, z);
            }
        }
        private class PointInt
        {
            public int X, Y;
            public PointInt() { }
            public PointInt(PointDouble p)
            {
                X = (int)Math.Round(p.X);
                Y = (int)Math.Round(p.Y);
            }
            public PointInt(double x, double y, double z, Params parameters)
                : this(new PointDouble(x, y, z, parameters)) { }
        }
        private static void PutPixel(int[, ] matrix, int x, int y, int color)
        {
            matrix[y, x] = color;
        }
        private static void PutPixel(int[, ] matrix, PointInt point, int color)
        {
            PutPixel(matrix, point.X, point.Y, color);
        }
        private void PutPixel(int[, ] matrix, PointDouble point, int color)
        {
            PutPixel(matrix, new PointInt(point), color);
        }
        private class ProectionBounds
        {
            public double MinX = double.PositiveInfinity;
            public double MaxX = double.NegativeInfinity;
            public double MinY = double.PositiveInfinity;
            public double MaxY = double.NegativeInfinity;
        }
        private double GetValue(double minBound, double maxBound, int iteration, int partsCount)
        {
            return minBound + iteration * (maxBound - minBound) / partsCount;
        }
        private ProectionBounds GetBounds(Params parameters, Func<int, int, double> GetX, Func<int, int, double> GetY)
        {
            var bounds = new ProectionBounds();
            for (var i = 0; i < parameters.n; i++)
            {
                for (var j = 0; j < parameters.m; j++)
                {
                    var x = GetX(i, j);
                    var y = GetY(i, j);
                    var z = parameters.Function(x, y);
                    var currentPoint = new PointDouble(x, y, z, parameters);
                    bounds.MaxX = Math.Max(bounds.MaxX, currentPoint.X);
                    bounds.MaxY = Math.Max(bounds.MaxY, currentPoint.Y);
                    bounds.MinX = Math.Min(bounds.MinX, currentPoint.X);
                    bounds.MinY = Math.Min(bounds.MinY, currentPoint.Y);
                }
            }
            return bounds;
        }
        private void FillHorizonHeights(int[] topHorizonHeight, int[] bottomHorizonHeight, Params parameters)
        {
            for (var i = 0; i < parameters.Width; i++)
            {
                topHorizonHeight[i] = parameters.Height;
                bottomHorizonHeight[i] = 0;
            }
        }
        private bool VisibleBottom(PointInt point, int[] bottomHorizonHeight)
        {
            return point.Y > bottomHorizonHeight[point.X];
        }
        private bool VisibleTop(PointInt point, int[] topHorizonHeight)
        {
            return point.Y < topHorizonHeight[point.X];
        }
        private List<PointInt> GetNewLine(int[, ] matrix, Params parameters, int iteration, ProectionBounds bounds, Func<int, int, double> GetX, Func<int, int, double> GetY)
        {
            var result = new List<PointInt>();
            for (var i = 0; i < parameters.m; i++)
            {
                var x = GetX(iteration, i);
                var y = GetY(iteration, i);
                var z = parameters.Function(x, y);
                var currentPointDouble = new PointDouble(x, y, z, parameters);
                var transformed = new PointDouble()
                {
                    X = (currentPointDouble.X - bounds.MinX) / (bounds.MaxX - bounds.MinX) * (parameters.Width - 1),
                    Y = (currentPointDouble.Y - bounds.MinY) / (bounds.MaxY - bounds.MinY) * (parameters.Height - 1)
                };
                result.Add(new PointInt(transformed));
            }
            return result;
        }
        private class ColoredPoint : PointInt
        {
            public int Color;
            public ColoredPoint(int x, int y, int color)
            {
                X = x;
                Y = y;
                Color = color;
            }
        }
        private IEnumerable<PointInt> DrawLineFromZero(PointInt target)
        {
            if (target.X == 0)
            {
                for (var i = 0; i <= target.Y; i++)
                    yield return new PointInt()
                    {
                        X = 0,
                        Y = i
                    };
                yield break;
            }
            var y = 0;
            for (var x = 0; x <= target.X; x++)
            {
                var flag = false;
                while ((y + 1) * target.X <= x * target.Y)
                {
                    flag = true;
                    y++;
                    yield return new PointInt()
                    {
                        X = x,
                        Y = y
                    };
                }
                if (!flag)
                    yield return new PointInt()
                    {
                        X = x,
                        Y = y
                    };
            }
        }
        private IEnumerable<ColoredPoint> BrezenhamLine(PointInt from, PointInt to, int color)
        {
            var x1 = to.X - from.X;
            var y1 = to.Y - from.Y;
            return DrawLineFromZero(new PointInt()
            {
                X = Math.Abs(x1),
                Y = Math.Abs(y1)
            })
                .Select(point =>
                    new ColoredPoint(Math.Sign(x1) * point.X + from.X, Math.Sign(y1) * point.Y + from.Y, color))
                .Skip(1);
        }
        int AddPoint(List<ColoredPoint> result, PointInt point, int color, int prevColor)
        {
            if (prevColor != 0)
                result.AddRange(BrezenhamLine(result[result.Count - 1], point, prevColor));
            else
                result.Add(new ColoredPoint(point.X, point.Y, color));
            return color;
        }
        private List<ColoredPoint> DrawContinuousLine(List<PointInt> sourcePoints, int[] topHorizonHeight, int[] bottomHorizonHeight)
        {
            var result = new List<ColoredPoint>();
            var prevColor = 0;
            foreach (var point in sourcePoints)
            {
                if (VisibleBottom(point, bottomHorizonHeight))
                {
                    prevColor = AddPoint(result, point, BottomColor, prevColor);
                    bottomHorizonHeight[point.X] = point.Y;
                }
                else if (VisibleTop(point, topHorizonHeight))
                {
                    prevColor = AddPoint(result, point, TopColor, prevColor);
                    topHorizonHeight[point.X] = point.Y;
                }
                else
                    prevColor = 0;
            }
            return result;
        }
        private void DrawNextLayer(int[,] matrix, int[] topHorizonHeight, int[] bottomHorizonHeight, 
            ProectionBounds bounds, Params parameters, Func<int, int, double> GetX, Func<int, int, double> GetY, int iteration)
        {
            var points = GetNewLine(matrix, parameters, iteration, bounds, GetX, GetY);
            var resultPoints = DrawContinuousLine(points, topHorizonHeight, bottomHorizonHeight);
            foreach (var point in resultPoints)
                PutPixel(matrix, point, point.Color);
        }
        private void DrawForDirection(int[, ] matrix, Params parameters, Func<int, int, double> GetX, Func<int, int, double> GetY, bool onlyFirstLine = false)
        {
            var topHorizonHeight = new int[parameters.Width];
            var bottomHorizonHeight = new int[parameters.Width];
            var bounds = GetBounds(parameters, GetX, GetY);
            FillHorizonHeights(topHorizonHeight, bottomHorizonHeight, parameters);
            for (var i = 0; i < parameters.n; i++)
            {
                DrawNextLayer(matrix, topHorizonHeight, bottomHorizonHeight, bounds, parameters, GetX, GetY, i);
                if (onlyFirstLine)
                    break;
            }
        }
        private class Dot
        {
            public int Height, Color;
            public Dot(int height, int color)
            {
                Height = height;
                Color = color;
            }
        }
        static int[] dx = new [] { -1, -1, -1, 0, 0, 1, 1, 1 };
        static int[] dy = new[] { -1, 0, 1, 1, -1, -1, 0, 1 };
        private bool IsInside(int r, int c, int n, int m)
        {
            return 0 <= r && r < n && 0 <= c && c < m;
        }
        void search(int[, ] matrix, int row, int col)
        {
            if (matrix[row, col] == 0)
                return;
            var n = matrix.GetLength(0);
            var m = matrix.GetLength(1);
            var neigsFound = 0;
            for (var d = 0; d < 8; d++)
            {
                var newRow = row + dx[d];
                var newCol = col + dy[d];
                if (IsInside(newRow, newCol, n, m) && matrix[newRow, newCol] != 0)
                    neigsFound++;
            }
            if (neigsFound >= 2)
                return;
            matrix[row, col] = 0;
            for (var d = 0; d < 8; d++)
            {
                var newRow = row + dx[d];
                var newCol = col + dy[d];
                if (IsInside(newRow, newCol, n, m) && matrix[newRow, newCol] == matrix[row, col])
                    search(matrix, newRow, newCol);
            }
        }
        private void TwoNeighboursFilter(int[, ] matrix)
        {
            var n = matrix.GetLength(0);
            var m = matrix.GetLength(1);
            for (var i = 0; i < n; i++)
                for (var j = 0; j < m; j++)
                    search(matrix, i, j);
        }
        private void TryWindowFilter(int[, ] matrix, int row, int col, int windowSideLength)
        {
            var n = matrix.GetLength(0);
            var m = matrix.GetLength(1);
            var colorsCount = new Dictionary<int, int>();
            for (var i = 0; i < windowSideLength; i++)
                for(var j = 0; j < windowSideLength; j++)
                {
                    var newRow = row - windowSideLength / 2 + i;
                    var newCol = col - windowSideLength / 2 + j;
                    if (!IsInside(newRow, newCol, n, m))
                        continue;
                    var currentColor = matrix[newRow, newCol];
                    if (colorsCount.ContainsKey(currentColor))
                        colorsCount[currentColor]++;
                    else
                        colorsCount[currentColor] = 1;
                }
            colorsCount.Remove(0);
            var sum = colorsCount.Sum(e => e.Value);
            var self = colorsCount[matrix[row, col]];
            if (self <= sum - self)
                matrix[row, col] = 0;
        }
        private void WindowFilter(int[, ] matrix)
        {
            var n = matrix.GetLength(0);
            var m = matrix.GetLength(1);
            for (var i = 0; i < n; i++)
                for (var j = 0; j < m; j++)
                    if (matrix[i, j] != 0)
                        TryWindowFilter(matrix, i, j, 23);
        }
        private void FirstLineFilter(int[, ] matrix, Params parameters)
        {
            var n = matrix.GetLength(0);
            var m = matrix.GetLength(1);
            var cleanupMatrix = new int[n, m];
            DrawForDirection(cleanupMatrix, parameters,
                (outerIteration, innerIteration) => GetValue(parameters.MaxX, parameters.MinX, outerIteration, parameters.n - 1),
                (outerIteration, innerIteration) => GetValue(parameters.MaxY, parameters.MinY, innerIteration, parameters.m - 1),
                true);

            DrawForDirection(cleanupMatrix, parameters,
                (outerIteration, innerIteration) => GetValue(parameters.MaxX, parameters.MinX, innerIteration, parameters.m - 1),
                (outerIteration, innerIteration) => GetValue(parameters.MaxY, parameters.MinY, outerIteration, parameters.n - 1),
                true);
            for (var i = 0; i < m; i++)
            {
                var pos = 0;
                for (var j = 0; j < n; j++)
                    if (cleanupMatrix[j, i] != 0)
                    {
                        pos = j;
                        break;
                    }
                for (var j = 0; j < n; j++)
                {
                    if (j < pos && matrix[j, i] == BottomColor)
                        matrix[j, i] = 0;
                    if (j > pos && matrix[j, i] == TopColor)
                        matrix[j, i] = 0;
                }
            }
            for (var i = 0; i < n; i++)
                for (var j = 0; j < m; j++)
                    if (cleanupMatrix[i, j] != 0)
                        matrix[i, j] = cleanupMatrix[i, j];
        }
        public void ContinuousFilter(int[, ] matrix)
        {
            var n = matrix.GetLength(0);
            var m = matrix.GetLength(1);
            for (var i = 0; i < n; i++)
                for (var j = 0; j < m; j++)
                {
                    if (matrix[i, j] != 0)
                        continue;
                    var cnt = 0;
                    for (var d = 0; d < 8; d++)
                    {
                        var newRow = i + dx[d];
                        var newCol = j + dy[d];
                        if (IsInside(newRow, newCol, n, m) && matrix[newRow, newCol] == TopColor)
                            cnt++;
                    }
                    if (cnt == 1)
                        matrix[i, j] = TopColor;
                }
            for (var i = 0; i < n; i++)
                for (var j = 0; j < m; j++)
                {
                    if (matrix[i, j] != 0)
                        continue;
                    var cnt = 0;
                    for (var d = 0; d < 8; d++)
                    {
                        var newRow = i + dx[d];
                        var newCol = j + dy[d];
                        if (IsInside(newRow, newCol, n, m) && matrix[newRow, newCol] == BottomColor)
                            cnt++;
                    }
                    if (cnt == 1)
                        matrix[i, j] = BottomColor;
                }
        }
        public void CutTrashDots(int[, ] matrix, Params parameters)
        {
            TwoNeighboursFilter(matrix);
            //WindowFilter(matrix);
            FirstLineFilter(matrix, parameters);
            //ContinuousFilter(matrix);
        }
        public int[,] DrawGraph(Params parameters)
        {
            var matrix = new int[parameters.Height, parameters.Width];
            DrawForDirection(matrix, parameters,
                (outerIteration, innerIteration) => GetValue(parameters.MaxX, parameters.MinX, outerIteration, parameters.n - 1),
                (outerIteration, innerIteration) => GetValue(parameters.MaxY, parameters.MinY, innerIteration, parameters.m - 1)
                );
            
            DrawForDirection(matrix, parameters,
                (outerIteration, innerIteration) => GetValue(parameters.MaxX, parameters.MinX, innerIteration, parameters.m - 1),
                (outerIteration, innerIteration) => GetValue(parameters.MaxY, parameters.MinY, outerIteration, parameters.n - 1)
                );
            CutTrashDots(matrix, parameters);
            
            return matrix;
        }
    }
}
