using Bresenham;


namespace Brezenham
{
    public class CosineDrawer : IBresenhamDrawer
    {
        private static bool[,] GetEmptyMatrix(int height, int width)
        {
            var matrix = new bool[height, width];
            for (var i = 0; i < height; i++)
                for (var j = 0; j < width; j++)
                    matrix[i, j] = false;
            return matrix;
        }

        
        private static void DrawCosineLine(bool[,] resultMatrix, GraphicParams param, int dx)
        {
            var metric = param.GetMetric(dx < 1);
            var pixX = param.XStart;
            var stepDirection = param.Direction;
            var xEnd = dx > 0 ? param.Width : 0;
            var yEnd = 2*param.PixPerYHalf - (param.YStart - param.Direction);
            
            for (var pixY = param.YStart; dx*pixX < xEnd; pixY += stepDirection)
            {

                if (pixY == yEnd)
                {
                    stepDirection = -stepDirection;
                    yEnd = param.Direction*stepDirection > 0 ? 2*param.PixPerYHalf - (param.YStart - param.Direction) : param.YStart - param.Direction;
                    continue;
                }

                while(dx*pixX < xEnd)
                {  
                    resultMatrix[pixY, pixX] = true;
                    var x = (double)(pixX - param.XStart) / param.PixPerUnit - param.XOffset;
                    var nextX = (double)(pixX + dx - param.XStart) / param.PixPerUnit - param.XOffset;
                    var y = (double)(param.PixPerYHalf - pixY) / param.PixPerUnit;
                    var nextY = (double)(param.PixPerYHalf - pixY - stepDirection) / param.PixPerUnit;
                    var diagonal = metric(nextX, nextY);
                    var horizontal = metric(nextX, y);
                    var vertical = metric(x, nextY);
                    if (horizontal < vertical)
                    {
                        pixX+=dx;
                        if (diagonal < horizontal)
                            break;
                    }
                    else
                    {
                        if (diagonal < vertical)
                            pixX+=dx;
                        break;
                    }
                }
            }
            
        }

        public bool[,] DrawLine(GraphicParams graphicParams)
        {
            var resultMatrix = GetEmptyMatrix(graphicParams.Height, graphicParams.Width);
            DrawCosineLine(resultMatrix, graphicParams, 1);
            DrawCosineLine(resultMatrix, graphicParams, -1);
            return resultMatrix;
        }
    }
}
