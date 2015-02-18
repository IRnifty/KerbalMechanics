using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KerbalMechanics
{
    /// <summary>
    /// The Kerbal Mechanics app.
    /// </summary>
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    class KerbalMechanicsApp : MonoBehaviour
    {
        #region Variables

        /// <summary>
        /// The instance of this app.
        /// </summary>
        static KerbalMechanicsApp instance;

        /// <summary>
        /// The App Launcher button.
        /// </summary>
        private ApplicationLauncherButton kmAppButton;

        /// <summary>
        /// Is the app showing?
        /// </summary>
        private bool isShowing = false;
        /// <summary>
        /// Is the app being hovered over?
        /// </summary>
        private bool isHovering = false;

        /// <summary>
        /// The window bounds.
        /// </summary>
        private Rect windowRect;
        /// <summary>
        /// The alert bounds.
        /// </summary>
        private Rect alertRect;

        /// <summary>
        /// The current window tab.
        /// </summary>
        private int currentTab = 1;
        /// <summary>
        /// The tab names.
        /// </summary>
        private string[] tabNames = new string[] { "Ship Status", "KM Settings" };

        /// <summary>
        /// The reference to the currently alerting part.
        /// </summary>
        public ModuleReliabilityBase alerting = null;
        /// <summary>
        /// The failure to alert.
        /// </summary>
        public string alertFailure = "";

        #endregion

        #region Unity Methods

        /// <summary>
        /// Called when the MonoBehaviour is awakened.
        /// </summary>
        public void Awake()
        {
            // If there's already an instance, delete this instance.
            if (instance != null)
            {
                Destroy(gameObject);
                return;
            }

            // Do not destroy this instance.
            DontDestroyOnLoad(gameObject);
            // Save this instance so others can detect it.
            instance = this;
            // If the player returns to the main menu, disconnect and destroy this isntance.
            GameEvents.onGameSceneLoadRequested.Add((e) => { if (e == GameScenes.MAINMENU) { instance = null; Destroy(gameObject); } });

            // Create the App Launcher button and add it.
            kmAppButton = ApplicationLauncher.Instance.AddModApplication(
                       DisplayApp,
                       HideApp,
                       HoverApp,
                       UnhoverApp,
                       DummyVoid,
                       Disable,
                       ApplicationLauncher.AppScenes.SPACECENTER | ApplicationLauncher.AppScenes.FLIGHT,
                       (Texture)GameDatabase.Instance.GetTexture("KerbalMechanics/Textures/Toolbar", false));

            // This app should be mutually exclusive. (It should disappear when the player clicks on another app.
            ApplicationLauncher.Instance.EnableMutuallyExclusive(kmAppButton);

            // Set up the window bounds.
            windowRect = new Rect(Screen.width - 200f, 40f, 250f, 200f);
            // Set up the alert bounds.
            alertRect = new Rect(Screen.width / 2f - 100f, Screen.height / 2f - 75f, 200f, 125f);

            // Add a level-loaded event which will reposition the app.
            GameEvents.onLevelWasLoaded.Add((e) => { Reposition(); KMSettings.Instance.SaveSettings(); });

            // Initialize settings
            KMSettings.Init();
        }

        #endregion

        #region GUI Methods

        /// <summary>
        /// Called when Unity reaches the GUI phase.
        /// </summary>
        public void OnGUI()
        {
            // If the app is showing or hovered over,
            if (isShowing || isHovering)
            {
                // Display the window.
                GUILayout.Window(GetInstanceID(), windowRect, KMWindow, "Kerbal Mechanics", HighLogic.Skin.window);
            }

            // If the alert is showing,
            if (alerting != null)
            {
                // Display the window.
                GUILayout.Window(GetInstanceID() + 1, alertRect, AlertWindow, "Alert! Part Failure!", HighLogic.Skin.window);
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

        #endregion

        #region App Methods

        /// <summary>
        /// Displays the app when the player clicks.
        /// </summary>
        private void DisplayApp()
        {
            Reposition();
            isShowing = true;
        }

        /// <summary>
        /// Displays the app while the player hovers.
        /// </summary>
        private void HoverApp()
        {
            Reposition();
            isHovering = true;
        }

        /// <summary>
        /// Hides the app when the player clicks a second time.
        /// </summary>
        private void HideApp()
        {
            isShowing = false;
        }

        /// <summary>
        /// Hides the app when the player unhovers.
        /// </summary>
        private void UnhoverApp()
        {
            isHovering = false;
        }

        /// <summary>
        /// Hides the app when it is disabled.
        /// </summary>
        private void Disable()
        {
            isShowing = false;
            isHovering = false;
        }

        /// <summary>
        /// Repositions the app.
        /// </summary>
        private void Reposition()
        {
            // Gets the button's anchor in 3D space.
            float anchor = kmAppButton.GetAnchor().x;

            // Adjusts the window bounds.
            windowRect = new Rect(Mathf.Min(anchor + 960.5f, Screen.width - 250f), 40f, 250f, 200f);
        }

        /// <summary>
        /// A dummy method which returns nothing.
        /// </summary>
        private void DummyVoid() { /* I do nothing!!! \('o')/ */ }

        #endregion

        #region Misc Methods

        /// <summary>
        /// Display a failure alert.
        /// </summary>
        /// <param name="moduleFor">The module for which the alert is being shown.</param>
        /// <param name="failure">The failure for which the alert is being shown.</param>
        public static void Alert(ModuleReliabilityBase moduleFor, string failure)
        {
            instance.alerting = moduleFor;
            instance.alertFailure = failure;

            if (KMSettings.Instance.highlightFailedPart) { KMUtil.SetPartHighlight(moduleFor.part, Color.red, Part.HighlightType.AlwaysOn); }
        }

        /// <summary>
        /// Returns true if the provided part reference is being alerted for.
        /// </summary>
        /// <param name="part">The part to check.</param>
        /// <returns>True if the provided part reference is being alerted for.</returns>
        public static bool IsAlertingThisPart(Part part)
        {
            if (instance.alerting == null) { return false; }
            return instance.alerting.part == part;
        }

        #endregion
    }
}