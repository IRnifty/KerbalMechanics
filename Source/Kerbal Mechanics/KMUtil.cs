using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Kerbal_Mechanics
{
    class KMUtil
    {
        public static readonly string NewLine = "\n";

        public static Vector2 GetPointOnCurve(Vector2[] curve, float percent)
        {
            return GetCasteljauPoint(curve, curve.Length - 1, 0, percent);
        }

        private static Vector2 GetCasteljauPoint(Vector2[] points, int r, int i, double t)
        {
            if (r == 0) { return points[i]; }

            Vector2 p1 = GetCasteljauPoint(points, r - 1, i, t);
            Vector2 p2 = GetCasteljauPoint(points, r - 1, i + 1, t);

            return new Vector2((int)((1 - t) * p1.x + t * p2.x), (int)((1 - t) * p1.y + t * p2.y));
        }

        public static Vector2 GetPointBetweenLine(Vector2 point1, Vector2 point2, float percent)
        {
            return point1 + ((point2 - point1) * percent);
        }
    }
}
