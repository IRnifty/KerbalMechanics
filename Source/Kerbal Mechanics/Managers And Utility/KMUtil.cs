using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KerbalMechanics
{
    static class KMUtilStatic
    {
        public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0) return min;
            else if (val.CompareTo(max) > 0) return max;
            else return val;
        }
    }

    class KMUtil
    {
        /// <summary>
        /// The source directory for all sounds.
        /// </summary>
        public static readonly string soundSource = "KerbalMechanics/Sounds/";

        /// <summary>
        /// The new line character.
        /// </summary>
        public static readonly string NewLine = "\n";

        /// <summary>
        /// The color representation of Kerbal Green.
        /// </summary>
        public static readonly Color KerbalGreen = new Color(0.478f, 0.698f, 0.478f, 0.698f);

        /// <summary>
        /// Posts a failure in the log and on the screen.
        /// </summary>
        /// <param name="part">The part that failed</param>
        /// <param name="failure">The failure</param>
        public static void PostFailure(Part part, string failure)
        {
            string message = "FAILURE: " + part.partInfo.title + failure;

            ScreenMessages.PostScreenMessage(new ScreenMessage(message, 3f, ScreenMessageStyle.UPPER_LEFT, StyleManager.GetStyle("Upper Left - Red")));
            Logger.DebugLog(message);
        }

        public static void SetPartHighlight(Part part, Color color, Part.HighlightType type)
        {
            part.SetHighlightColor(color);
            part.SetHighlightType(type);
            
        }

        public static Vector2 GetPointOnCurve(Vector2[] curve, float percent)
        {
            return GetCasteljauPoint(curve, curve.Length - 1, 0, percent);
        }

        private static Vector2 GetCasteljauPoint(Vector2[] points, int r, int i, double t)
        {
            if (r == 0) { return points[i]; }

            Vector2 p1 = GetCasteljauPoint(points, r - 1, i, t);
            Vector2 p2 = GetCasteljauPoint(points, r - 1, i + 1, t);

            //return new Vector2((float)((1 - t) * p1.x + t * p2.x), (float)((1 - t) * p1.y + t * p2.y));
            return Vector2.Lerp(p1, p2, (float)t);
        }

        public static Vector2d GetPointOnCurve(Vector2d[] curve, float percent)
        {
            return GetCasteljauPoint(curve, curve.Length - 1, 0, percent);
        }

        private static Vector2d GetCasteljauPoint(Vector2d[] points, int r, int i, double t)
        {
            if (r == 0) { return points[i]; }

            Vector2d p1 = GetCasteljauPoint(points, r - 1, i, t);
            Vector2d p2 = GetCasteljauPoint(points, r - 1, i + 1, t);

            //return new Vector2d((1 - t) * p1.x + t * p2.x, (1 - t) * p1.y + t * p2.y);
            return Vector2d.Lerp(p1, p2, (float)t);
        }

        public static bool IsFlagSet<T>(T flags, T flag) where T : struct
        {
            int flagsValue = (int)(object)flags;
            int flagValue = (int)(object)flag;

            return (flagsValue & flagValue) != 0;
        }

        public static void SetFlag<T>(ref T flags, T flag) where T : struct
        {
            int flagsValue = (int)(object)flags;
            int flagValue = (int)(object)flag;

            flags = (T)(object)(flagsValue | flagValue);
        }

        public static void UnsetFlag<T>(ref T flags, T flag) where T : struct
        {
            int flagsValue = (int)(object)flags;
            int flagValue = (int)(object)flag;

            flags = (T)(object)(flagsValue & (~flagValue));
        }

        public static string FormatPercent(double val)
        {
            string toRet = val.ToString("P10");

            toRet = toRet.Substring(0, toRet.Length - 2);

            toRet = toRet.TrimEnd(new Char[] {'0'});

            if (toRet[toRet.Length - 1] == '.')
            {
                toRet = toRet.Substring(0, toRet.Length - 1);
            }

            return toRet + "%";
        }

        public static string FormatPercent(double val, int decimalCutoff)
        {
            string toRet = val.ToString("P" + decimalCutoff.ToString());

            toRet = toRet.Substring(0, toRet.Length - 2);

            toRet = toRet.TrimEnd(new Char[] { '0' });

            if (toRet[toRet.Length - 1] == '.')
            {
                toRet = toRet.Substring(0, toRet.Length - 1);
            }

            return toRet + "%";
        }
    }
}
