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
        /// Has Debug been declared by the command line?
        /// </summary>
        public static bool DebugDeclared = false;

        /// <summary>
        /// Is this the Career game mode?
        /// </summary>
        public static bool IsModeCareer
        {
            get { return HighLogic.CurrentGame.Mode == Game.Modes.CAREER; }
        }

        /// <summary>
        /// Stops the current time warp, resetting to x1 speed.
        /// </summary>
        public static void StopTimeWarp()
        {
            TimeWarp.SetRate(0, true);
        }

        /// <summary>
        /// Sets the part's highlight color.
        /// </summary>
        /// <param name="part">The part to set.</param>
        /// <param name="color">The color to set.</param>
        /// <param name="type">The highlight type to set.</param>
        public static void SetPartHighlight(Part part, Color color, Part.HighlightType type)
        {
            part.highlightColor = color;
            part.highlightType = type;
            part.SetHighlight(type == Part.HighlightType.AlwaysOn, false);
        }

        /// <summary>
        /// Gets the interpolated color based on the reliability passed in, between green, yellow, and red.
        /// </summary>
        /// <param name="reliability">The reliability</param>
        /// <returns>Returns the interpolated color.</returns>
        public static Color GetReliabilityColor(float reliability)
        {
            if (reliability < 0.5f)
            {
                return Color.Lerp(Color.red, Color.yellow, reliability * 2f);
            }
            
            return Color.Lerp(Color.yellow, Color.green, (reliability - 0.5f) * 2f);
        }

        /// <summary>
        /// Posts a failure in the log and on the screen.
        /// </summary>
        /// <param name="part">The part that failed</param>
        /// <param name="failure">The failure</param>
        public static void PostFailure(ModuleReliabilityBase module, string failure)
        {
            if (KMSettings.Instance.stopTimeWarpOnFailure) { StopTimeWarp(); }

            string message = "FAILURE: " + module.part.partInfo.title + failure;

            ScreenMessages.PostScreenMessage(new ScreenMessage(message, 3f, ScreenMessageStyle.UPPER_LEFT, StyleManager.GetStyle("Upper Left - Red")));
            if (KMSettings.Instance.alertMessageOnFailure) { KerbalMechanicsApp.Alert(module, message); }
            Logger.DebugLog(message);
        }

        /// <summary>
        /// Gets a Vector2 point on an nth order curve.
        /// </summary>
        /// <param name="curve">The list of Vector2 points defining the curve as well as the order of the curve.</param>
        /// <param name="percent">The percent in the curve.</param>
        /// <returns>Returns the  Vector2 point on the curve, given the set of points and the percent along it.</returns>
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

        /// <summary>
        /// Gets a Vector2d point on an nth order curve.
        /// </summary>
        /// <param name="curve">The list of Vector2d points defining the curve as well as the order of the curve.</param>
        /// <param name="percent">The percent in the curve.</param>
        /// <returns>Returns the  Vector2d point on the curve, given the set of points and the percent along it.</returns>
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

        /// <summary>
        /// Assesses whether the specified bit flag is set on the specified flag arrangement.
        /// </summary>
        /// <typeparam name="T">The type of flag set.</typeparam>
        /// <param name="flags">The flag set to evaluate.</param>
        /// <param name="flag">The flag evaluated in the flag set.</param>
        /// <returns>Returns true if the specified flag is set in the specified flag set, otherwise false.</returns>
        public static bool IsFlagSet<T>(T flags, T flag) where T : struct
        {
            int flagsValue = (int)(object)flags;
            int flagValue = (int)(object)flag;

            return (flagsValue & flagValue) != 0;
        }

        /// <summary>
        /// Sets the specified bit flag to true in the specified flag set.
        /// </summary>
        /// <typeparam name="T">The type of flag set.</typeparam>
        /// <param name="flags">The flag set to set.</param>
        /// <param name="flag">The flag to set to true in the flag set.</param>
        public static void SetFlag<T>(ref T flags, T flag) where T : struct
        {
            int flagsValue = (int)(object)flags;
            int flagValue = (int)(object)flag;

            flags = (T)(object)(flagsValue | flagValue);
        }

        /// <summary>
        /// Sets the specified bit flag to false in the specified flag set.
        /// </summary>
        /// <typeparam name="T">The type of flag set.</typeparam>
        /// <param name="flags">The flag set to set.</param>
        /// <param name="flag">The flag to set to false in the flag set.</param>
        public static void UnsetFlag<T>(ref T flags, T flag) where T : struct
        {
            int flagsValue = (int)(object)flags;
            int flagValue = (int)(object)flag;

            flags = (T)(object)(flagsValue & (~flagValue));
        }
    }
}
