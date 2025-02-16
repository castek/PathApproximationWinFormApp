using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PathApproximation.Program;

namespace PathApproximation
{
    public class Approximizer
    {
        const double Degree = Math.PI / 180d;

        /**
         * Approximate the input path by reducing the number of its points.
         * The new path should be within the specified threshold of the input path.
         * Complexity is O(n^2). It uses optimization, so that for appropriate input, the complexity is up to O(n).
         * Idea of algorithm: 
         *      Making a segment longer and longer, checking if the deviation is exceeded, if yes, the previous point is suspicious to be a part of approximate path.
         *      Suspicious points are checked once again to see if they are sufficient to form approximate path. 
         *      So we are switching between highlevel checking against corner points only and detailed checking against all input points.
         * 
         * @param inputPath The input path to approximate.
         * @param threshold The maximum distance between the input path and the new path.
         * @return A new path that approximates the input path.
         */
        public List<PointLog> Approximate(List<PointLog> inputPath, double threshold)
        {
            if (inputPath == null)
            {
                throw new ArgumentNullException(nameof(inputPath));
            }

            if (threshold <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(threshold));
            }

            var input = inputPath.Select((p, i) => new IndexedPoint(p.Point.X, p.Point.Y, i)).ToList();

            var segment = input.Count > 1 ? new Vector(input[0], input[1]) : null;
            var cornerpoints = FilterCornerpoints(input);
            var checkpoints = cornerpoints;
            var output = new List<IndexedPoint>();
            var segmentIndexStart = 0;
            var everyPointCheckStop = 0;
            if (input.Count > 0)
            {
                output.Add(input[0]);
            }
            for (int i = 2; i < input.Count; i++)
            {
                var point = input[i];
                // making the potential segment longer
                segment = new Vector(segment!.From, point);

                if (segment.From.X == point.X && segment.From.Y == point.Y)
                {
                    // still standing in the same place
                    continue;
                }
                if (i == input.Count - 1)
                {
                    // switching to detailed check for the last segment
                    checkpoints = input;
                }

                var deviationExceeded = CheckDeviation(segment, checkpoints, threshold, segmentIndexStart, i, detailed: false);
                if (deviationExceeded)
                {
                    // The point that we just checked exceeded the deviation, so we will add previous point to the aproximated path.

                    var previousPoint = input[i - 1];

                    // Before adding it to the approximated path we will just check the previous segment properly once more
                    if (everyPointCheckStop < i && CheckDeviation(new Vector(segment.From, previousPoint), input, threshold, segmentIndexStart, i, detailed: true))
                    {
                        // Segment was wrongly considered to be a part of aproximated path, let's go through segment once more and check every point
                        everyPointCheckStop = i;
                        i = segmentIndexStart;
                        checkpoints = input; // this will check every point, not just the corner points
                    }
                    else
                    {
                        // Segment is OK, let's add it to the aproximated path
                        output.Add(previousPoint);
                        segment = new Vector(previousPoint, point);
                        segmentIndexStart = i;
                        everyPointCheckStop = Math.Max(everyPointCheckStop, i);
                        if (everyPointCheckStop == i)
                        {
                            checkpoints = cornerpoints; // switching back to corner points only
                        }
                    }
                }
            }
            if (output.Last() != null && (output.Last().X != input.Last().X || output.Last().Y != input.Last().Y))
            {
                output.Add(input.Last());
            }
            return inputPath.Where((p, i) => output.Any(o => o.Index == i)).ToList();
        }

