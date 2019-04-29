namespace Bresenham
{
    public interface IBresenhamDrawer
    {
        bool[,] DrawLine(double a, double b, double c, double d, int width, int height);
    }
}