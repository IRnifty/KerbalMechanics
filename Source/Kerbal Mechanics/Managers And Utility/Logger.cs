using System.IO;
using UnityEngine;

namespace KerbalMechanics
{
    class Logger
    {
        public static void DebugLog(string text)
        {
            Debug.Log("[KM] (Log): " + text);
        }

        public static void DebugWarning(string text)
        {
            Debug.LogWarning("[KM] (Warning): " + text);
        }

        public static void DebugError(string text)
        {
            Debug.LogError("[KM] (ERROR): " + text);
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