        /**
         * Finds all points that have locally maximal angles between preceding and succesive points.
         * If more points in a row have the same angle, only the first one is considered to be a corner point.
         * Finally it adds the first point of straight sequence of points as a corner point as well.
         */
        public List<IndexedPoint> FilterCornerpoints(List<IndexedPoint> input)
        {
            // changes in angle smaller than this will be ignored
            const double angleSmoothing = 2 * Degree;

            var cornerpoints = new List<IndexedPoint>();
            var previousAngle = (double?)null;
            var maxAngle = (double?)null;
            var cornerIndex = 0;
            for (int i = 2; i < input.Count; i++)
            {
                var first = input[i - 2];
                var middle = input[i - 1];
                var last = input[i];
                var angle = Angle(new Vector(first, middle), new Vector(middle, last));
                if (Math.Abs(angle) < angleSmoothing)
                {
                    if (maxAngle != null)
                    {
                        cornerpoints.Add(input[cornerIndex]);
                        maxAngle = null;
                    }
                    if (previousAngle != null && Math.Abs(previousAngle.Value) > angleSmoothing)
                    {
                        // adding the first point of straight sequence of points as a corner point too
                        cornerpoints.Add(input[i]);
                    }
                }
                else
                {
                    if (maxAngle != null
                        && previousAngle != null
                        && (Math.Abs(previousAngle.Value) > Math.Abs(angle) + angleSmoothing || Math.Sign(previousAngle.Value) != Math.Sign(angle)))
                    {
                        // adding the corner point with maximal angle
                        cornerpoints.Add(input[cornerIndex]);
                        maxAngle = null;
                    }
                    if ((maxAngle != null && Math.Abs(maxAngle.Value) < Math.Abs(angle) && Math.Sign(maxAngle.Value) == Math.Sign(angle))
                        || previousAngle == null
                        || Math.Abs(previousAngle.Value) < Math.Abs(angle)
                        || Math.Sign(previousAngle.Value) != Math.Sign(angle))
                    {
                        // looking for the corner point with maximal angle
                        maxAngle = angle;
                        cornerIndex = i - 1;
                    }
                }
                previousAngle = angle;
            }
            if (maxAngle != null)
            {
                cornerpoints.Add(input[cornerIndex]);
            }
            if (cornerpoints.Last() != null && (cornerpoints.Last().X != input.Last().X || cornerpoints.Last().Y != input.Last().Y))
            {
                cornerpoints.Add(input.Last());
            }
            return cornerpoints;
        }


        /**
         * Checks the shortest distance of a sequence of points to a segment.
         * If the distance is greater than the threshold, the deviation is exceeded.
         */
        private bool CheckDeviation(Vector segment, List<IndexedPoint> points, double threshold, int start, int stop, bool detailed)
        {
            var k = points.Select((c, i) => (c, (int?)i)).Where(c => c.c.Index >= start).FirstOrDefault().Item2;
            if (k == null)
            {
                return !detailed;
            }
            var j = k.Value;
            var point = points[j];
            var segmentFrom = new Point(segment.From.X, segment.From.Y);
            var segmentLength = PointToPointDistance(segmentFrom, new Point(segment.To.X, segment.To.Y));
            while (point.Index < stop)
            {
                var p = new Point(point.X, point.Y);

                var distance = PointToLineDistance(p, segment);
                if (distance > threshold
                    || PointToPointDistance(p, segmentFrom) > segmentLength + threshold) // point p lies in a continuation of the segment further than threshold of segment.To point
                {
                    return true;
                }
                point = points[++j];
            }
            return false;
        }

        private double PointToLineDistance(Point point, Vector vector)
        {
            return Math.Abs((point.X - vector.From.X) * (vector.To.Y - vector.From.Y) - (point.Y - vector.From.Y) * (vector.To.X - vector.From.X)) / Math.Sqrt(Math.Pow(vector.To.Y - vector.From.Y, 2) + Math.Pow(vector.To.X - vector.From.X, 2));
        }

        private double PointToPointDistance(Point point, Point point2)
        {
            return Math.Sqrt(Math.Pow(point.X - point2.X, 2) + Math.Pow(point.Y - point2.Y, 2));
        }

        private double Angle(Vector v1, Vector v2)
        {
            return Angle(new Point(v1.To.X - v1.From.X, v1.To.Y - v1.From.Y), new Point(v2.To.X - v2.From.X, v2.To.Y - v2.From.Y));
        }

        /**
         * Calculate the angle between two vectors.
         * 
         * @param v1 The first vector.
         * @param v2 The second vector.
         * @return The angle between the two vectors in radians. If result is positive, the second vector is to the left (turns counterclockwise) of the first vector.
         */
        private double Angle(Point v1, Point v2)
        {
            var dotProduct = v1.X * v2.X + v1.Y * v2.Y;
            var determinant = v1.X * v2.Y - v1.Y * v2.X;
            return Math.Atan2(determinant, dotProduct);
        }
    }
}
