using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra.Complex32;
using MathNet.Numerics.Random;

namespace PolygonExternalClipping
{

    public enum IntersectionStatus
    {
        Yes,
        No
    }

    public enum EnteringStatus
    {
        In, 
        Out,
        No
    }

    public class WeilerAtherton : IPolyginClippingAlgorithm
    {
        private CircularLinkedList<(Vector2 point, IntersectionStatus intersectionStatus)> IntersectionClip;
        private CircularLinkedList<(Vector2 point, IntersectionStatus intersectionStatus)> IntersectionSubject;
        private CircularLinkedList<Vector2> Subject;
        private CircularLinkedList<Vector2> Clip;
        private CircularLinkedList<(Vector2 point, EnteringStatus enteringStatus)> SubjectMove;
        private CircularLinkedList<(Vector2 point, EnteringStatus enteringStatus)> ClipMove;
        private HashSet<Vector2> VisitedOut;
        
        public ICollection<CircularLinkedList<Vector2>> Process(CircularLinkedList<Vector2> subject, CircularLinkedList<Vector2> clip)
        {
            SubjectMove = new CircularLinkedList<(Vector2 point, EnteringStatus enteringStatus)>();
            ClipMove = new CircularLinkedList<(Vector2 point, EnteringStatus enteringStatus)>();
            Subject = subject;
            Clip = clip;
            IntersectionSubject = GetIntersectionPoints(subject, clip);
            IntersectionClip = GetIntersectionPoints(clip, subject);
            VisitedOut = new HashSet<Vector2>();
            DeleteEdgeSpecialPoints();
            DeleteSpecialVertices(); 
            LabelSubjectPoints();
            LabelClipPoints();
            
            return ClipPolygon();
        }

        private IList<CircularLinkedList<Vector2>> ClipPolygon()
        {
            var outCount = SubjectMove.Count(s => s.enteringStatus == EnteringStatus.Out);
            var result = new List<CircularLinkedList<Vector2>>();
            
            while (VisitedOut.Count != outCount)
            {
                var start = GetStartPoint();
                var cur = start;
                var part = new CircularLinkedList<Vector2>();
                while (cur.Value.enteringStatus != EnteringStatus.In)
                {
                    part.AddLast(cur.Value.point);
                    cur = SubjectMove.NextOrFirst(cur);
                }
                
                var curClip = FindNode(cur.Value.point, cur.Value.enteringStatus);
                while (Math.Abs(curClip.Value.point.X - start.Value.point.X) > 1e-5 || Math.Abs(curClip.Value.point.Y - start.Value.point.Y) > 1e-5)
                {
                    part.AddLast(curClip.Value.point);
                    curClip = ClipMove.NextOrFirst(curClip);
                }
                result.Add(part);
            }

            return result;
        }

        private LinkedListNode<(Vector2 point, EnteringStatus enteringStatus)> GetStartPoint()
        {
            var cur = SubjectMove.First;
            for (var i = 0; i < SubjectMove.Count; i++)
            {
                if (cur.Value.enteringStatus != EnteringStatus.Out || VisitedOut.Contains(cur.Value.point))
                {
                    cur = SubjectMove.NextOrFirst(cur);
                    continue;
                }
                VisitedOut.Add(cur.Value.point);
                return cur;
            }
            return null;
        }

        private CircularLinkedList<(Vector2 point, IntersectionStatus intersectionStatus)> GetIntersectionPoints(CircularLinkedList<Vector2> main, CircularLinkedList<Vector2> side)
        {
            var currentSubject = main.First;
            var currentClip = side.First;
            var resultList = new CircularLinkedList<(Vector2 point, IntersectionStatus intersectionStatus)>();
            for (var i = 0; i < main.Count; i++)
            {
                var prevSubj = currentSubject.Value;
                currentSubject = main.NextOrFirst(currentSubject);
                if (!resultList.Select(s => s.point).Contains(prevSubj))
                    resultList.AddLast((prevSubj, IntersectionStatus.No));
                var temp = new List<Vector2>();
                for (var j = 0; j < side.Count; j++)
                {
                    var prevClip = currentClip.Value;
                    currentClip = side.NextOrFirst(currentClip);
                    if (!GetIntersectionPoint(prevSubj, currentSubject.Value, prevClip, currentClip.Value, out var result) || 
                        resultList.Contains((result, IntersectionStatus.Yes)) || temp.Contains(result)) continue;
                    
                    if (resultList.Contains((result, IntersectionStatus.No)))
                        resultList.Remove((result, IntersectionStatus.No));
                    temp.Add(result);
                }
                
                foreach (var elem in temp.OrderByDescending(s => s, new ClockwiseComparer(GetDirection(prevSubj, currentSubject.Value))))
                    resultList.AddLast((elem, IntersectionStatus.Yes));
            }
            return resultList;
        }

