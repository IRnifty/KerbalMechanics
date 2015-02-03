using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KerbalMechanics
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
    class KerbalMechanicsApp : MonoBehaviour
    {
        static KerbalMechanicsApp instance;

        private ApplicationLauncherButton kmAppButton;

        private bool isShowing = false;
        private bool isHovering = false;

        private Rect windowRect;
        private Rect alertRect;

        private int currentTab = 1;

        private string[] tabNames = new string[] { "Ship Status", "KM Settings" };

        public ModuleReliabilityBase alerting = null;
        public string alertFailure = "";

        public void Awake()
        {
            instance = this;

            DontDestroyOnLoad(gameObject);

            kmAppButton = ApplicationLauncher.Instance.AddModApplication(
                       DisplayApp,
                       HideApp,
                       HoverApp,
                       UnhoverApp,
                       DummyVoid,
                       DummyVoid,
                       ApplicationLauncher.AppScenes.SPACECENTER | ApplicationLauncher.AppScenes.FLIGHT,
                       (Texture)GameDatabase.Instance.GetTexture("KerbalMechanics/Textures/Toolbar", false));

            ApplicationLauncher.Instance.EnableMutuallyExclusive(kmAppButton);

            windowRect = new Rect(Screen.width - 200f, 40f, 250f, 200f);
            alertRect = new Rect(Screen.width / 2f - 100f, Screen.height / 2f - 75f, 200f, 125f);

            GameEvents.onLevelWasLoaded.Add((e) => { Reposition(); KMSettings.Instance.SaveSettings(); });

            // Settings

            KMSettings.Init();
        }

        public void OnGUI()
        {
            if (isShowing || isHovering)
            {
                GUILayout.Window(GetInstanceID(), windowRect, KMWindow, "Kerbal Mechanics", HighLogic.Skin.window);
            }

            if (alerting != null)
            {
                GUILayout.Window(GetInstanceID(), alertRect, AlertWindow, "Alert! Part Failure!", HighLogic.Skin.window);
            }
        }

        private void KMWindow(int windowID)
        {
            GUILayout.BeginVertical();

            if (HighLogic.LoadedSceneIsFlight)
            {
                GUILayout.BeginHorizontal();

                currentTab = GUILayout.SelectionGrid(currentTab, tabNames, 2, HighLogic.Skin.button);

                GUILayout.EndHorizontal();
            }
            else if (currentTab == 0)
            {
                currentTab = 1;
            }

            if (currentTab == 0)
            {
                InstrumentReliabilityManager.Instance.highlightingReliability = GUILayout.Toggle(InstrumentReliabilityManager.Instance.highlightingReliability, "Map Ship Status", HighLogic.Skin.toggle);
            }
            else if (currentTab == 1)
            {
                KMSettings.Instance.stopTimeWarpOnFailure = GUILayout.Toggle(KMSettings.Instance.stopTimeWarpOnFailure, "Stop Time Warp on Failure", HighLogic.Skin.toggle);
                KMSettings.Instance.alertMessageOnFailure = GUILayout.Toggle(KMSettings.Instance.alertMessageOnFailure, "Alert Message on Failure", HighLogic.Skin.toggle);
                GUI.enabled = KMSettings.Instance.alertMessageOnFailure;
                KMSettings.Instance.highlightFailedPart = GUILayout.Toggle(KMSettings.Instance.highlightFailedPart, "Highlight Part With Alert", HighLogic.Skin.toggle);
                GUI.enabled = true;
            }

            GUILayout.EndVertical();
        }

        private void AlertWindow(int windowID)
        {
            GUILayout.BeginVertical();

            GUILayout.Label(alertFailure, HighLogic.Skin.label);
            GUILayout.Label("", HighLogic.Skin.label);

            if (GUILayout.Button("Close", HighLogic.Skin.button))
            {
                if (!InstrumentReliabilityManager.Instance.highlightingReliability)
                {
                    KMUtil.SetPartHighlight(alerting.part, KMUtil.KerbalGreen, Part.HighlightType.OnMouseOver);
                }

                alerting = null;
            }

            GUILayout.EndVertical();
        }

        private void DisplayApp()
        {
            Reposition();
            isShowing = true;
        }

        private void HoverApp()
        {
            Reposition();
            isHovering = true;
        }

        private void HideApp()
        {
            isShowing = false;
        }

        private void UnhoverApp()
        {
            isHovering = false;
        }

        private void Reposition()
        {
            float anchor = kmAppButton.GetAnchor().x;
            Logger.DebugLog(anchor.ToString());

            windowRect = new Rect(Mathf.Min(anchor + 960.5f, 1670f), 40f, 250f, 200f);
        }

        private void DummyVoid() { }

        public static void Alert(ModuleReliabilityBase moduleFor, string failure)
        {
            instance.alerting = moduleFor;
            instance.alertFailure = failure;

            if (KMSettings.Instance.highlightFailedPart) { KMUtil.SetPartHighlight(moduleFor.part, Color.red, Part.HighlightType.AlwaysOn); }
        }

        public static bool IsAlertingThisPart(Part part)
        {
            if (instance.alerting == null) { return false; }
            return instance.alerting.part == part;
        }
    }
}