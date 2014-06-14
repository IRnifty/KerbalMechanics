using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Kerbal_Mechanics
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    class PartLister : MonoBehaviour
    {
        Vector2 scrollPos = Vector2.zero;
        Rect windowRect = new Rect(20, 20, 400, 300);

        void OnGUI()
        {
            GUILayout.Window(4761, windowRect, DrawPartList, "Part List");
        }

        void DrawPartList(int windowID)
        {
            scrollPos = GUILayout.BeginScrollView(scrollPos);

            foreach (Part p in FlightGlobals.ActiveVessel.Parts)
            {
                GUILayout.Label(p.partInfo.title);
                foreach (PartModule m in p.Modules)
                {
                    GUILayout.Label("     " + (string.IsNullOrEmpty(m.GUIName) ? m.ClassName : m.GUIName));
                }
            }

            GUILayout.EndScrollView();

            GUI.DragWindow();
        }
    }
}
