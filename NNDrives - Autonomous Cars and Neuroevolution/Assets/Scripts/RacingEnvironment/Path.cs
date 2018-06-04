using System;
using System.Collections.Generic;

using Helpers;

using UnityEngine;

namespace RacingEnvironment
{
    /// <summary>
    /// Represents a curve consisting of segments.
    /// Segments are fourth-order Bézier curves so there are 2 anchor and 2 control points in each one.
    /// </summary>
    [Serializable]
    public class Path
    {
        /// <summary>
        /// Contains all points of this path.
        /// </summary>
        /// <remarks>
        /// Except the very first segment, 3 points are hold for each segment in the order mentioned below:
        ///     1. First control point
        ///     2. Second control point
        ///     3. Second anchor point.
        /// *) The first anchor point of each of these segments is the second anchor point of the previous segment.
        /// For the very first segment 4 points are hold:
        ///     1. First anchor point
        ///     2. First control point
        ///     3. Second control point
        ///     4. Second anchor point.
        /// </remarks>
        [SerializeField, HideInInspector]
        private List<Vector2> points;

        /// <summary>
        /// A value indicating whether the path is closed.
        /// </summary>
        [SerializeField, HideInInspector]
        private bool isClosed;

        /// <summary>
        /// A value indicating whether the points must be set automatically.
        /// </summary>
        [SerializeField, HideInInspector]
        private bool autoSetControlPoints;

        /// <summary>
        /// Initializes a new instance of the <see cref="Path"/> class which has one segment with the given center point.
        /// </summary>
        /// <param name="center">The center point of the curve.</param>
        public Path(Vector2 center)
        {
            points = new List<Vector2>
        {
            center + Vector2.left,                 // first anchor point
            center + (Vector2.left * 0.5f),        // first control point
            center + (Vector2.right * 0.5f),       // second control point
            center + Vector2.right                 // second anchor point
        };
        }

        /// <summary>
        /// Gets the number of all points in the path.
        /// </summary>
        public int PointsCount
        {
            get
            {
                return points.Count;
            }
        }