        private ClockwiseComparer.Direction GetDirection(Vector2 prev, Vector2 cur)
        {
            if (prev.Y > cur.Y)
                return ClockwiseComparer.Direction.down;
            if (prev.Y < cur.Y)
                return ClockwiseComparer.Direction.up;
            return prev.X > cur.X ? ClockwiseComparer.Direction.up : ClockwiseComparer.Direction.down;
        }

        private void LabelSubjectPoints()
        {
            var cur = IntersectionSubject.First;
            
            for (var i = 0; i < IntersectionSubject.Count; i++)
            {
                if (cur.Value.intersectionStatus == IntersectionStatus.Yes)
                {
                    var prev = IntersectionSubject.PreviousOrLast(cur).Value;
                    var next = IntersectionSubject.NextOrFirst(cur).Value;
                    if ((IsInsideWindow(prev.point) || prev.intersectionStatus != IntersectionStatus.No) && 
                        (!IsInsideWindow(next.point) || next.intersectionStatus != IntersectionStatus.No))
                        SubjectMove.AddLast((cur.Value.point, EnteringStatus.Out));
                    else
                        SubjectMove.AddLast((cur.Value.point, EnteringStatus.In));
                }
                else
                {
                    SubjectMove.AddLast((cur.Value.point, EnteringStatus.No));    
                }
                cur = IntersectionSubject.NextOrFirst(cur);
            }
        }

        private LinkedListNode<(Vector2, IntersectionStatus)> FindNode(Vector2 point, IntersectionStatus intersectionStatus)
        {
            var cur = IntersectionSubject.First;
            for (var i = 0; i < IntersectionSubject.Count; i++)
            {
                if (Math.Abs(cur.Value.point.X - point.X) < 1e-5 && Math.Abs(cur.Value.point.Y - point.Y) < 1e-5 && cur.Value.intersectionStatus == intersectionStatus)
                    return cur;
                cur = IntersectionSubject.NextOrFirst(cur);
            }

            return null;
        }
        
        private LinkedListNode<(Vector2 point, EnteringStatus enteringStatus)> FindNode(Vector2 point, EnteringStatus enteringStatus)
        {
            var cur = ClipMove.First;
            for (var i = 0; i < ClipMove.Count; i++)
            {
                if (Math.Abs(cur.Value.point.X - point.X) < 1e-5 && Math.Abs(cur.Value.point.Y - point.Y) < 1e-5 && cur.Value.enteringStatus == enteringStatus)
                    return cur;
                cur = ClipMove.NextOrFirst(cur);
            }

            return null;
        }

        private void LabelClipPoints()
        {
            var cur = IntersectionClip.First;
            
            for (var i = 0; i < IntersectionClip.Count; i++)
            {
                if (cur.Value.intersectionStatus == IntersectionStatus.Yes)
                {
                    var curInSub = FindNode(cur.Value.point, cur.Value.intersectionStatus);
                    var prev = IntersectionSubject.PreviousOrLast(curInSub).Value.point;
                    var next = IntersectionSubject.NextOrFirst(curInSub).Value.point;
                    if (IsInsideWindow(prev) && !IsInsideWindow(next))
                        ClipMove.AddFirst((cur.Value.point, EnteringStatus.Out));
                    else
                        ClipMove.AddFirst((cur.Value.point, EnteringStatus.In));
                }
                else
                {
                    ClipMove.AddFirst((cur.Value.point, EnteringStatus.No));    
                }

                cur = IntersectionClip.NextOrFirst(cur);
            }
        }

        private void DeleteEdgeSpecialPoints()
        {
            var curSubject = Subject.First;
            var curClip = Clip.First;
            var tempToDelete = new HashSet<(Vector2 point, IntersectionStatus intersectionStatus)>();
            
            for (var i = 0; i < Subject.Count; i++)
            {
                var prevSubj = curSubject.Value;
                curSubject = Subject.NextOrFirst(curSubject);
                for (var j = 0; j < Clip.Count; j++)
                {
                    var prevClip = curClip.Value;
                    curClip = Clip.NextOrFirst(curClip);
                    if (IsSpecialEdge(prevClip, curClip.Value, prevSubj, curSubject.Value) && 
                        !IsIntersections(prevClip, curClip.Value, prevSubj, curSubject.Value))
                    {
                        tempToDelete.Add((prevClip, IntersectionStatus.Yes));
                        tempToDelete.Add((curClip.Value, IntersectionStatus.Yes));
                    }
                    
                    if (IsSpecialEdge(prevSubj, curSubject.Value, prevClip, curClip.Value) && 
                        !IsIntersections(prevSubj, curSubject.Value, prevClip, curClip.Value))
                    {
                        tempToDelete.Add((prevSubj, IntersectionStatus.Yes));
                        tempToDelete.Add((curSubject.Value, IntersectionStatus.Yes));
                    }
                }
            }

            foreach (var elem in tempToDelete)
            {
                if (Subject.Contains(elem.point))
                    IntersectionSubject.AddAfter(IntersectionSubject.Find(elem), (elem.point, IntersectionStatus.No));

                if (Clip.Contains(elem.point))
                    IntersectionClip.AddAfter(IntersectionClip.Find(elem), (elem.point, IntersectionStatus.No));
                
                IntersectionSubject.Remove(elem);
                IntersectionClip.Remove(elem);
            }
        }

