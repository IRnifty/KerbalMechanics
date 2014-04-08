using System.IO;
using UnityEngine;

namespace Kerbal_Mechanics
{
    class Logger
    {
        public static void DebugLog(string text)
        {
            Debug.Log("[KM]: " + text);
        }

        public static void DebugWarning(string text)
        {
            Debug.LogWarning("[KM]: " + text);
        }

        public static void DebugError(string text)
        {
            Debug.LogError("[KM]: " + text);
        }

        public static void LogToFile(string text, bool asLines)
        {
            try
            {
                if (asLines)
                {
                    string[] lines = text.Split("\n".ToCharArray());

                    File.WriteAllLines(@"C:\KM Log.txt", lines);
                }
                else
                {
                    File.WriteAllText(@"C:\KM Log.txt", text);
                }
            }
            catch
            {

            }
        }
    }
}
