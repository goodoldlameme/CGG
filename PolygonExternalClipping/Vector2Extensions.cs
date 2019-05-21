using System.Numerics;

namespace PolygonExternalClipping
{
    public static class Vector2Extensions
    {
        public static bool IsCollinear(this Vector2 vector1, Vector2 vector2)
        {
            return vector1.X == 0 && vector2.X == 0 || vector1.Y == 0 && vector2.Y == 0 ||
                   vector1.X != 0 && vector1.Y != 0 && vector2.X / vector1.X == vector2.Y / vector1.Y;
        }
    }
}