        private void DeleteSpecialVertices()
        {
            var tempToDelete = new HashSet<(Vector2 point, IntersectionStatus intersectionStatus)>();
            var cur = IntersectionSubject.First;
            for (var i = 0; i < IntersectionSubject.Count; i++)
            {
                if (cur.Value.intersectionStatus == IntersectionStatus.Yes &&
                    (Subject.Contains(cur.Value.point) || Clip.Contains(cur.Value.point)) && 
                    IsInsideWindow(IntersectionSubject.PreviousOrLast(cur).Value.point) == IsInsideWindow(IntersectionSubject.NextOrFirst(cur).Value.point))
                    tempToDelete.Add(cur.Value);
                cur = IntersectionSubject.NextOrFirst(cur);
            }
            foreach (var elem in tempToDelete)
            {
                if (Subject.Contains(elem.point))
                    IntersectionSubject.AddAfter(IntersectionSubject.Find(elem), (elem.point, IntersectionStatus.No));

                if (Clip.Contains(elem.point))
                    IntersectionClip.AddAfter(IntersectionClip.Find(elem), (elem.point, IntersectionStatus.No));
                
                IntersectionSubject.Remove(elem);
                IntersectionClip.Remove(elem);
            }
        }

        private bool IsInsideWindow(Vector2 point)
        {
            var intersectionsNum = 0;
            var prev = Clip.Last;
            var cur = Clip.First;
            var prevUnder = prev.Value.Y < point.Y;

            for (var i = 0; i < Clip.Count; ++i)
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
                cur = Clip.NextOrFirst(cur);
                prevUnder = curUnder;        
            }

            return (intersectionsNum&1) != 0;
        }

        private bool IsIntersections(Vector2 insideA, Vector2 insideB, Vector2 outsideA, Vector2 outsideB)
        {
            return IntersectionSubject
                .Where(s => s.intersectionStatus == IntersectionStatus.Yes)
                .Select(s => s.point)
                .Any(elem => elem != insideA && elem != insideB && IsBelongsLine(outsideA, outsideB, elem));
        }

        private bool IsSpecialEdge(Vector2 clipA, Vector2 clipB, Vector2 subjectA, Vector2 subjectB)
        {
            var clip = clipB - clipA;
            var subject = subjectB - subjectA;
            if (!clip.IsCollinear(subject) || !IsBelongsLine(clipA, clipB, subjectA)) return false;
            return IsEdgeInside(clipA, clipB, subjectA, subjectB);
        }

        private bool IsBelongsLine(Vector2 point1, Vector2 point2, Vector2 pointToCheck) =>
            (point1.Y - point2.Y) * pointToCheck.X + (point2.X - point1.X) * pointToCheck.Y == point2.X * point1.Y - point1.X * point2.Y ;
        

        private bool IsEdgeInside(Vector2 insideA, Vector2 insideB, Vector2 outsideA, Vector2 outsideB)
        {
            return (outsideA.X < outsideB.X && insideA.X >= outsideA.X && insideA.X <= outsideB.X && insideB.X >= outsideA.X && insideB.X <= outsideB.X) || 
                   (outsideB.X < outsideA.X && insideA.X >= outsideB.X && insideA.X <= outsideA.X && insideB.X >= outsideB.X && insideB.X <= outsideA.X) ||
                   (outsideA.X == outsideB.X && 
                        (outsideA.Y < outsideB.Y && insideA.Y >= outsideA.Y && insideA.Y <= outsideB.Y && insideB.Y >= outsideA.Y && insideB.Y <= outsideB.Y) ||
                        (outsideA.Y > outsideB.Y && insideA.Y <= outsideA.Y && insideA.Y >= outsideB.Y && insideB.Y <= outsideA.Y && insideB.Y >= outsideB.Y));
        }

        private bool GetIntersectionPoint(Vector2 first1, Vector2 first2, Vector2 second1, Vector2 second2, out Vector2 result)
        {
            var s1 = first2 - first1;
            var s2 = second2 - second1;
            result = new Vector2();

            try
            {
                var s = (-s1.Y * (first1.X - second1.X) + s1.X * (first1.Y - second1.Y)) / (-s2.X * s1.Y + s1.X * s2.Y);
                var t = (s2.X * (first1.Y - second1.Y) - s2.Y * (first1.X - second1.X)) / (-s2.X * s1.Y + s1.X * s2.Y);

                if (!(0 <= s) || !(s <= 1) || !(0 <= t) || !(t <= 1)) return false;
                result = first1 + t * s1;
                return true;

            }
            catch (DivideByZeroException)
            {
                return false;
            }
        }
    }
}