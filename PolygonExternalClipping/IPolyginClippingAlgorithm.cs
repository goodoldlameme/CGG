using System.Collections.Generic;
using System.Numerics;

namespace PolygonExternalClipping
{
    public interface IPolyginClippingAlgorithm
    {
        ICollection<CircularLinkedList<Vector2>> Process(
            CircularLinkedList<Vector2> subject, CircularLinkedList<Vector2> clip);
    }
}