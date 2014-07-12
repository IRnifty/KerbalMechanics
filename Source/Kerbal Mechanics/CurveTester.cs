using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Kerbal_Mechanics
{
    [KSPAddon(KSPAddon.Startup.MainMenu, false)]
    class CurveTester : MonoBehaviour
    {
        public float quality = 0.75f;

        public int reliabilityDrainPerfect = 425;
        public int reliabilityDrainTerrible = 53;

        Vector2d[] points;

        Texture2D graph;

        // Use this for initialization
        void Start()
        {

            //points = new Vector2d[] { new Vector2d(0, 1),
            //new Vector2d(0.75, 1),
            //new Vector2d(0.25, 0),
            //new Vector2d(1, 0) };

            points = new Vector2d[] { new Vector2d(0, reliabilityDrainTerrible),
            new Vector2d(0.75, reliabilityDrainTerrible),
            new Vector2d(0.25, reliabilityDrainPerfect),
            new Vector2d(1, reliabilityDrainPerfect) };

            graph = new Texture2D(200, 200, TextureFormat.RGBA32, false);

            for (int i = 0; i < 200; i++)
            {
                for (int j = 0; j < 200; j++)
                {
                    graph.SetPixel(i, j, Color.black);
                }
            }

            for (float f = 0; f <= 1f; f += 0.0001f)
            {
                Vector2d p = KMUtil.GetPointOnCurve(points, f);
                graph.SetPixel((int)(p.x * 200), (int)(((p.y - reliabilityDrainTerrible) / (reliabilityDrainPerfect - reliabilityDrainTerrible)) * 200), Color.red);
            }

            graph.Apply();
        }

        //void OnGUI()
        //{

        //    GUILayout.Window(GetInstanceID(), new Rect(60f, 60f, 400f, 400f), WindowFunc, "Window");
        //}

        public void WindowFunc(int windowID)
        {
            double fortyFive = KMUtil.GetPointOnCurve(points, 0.45f).y;
            double fiftyFive = KMUtil.GetPointOnCurve(points, 0.55f).y;

            double ninety = KMUtil.GetPointOnCurve(points, 0.9f).y;
            double hundred = KMUtil.GetPointOnCurve(points, 1f).y;

            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.Label(quality.ToString("P1"));
            quality = GUILayout.HorizontalSlider(quality, 0f, 1f);
            GUILayout.EndHorizontal();
            GUILayout.Label("45% - 55%:  " + (fiftyFive - fortyFive).ToString("0.##########"));
            GUILayout.Label("90% - 100%: " + (hundred - ninety).ToString("0.##########"));
            GUIContent c = new GUIContent(graph, "Graph");
            GUILayout.Label(c);
            GUILayout.EndVertical();
        }
    }
}
