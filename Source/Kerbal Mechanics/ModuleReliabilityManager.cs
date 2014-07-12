using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Kerbal_Mechanics
{
    class ModuleReliabilityManager : PartModule
    {
        //OTHER VARS
        #region OTHER VARS
        /// <summary>
        /// The array of reliability modules, excluding this one, placed on this part.
        /// </summary>
        ModuleReliabilityBase[] rModuleList;
        /// <summary>
        /// An array of boolean values corresponding to the array of reliability modules. If a value is true, the GUI window will display additional information for that module.
        /// </summary>
        bool[] displayingModule;

        /// <summary>
        /// If true, shows a GUI displaying stats and, if in the editor, some editable fields such as quality.
        /// </summary>
        bool showingGUI = false;
        /// <summary>
        /// The GUI window rect.
        /// </summary>
        Rect windowRect = new Rect(256f, 26f, 350f, 600f);
        /// <summary>
        /// The GUI scroll position.
        /// </summary>
        Vector2 scrollPos = Vector2.zero;
        /// <summary>
        /// The string used as an intermediary between the text field and the quality variable.
        /// </summary>
        string[] qualityString;
        #endregion

        //KSP METHODS
        #region KSP METHODS
        /// <summary>
        /// Called when the part is started.
        /// </summary>
        /// <param name="state">The start state.</param>
        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);

            if (rModuleList == null)
            {
                PreStart();
            }

            qualityString = new string[rModuleList.Length];

            for (int i = 0; i < rModuleList.Length; i++)
            {
                qualityString[i] = (rModuleList[i].quality * 100f).ToString();
            }

            showingGUI = false;
            Events["ShowGUI"].active = true;
        }

        /// <summary>
        /// Called when the part is updated.
        /// </summary>
        public override void OnUpdate()
        {
            base.OnUpdate();

            bool shouldBeRed = false;

            foreach (ModuleReliabilityBase mRel in rModuleList)
            {
                if (mRel.failure != "")
                {
                    shouldBeRed = true;
                }
            }

            if (shouldBeRed)
            {
                if (part.highlightColor != Color.red)
                {
                    KMUtil.SetPartHighlight(part, Color.red, Part.HighlightType.AlwaysOn);
                }
            }
            else if (part.highlightColor != KMUtil.KerbalGreen)
            {
                KMUtil.SetPartHighlight(part, KMUtil.KerbalGreen, Part.HighlightType.OnMouseOver);
            }
        }
        #endregion

        //KSP EVENTS
        #region KSP EVENTS
        [KSPEvent(active = true, guiActive = true, guiActiveEditor = true, guiName = "Reliability Stats")]
        public void ShowGUI()
        {
            showingGUI = true;
            Events["ShowGUI"].active = false;
        }
        #endregion

        //UNITY METHODS
        #region UNITY METHODS
        void OnGUI()
        {
            if (showingGUI && (HighLogic.LoadedSceneIsEditor || HighLogic.LoadedSceneIsFlight))
            {
                windowRect = GUI.Window(GetInstanceID(), windowRect, GUIWindow, "Reliability Information", HighLogic.Skin.window);
            }
        }
        #endregion

        //OTHER METHODS
        #region OTHER METHODS
        /// <summary>
        /// Pre starts the module, filling its list of Reliability modules for display in GetDesc().
        /// </summary>
        public void PreStart()
        {
            rModuleList = part.Modules.OfType<ModuleReliabilityBase>().ToArray<ModuleReliabilityBase>();

            displayingModule = new bool[rModuleList.Length];
        }

        /// <summary>
        /// Gets the description of the monitored Reliability Modules.
        /// </summary>
        /// <returns>The description.</returns>
        public string GetDesc()
        {
            string info = base.GetInfo();

            info += "<b>Modules that can fail</b>:";
            info += KMUtil.NewLine;

            foreach (ModuleReliabilityBase mRel in rModuleList)
            {
                info += mRel.ModuleName + KMUtil.NewLine;
            }

            return info;
        }

        void GUIWindow(int windowID)
        {
            //X Button
            if (GUI.Button(new Rect(windowRect.width - 25f, 5f, 20f, 20f), "X", HighLogic.Skin.button))
            {
                showingGUI = false;
                Events["ShowGUI"].active = true;
            }

            GUILayout.Label(part.partInfo.title, HighLogic.Skin.label);

            scrollPos = GUILayout.BeginScrollView(scrollPos, HighLogic.Skin.scrollView);

            for (int i = 0; i < rModuleList.Length; i++)
            {
                //Module Title
                GUILayout.BeginHorizontal();
                
                if (GUILayout.Button((displayingModule[i] ? "-" : "+"), HighLogic.Skin.button, GUILayout.Width(20f), GUILayout.Height(20f)))
                {
                    displayingModule[i] = !displayingModule[i];
                }
                GUILayout.Label(rModuleList[i].ModuleName, HighLogic.Skin.label);
                if (rModuleList[i].failure != "")
                {
                    GUILayout.Label("FAILURE: " + rModuleList[i].failure, StyleManager.GetStyle("GUI - Red"));
                }
                GUILayout.EndHorizontal();

                //Module Content
                if (displayingModule[i])
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Quality: ", HighLogic.Skin.label);

                    if (HighLogic.LoadedSceneIsEditor)
                    {
                        qualityString[i] = GUILayout.TextField(qualityString[i], 3, HighLogic.Skin.textField, GUILayout.MaxWidth(40f));
                        qualityString[i] = Regex.Replace(qualityString[i], @"[^0-9]", "");
                        float testFloat = 0f;
                        if (float.TryParse(qualityString[i], out testFloat))
                        {
                            rModuleList[i].quality = Mathf.Clamp01(testFloat / 100f);
                            qualityString[i] = (rModuleList[i].quality * 100f).ToString();
                        }

                        GUILayout.Label("%", HighLogic.Skin.label);
                    }
                    else
                    {
                        GUILayout.Label(rModuleList[i].quality.ToString("P1"), HighLogic.Skin.label);
                    }

                    GUILayout.EndHorizontal();

                    //Actual Info
                    rModuleList[i].DisplayDesc();
                }
            }

            GUILayout.EndScrollView();

            //Drag Window
            GUI.DragWindow();
        }
        #endregion
    }
}