        /// <summary>
        /// Gets the number of all segments in this path.
        /// </summary>
        /// <remarks>
        /// To get segments count all points count is divided by 3 and not 4, 
        /// because each anchor point(except the very first one) is an anchor point twice:
        ///  1. Second anchor point of the left segment
        ///  2. First anchor point of the right segment.
        /// But the <see cref="points"/> has only 1 object for that point.
        /// </remarks>
        public int SegmentsCount
        {
            get
            {
                return points.Count / 3;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the path is closed.
        /// </summary>
        public bool IsClosed
        {
            get
            {
                return isClosed;
            }

            set
            {
                if (isClosed != value)
                {
                    ToggleClosed();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the points in this path must be set automatically.
        /// If this option is turned on, only anchor points can be moved by the user.
        /// </summary>
        public bool AutoSetControlPoints
        {
            get
            {
                return autoSetControlPoints;
            }

            set
            {
                if (autoSetControlPoints != value)
                {
                    autoSetControlPoints = value;
                    if (autoSetControlPoints)
                    {
                        AutoSetAllControlPoints();
                    }
                }
            }
        }

        /// <summary>
        /// Gets point of this path at the given index.
        /// </summary>
        /// <param name="index">Index of the point.</param>
        /// <returns>Point at the given index.</returns>
        public Vector2 this[int index]
        {
            get
            {
                return points[index];
            }
        }

        /// <summary>
        /// Get points of the segment at given index. 
        /// </summary>
        /// <param name="index">Index of the segment.</param>
        /// <returns>All 4 points of the segment.</returns>
        public Vector2[] GetPointsInSegment(int index)
        {
            if (index < 0 || index >= SegmentsCount)
            {
                throw new ArgumentOutOfRangeException("index", "Index was out of range. It be non - negative and less than the number of segments.");
            }

            // Make sure there won't be ArgumentRangeException when the path is closed.
            return new Vector2[]
            {
            points[(index * 3) + 0],
            points[(index * 3) + 1],
            points[(index * 3) + 2],
            points[GetValidIndex((index * 3) + 3)]
            };
        }

        /// <summary>
        /// Adds a new segment which has two anchor points:
        ///     1. The last point of this path before this operation,
        ///     2. A new anchor point with given <paramref name="anchorPosition"/>.
        /// </summary>
        /// <param name="anchorPosition">Position for the new anchor point.</param>
        public void AddSegment(Vector2 anchorPosition)
        {
            points.Add((points[points.Count - 1] * 2) - points[points.Count - 2]);
            points.Add((points[points.Count - 1] + anchorPosition) * 0.5f);
            points.Add(anchorPosition);

            if (autoSetControlPoints)
            {
                AutoSetAllAffectedControlPoints(points.Count - 1);
            }
        }

        /// <summary>
        /// Splits the segment at given index into 2 new segments.
        /// </summary>
        /// <param name="splitPointPosition">Position at which split must be done.</param>
        /// <param name="segmentIndex">Index of the segment which has to be split.</param>
        public void SplitSegment(Vector2 splitPointPosition, int segmentIndex)
        {
            if (segmentIndex < 0 || segmentIndex >= SegmentsCount)
            {
                throw new ArgumentOutOfRangeException("segmentIndex", "Index was out of range. Must be non-negative and less than the number of segments.");
            }

            int lastPointIndexInSegment = (segmentIndex * 3) + 2;
            points.InsertRange(lastPointIndexInSegment, new Vector2[] { Vector2.zero, splitPointPosition, Vector2.zero });

            int insertedAnchorIndex = lastPointIndexInSegment + 1;
            if (autoSetControlPoints)
            {
                AutoSetAllAffectedControlPoints(insertedAnchorIndex);
            }
            else
            {
                AutoSetAnchorControlPoints(insertedAnchorIndex);
            }
        }

        /// <summary>
        /// Deletes the segment at given index.
        /// </summary>
        /// <param name="index">Index of the segment which has to be deleted.</param>
        public void DeleteSegment(int index)
        {
            if (points.Count <= 4 || (isClosed && SegmentsCount <= 2))
            {
                return;
            }

            int anchorIndex = index * 3;
            if (anchorIndex < 0 || anchorIndex > points.Count - 1)
            {
                throw new ArgumentOutOfRangeException("index", "Index was out of range. Must be non-negative and less than the number of segments.");
            }

            if (anchorIndex == 0)
            {
                if (isClosed)
                {
                    points[points.Count - 1] = points[2];
                }

                points.RemoveRange(0, 3);
            }
            else if (anchorIndex == points.Count - 1 && !isClosed)
            {
                points.RemoveRange(anchorIndex - 2, 3);
            }
            else
            {
                points.RemoveRange(anchorIndex - 1, 3);
            }
        }

        /// <summary>
        /// Moves the path to a new position with coordinates of <paramref name="newPosition"/>.
        /// </summary>
        /// <param name="newPosition">Position the first control point of the path must be moved to.</param>
        public void Move(Vector2 newPosition)
        {
            Vector2 firstControlPoint = points[1];
            points[1] = newPosition;

            for (int i = 0; i < points.Count; i++)
            {
                if (i == 1)
                {
                    continue;
                }

                Vector2 offset = points[i] - firstControlPoint;
                points[i] = newPosition + offset;
            }
        }

        /// <summary>
        /// Moves the point at the given <paramref name="index"/> to a new position 
        /// with coordinates of <paramref name="newPosition"/>.
        /// </summary>
        /// <param name="index">Index of the point which has to be moved.</param>
        /// <param name="newPosition">Position the point must be moved to.</param>
        public void MovePoint(int index, Vector2 newPosition)
        {
            // If 'AutoSetControlPoints' option is turned on, only anchor points can be moved.
            if (index % 3 != 0 && autoSetControlPoints)
            {
                return;
            }

            Vector2 deltaMove = newPosition - points[index];
            points[index] += deltaMove;

            if (autoSetControlPoints)
            {
                AutoSetAllAffectedControlPoints(index);
            }
            else
            {
                /*
                 * If the moved point is an anchor point, 
                 * then 2 (1 in case of edge anchor points) neighbor control points must be also moved.
                 * Otherwise, if the moved point is a control point, then the other one must be rotated to keep the angle between
                 * those 2 points be the same as it was before movement.
                 */

                if (index % 3 == 0)
                {
                    if (index - 1 >= 0 || isClosed)
                    {
                        points[GetValidIndex(index - 1)] += deltaMove;
                    }

                    if (index + 1 < points.Count || isClosed)
                    {
                        points[GetValidIndex(index + 1)] += deltaMove;
                    }
                }
                else
                {
                    bool isTheLeftControlPoint = (index + 1) % 3 == 0;
                    int anchorIndex = isTheLeftControlPoint ? index + 1 : index - 1;
                    int correspondingIndex = isTheLeftControlPoint ? index + 2 : index - 2;

                    if ((correspondingIndex >= 0 && correspondingIndex < points.Count) || isClosed)
                    {
                        float distanceToAnchor = (points[GetValidIndex(anchorIndex)] - points[GetValidIndex(correspondingIndex)]).magnitude;

                        Vector2 newDirectionToAnchor = (points[GetValidIndex(anchorIndex)] - newPosition).normalized;

                        points[GetValidIndex(correspondingIndex)] = points[GetValidIndex(anchorIndex)] + (newDirectionToAnchor * distanceToAnchor);
                    }
                }
            }
        }

        /// <summary>
        /// Gets points on the path that are evenly spaced with given value.
        /// </summary>
        /// <param name="spacing">Distance between points.</param>
        /// <param name="resolution">Time value.</param>
        /// <returns>Array of points that are evenly spaced on the curve.</returns>
        public Vector2[] CalculateEvenlySpacedPoints(float spacing, float resolution = 1)
        {
            var evenlySpacedPoints = new List<Vector2>
        {
            points[0]
        };

            Vector2 lastPoint = points[0];
            float distanceSinceLastEvenPoint = 0;

            for (int i = 0; i < SegmentsCount; i++)
            {
                Vector2[] pointsInSegment = GetPointsInSegment(i);

                float controlNetLength = Vector2.Distance(pointsInSegment[0], pointsInSegment[1])
                                        + Vector2.Distance(pointsInSegment[1], pointsInSegment[2])
                                        + Vector2.Distance(pointsInSegment[2], pointsInSegment[3]);

                float estimatedCurveLength = Vector2.Distance(pointsInSegment[0], pointsInSegment[3]) + (controlNetLength / 2);
                int divisions = Mathf.CeilToInt((estimatedCurveLength * resolution) * 10);

                float t = 0;
                while (t <= 1)
                {
                    t += 1.0f / divisions;

                    // Get point on the curve
                    Vector2 pointOnCurve = BezierHelper.EvaluateCubic(pointsInSegment[0], pointsInSegment[1], pointsInSegment[2], pointsInSegment[3], t);
                    distanceSinceLastEvenPoint += Vector2.Distance(lastPoint, pointOnCurve);

                    /*
                     * If distance between the last point and the new one is less than spacing, then just ignore that new point.
                     * Otherwise if the distance is equal to spacing, add that point to evenly spaced points.
                     * And if the distance is more than spacing, then move that point back so that the distance becomes equal to 
                     * spacing and that point to evenly spaced points.
                     */
                    while (distanceSinceLastEvenPoint >= spacing)
                    {
                        float overshootDistance = distanceSinceLastEvenPoint - spacing;
                        Vector2 newEvenlySpacedPoint = pointOnCurve + ((lastPoint - pointOnCurve).normalized * overshootDistance);

                        evenlySpacedPoints.Add(newEvenlySpacedPoint);

                        distanceSinceLastEvenPoint = overshootDistance;
                        lastPoint = newEvenlySpacedPoint;
                    }

                    lastPoint = pointOnCurve;
                }
            }

            return evenlySpacedPoints.ToArray();
        }

        /// <summary>
        /// Automatically moves all control points to new positions to make path be smooth.
        /// </summary>
        private void AutoSetAllControlPoints()
        {
            // Autoset 2 neighbor control points of each anchor point.
            for (int i = 0; i < points.Count; i += 3)
            {
                AutoSetAnchorControlPoints(i);
            }

            // Autoset the first and last control points.
            AutoSetStartAndEndControls();
        }

        /// <summary>
        /// Automatically determines new positions for 2 neighbor control points of the anchor point at given index
        /// and moves them to those positions.
        /// </summary>
        /// <param name="anchorIndex">Index of the anchor point.</param>
        private void AutoSetAnchorControlPoints(int anchorIndex)
        {
            Vector2 anchorPosition = points[anchorIndex];

            Vector2 direction = Vector2.zero;
            float leftNeighborDistance = 0f;
            float rightNeighborDistance = 0f;

            /*
             * Control points are going to be in a vector (line) which is perpendicular to the bisector of the angle between
             * vectors contructed from 3 anchor points:
             *      1. Anchor point and previous anchor point,
             *      2. Next anchor point and anchor point.
             */

            if (anchorIndex - 3 >= 0 || isClosed)
            {
                Vector2 offsetToFirstControlPoint = points[GetValidIndex(anchorIndex - 3)] - anchorPosition;

                leftNeighborDistance = offsetToFirstControlPoint.magnitude;
                direction += offsetToFirstControlPoint.normalized;
            }

            if (anchorIndex + 3 <= points.Count || isClosed)
            {
                Vector2 offsetToSecondControlPoint = points[GetValidIndex(anchorIndex + 3)] - anchorPosition;

                rightNeighborDistance = offsetToSecondControlPoint.magnitude;
                direction -= offsetToSecondControlPoint.normalized;
            }

            direction.Normalize();

            int leftControlIndex = anchorIndex - 1;
            if (leftControlIndex >= 0 || isClosed)
            {
                points[GetValidIndex(leftControlIndex)] = anchorPosition + (direction * leftNeighborDistance * 0.5f);
            }

            int rightControlIndex = anchorIndex + 1;
            if (rightControlIndex < points.Count || isClosed)
            {
                points[GetValidIndex(rightControlIndex)] = anchorPosition + (direction * (-1) * rightNeighborDistance * 0.5f);
            }
        }

        /// <summary>
        /// If the path isn't closed automatically determines new positions for the first and last control points 
        /// and moves them to those positions.
        /// </summary>
        private void AutoSetStartAndEndControls()
        {
            if (!isClosed)
            {
                points[1] = (points[0] + points[2]) * 0.5f;
                points[points.Count - 2] = (points[points.Count - 1] + points[points.Count - 3]) * 0.5f;
            }
        }

        /// <summary>
        /// After an anchor point has been somehow updated, automatically moves all control points 
        /// in 2 neighbor segments to new positions to make path be smooth.
        /// </summary>
        /// <param name="updatedAnchorIndex">Index of the anchor point that has been updated.</param>
        private void AutoSetAllAffectedControlPoints(int updatedAnchorIndex)
        {
            for (int i = updatedAnchorIndex - 3; i <= updatedAnchorIndex + 3; i += 3)
            {
                if ((i >= 0 && i < points.Count) || isClosed)
                {
                    AutoSetAnchorControlPoints(GetValidIndex(i));
                }
            }

            AutoSetStartAndEndControls();
        }

        /// <summary>
        /// If the path is closed, makes it open and if the path is open makes it closed.
        /// </summary>
        private void ToggleClosed()
        {
            isClosed = !isClosed;

            if (isClosed)
            {
                points.Add((points[points.Count - 1] * 2) - points[points.Count - 2]);
                points.Add((points[0] * 2) - points[1]);

                if (autoSetControlPoints)
                {
                    AutoSetAnchorControlPoints(0);
                    AutoSetAnchorControlPoints(points.Count - 3);
                }
            }
            else
            {
                points.RemoveRange(points.Count - 2, 2);

                if (autoSetControlPoints)
                {
                    AutoSetStartAndEndControls();
                }
            }
        }

        /// <summary>
        /// Returns the representation of the <paramref name="index"/> between 0 and <see cref="PointsCount"/>.
        /// So the returned index and the given one are congruent modulo <see cref="PointsCount"/>.
        /// </summary>
        /// <param name="index">Index which has to be validated.</param>
        /// <returns>An integer number that is between 0 and <see cref="PointsCount"/>.</returns>
        private int GetValidIndex(int index)
        {
            return (index + points.Count) % points.Count;
        }
    }
}