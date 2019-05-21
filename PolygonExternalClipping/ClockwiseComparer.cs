using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace PolygonExternalClipping
{
    public class ClockwiseComparer : IComparer<Vector2>
    {
        public enum Direction
        {
            up,
            down
        }

        private Direction direction;
        public ClockwiseComparer(Direction direction)
        {
            this.direction = direction;
        }


        public int Compare(Vector2 p1, Vector2 p2)
        {
            if (p2.X == p1.X)
            {
                return direction == Direction.down ? p1.Y.CompareTo(p2.Y) : (-p1.Y).CompareTo(-p2.Y);
            }

            var val = (p2.Y - p1.Y) / (p2.X - p1.X);
            var atan = Math.Atan(val);
            if (atan > 0)
            {
                return direction == Direction.down ? p1.X.CompareTo(p2.X) : (-p1.X).CompareTo(-p2.X);
            }

            return direction == Direction.up ? p1.X.CompareTo(p2.X) : (-p1.X).CompareTo(-p2.X);
            

        }
    }
}