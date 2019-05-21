using System.Collections.Generic;

namespace PolygonExternalClipping
{
    public class CircularLinkedList<T> : LinkedList<T>
    {
        public LinkedListNode<T> NextOrFirst(LinkedListNode<T> current)
        {
            return current.Next ?? current.List.First;
        }

        public LinkedListNode<T> PreviousOrLast(LinkedListNode<T> current)
        {    
            return current.Previous ?? current.List.Last;
        }

        public CircularLinkedList<T> Reverse()
        {
            var temp = new CircularLinkedList<T>();
            foreach (var current in this)
                temp.AddFirst(current);

            return temp;
        }
        
        public static explicit operator CircularLinkedList<T>(List<T> list)
        {
            var result = new CircularLinkedList<T>();
            foreach (var e in list)
            {
                result.AddLast(e);
            }
            
            return result;
        }
    }
}