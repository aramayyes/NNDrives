using UnityEngine;

namespace Helpers
{
    /// <summary>
    /// Provides helper methods to work with Bezier Curves.
    /// </summary>
    public static class BezierHelper
    {
        /// <summary>
        /// Interpolates between the vectors <paramref name="startControlPoint"/>
        ///   and <paramref name="endControlPoint"/> using quadratic Bézier curve with given control point.
        /// </summary>
        /// <param name="startControlPoint">The start control point.</param>
        /// <param name="controlPoint">Extra control point for quadratic Bézier curve.</param>
        /// <param name="endControlPoint">The end control point.</param>
        /// <param name="time">The interpolation value.</param>
        /// <returns>The interpolated point.</returns>
        public static Vector2 EvaluateQuadratic(Vector2 startControlPoint, Vector2 controlPoint, Vector2 endControlPoint, float time)
        {
            Vector2 intermediatePoint1 = Vector2.Lerp(startControlPoint, controlPoint, time);
            Vector2 intermediatePoint2 = Vector2.Lerp(controlPoint, endControlPoint, time);

            return Vector2.Lerp(intermediatePoint1, intermediatePoint2, time);
        }

        /// <summary>
        /// Interpolates between the vectors <paramref name="startControlPoint"/>
        ///   and <paramref name="endControlPoint"/> using cubic Bézier curve with 2 given control point.
        /// </summary>
        /// <param name="startControlPoint">The start control point.</param>
        /// <param name="controlPoint1">First extra control point for cubic Bézier curve.</param>
        /// <param name="controlPoint2">Second extra control point for cubic Bézier curve.</param>
        /// <param name="endControlPoint">The end control point.</param>
        /// <param name="time">The interpolation value.</param>
        /// <returns>The interpolated point.</returns>
        public static Vector2 EvaluateCubic(Vector2 startControlPoint, Vector2 controlPoint1, Vector2 controlPoint2, Vector2 endControlPoint, float time)
        {
            Vector2 intermediatePoint1 = EvaluateQuadratic(startControlPoint, controlPoint1, controlPoint2, time);
            Vector2 intermediatePoint2 = EvaluateQuadratic(controlPoint1, controlPoint2, endControlPoint, time);

            return Vector2.Lerp(intermediatePoint1, intermediatePoint2, time);
        }
    }
}