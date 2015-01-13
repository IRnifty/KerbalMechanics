using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KerbalMechanics
{
    class ModuleReliabilityManager : PartModule, IPartCostModifier
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
        /// Whether or not changes to this module also affect the modules of any symetrical counterparts.
        /// </summary>
        public bool affectingSymetryCounterparts = false;
        /// <summary>
        /// A last-frame reference of the above.
        /// </summary>
        public bool prevAffectingSmetryCounterparts = false;
        /// <summary>
        /// The symetry counterparts.
        /// </summary>
        ModuleReliabilityManager[] symetryCounterparts = new ModuleReliabilityManager[] {};

        /// <summary>
        /// The severity of the inaccuracy that will affect this part's reliability displays.
        /// </summary>
        double inaccuracy = 0;
        /// <summary>
        /// The current time since the last random change in inaccuracy severity.
        /// </summary>
        float currentSeverityTime = 0f;
        /// <summary>
        /// The current interval until the next random change in inaccuracy severity.
        /// </summary>
        float currentSeverityInterval = 5f;
        /// <summary>
        /// The minimum interval until the next random change in inaccuracy severity.
        /// </summary>
        float minSeverityInterval = 0.25f;
        /// <summary>
        /// The maximum interval until the next random change in inaccuracy severity.
        /// </summary>
        float maxSeverityInterval = 5f;

        /// <summary>
        /// The cost of this part if all reliability modules had 0% quality.
        /// </summary>
        float allTerribleCost;
        /// <summary>
        /// The cost of this part if all reliability modules had 75% quality.
        /// </summary>
        float allDefaultCost;
        /// <summary>
        /// The change in cost per 1% quality
        /// </summary>
        float changePerPercent;
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

            showingGUI = false;
            Events["ShowGUI"].active = true;

            allDefaultCost = part.partInfo.cost;
            allTerribleCost = allDefaultCost * 0.5f;
            changePerPercent = (allDefaultCost - allTerribleCost) / 75 / rModuleList.Length;

            if (HighLogic.LoadedSceneIsEditor)
            {
                GameEvents.onEditorShipModified.Fire(EditorLogic.fetch.ship);
            }
        }

        /// <summary>
        /// Called when the part is updated.
        /// </summary>
        public override void OnUpdate()
        {
            base.OnUpdate();

            if (HighLogic.LoadedSceneIsEditor) // Editor scene doesn't use OnUpdate anymore???
            {
                
            }
            else if (HighLogic.LoadedSceneIsFlight)
            {
                if (currentSeverityTime < currentSeverityInterval)
                {
                    currentSeverityTime += TimeWarp.deltaTime;
                }
                else
                {
                    currentSeverityTime = 0f;
                    currentSeverityInterval = UnityEngine.Random.Range(minSeverityInterval, maxSeverityInterval);
                    if (HighLogic.LoadedSceneIsFlight)
                    {
                        if (InstrumentReliabilityManager.Instance.averageMonitorReliability < 0.5)
                        {
                            double severity = (0.5 - InstrumentReliabilityManager.Instance.averageMonitorReliability) / 2;

                            inaccuracy = UnityEngine.Random.Range((float)-severity, (float)severity);
                        }
                        else
                        {
                            inaccuracy = 0;
                        }
                    }
                }

                bool shouldBeRed = false;

                foreach (ModuleReliabilityBase mRel in rModuleList)
                {
                    if (mRel.failure != "")
                    {
                        shouldBeRed = true;
                        break;
                    }
                }

                if (InstrumentReliabilityManager.Instance.highlightFailedParts)
                {
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
                else if (InstrumentReliabilityManager.Instance.highlightingReliability)
                {
                    KMUtil.SetPartHighlight(part, KMUtil.GetReliabilityColor(Mathf.Clamp01((float) (GetAverageReliability() + inaccuracy))), Part.HighlightType.AlwaysOn);
                }
                else if (part.highlightColor != KMUtil.KerbalGreen)
                {
                    KMUtil.SetPartHighlight(part, KMUtil.KerbalGreen, Part.HighlightType.OnMouseOver);
                }
            }
        }

        /// <summary>
        /// Gets the module's cost. Can be negative to reduce part cost.
        /// </summary>
        /// <returns>Returns the module's cost. Positive or negative based on overall quality.</returns>
        public float GetModuleCost(float defaultCost)
        {
            try
            {
                return CalculateCost() - defaultCost;
            }
            catch
            {
                return 0f;
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
        /// <summary>
        /// Called when the GUI is drawn.
        /// </summary>
        void OnGUI()
        {
            if (HighLogic.LoadedSceneIsEditor)
            {
                if (part.symmetryCounterparts.Count != symetryCounterparts.Length) // TODO: Look for potential exceptions happening in the editor?
                {
                    symetryCounterparts = new ModuleReliabilityManager[part.symmetryCounterparts.Count];

                    for (int i = 0; i < symetryCounterparts.Length; i++)
                    {
                        symetryCounterparts[i] = part.symmetryCounterparts[i].Modules.OfType<ModuleReliabilityManager>().FirstOrDefault<ModuleReliabilityManager>();
                    }
                }
            }

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
        /// Calculates the cost of the part.
        /// </summary>
        /// <returns>Returns the cost of the part.</returns>
        public float CalculateCost()
        {
            float currentCost = allTerribleCost;
            foreach (ModuleReliabilityBase rMod in rModuleList)
            {
                currentCost += changePerPercent * (rMod.quality * 100f);
            }
            return Mathf.Round(currentCost);
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

        public double GetAverageReliability()
        {
            double toRet = 0;
            foreach(ModuleReliabilityBase rMod in rModuleList)
            {
                toRet += rMod.reliability;
            }
            toRet /= rModuleList.Length;

            return toRet;
        }

        public void ModifyQuality(Type t, float quality)
        {
            Logger.DebugLog(t.ToString());

            ModuleReliabilityBase m = null;
            int index = -1;

            for (int i = 0; i < rModuleList.Length; i++)
            {
                if (rModuleList[i].GetType().Equals(t))
                {
                    m = rModuleList[i];
                    index = i;
                    break;
                }
            }
            if (m == null)
            {
                Logger.DebugError("Symetrical module couldn't be found!");
                return;
            }

            m.quality = quality;
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
                        float newVal = GUILayout.HorizontalSlider(rModuleList[i].quality, 0f, 1f, HighLogic.Skin.horizontalSlider, HighLogic.Skin.horizontalSliderThumb);
                        GUILayout.Label(newVal.ToString("##0%"), HighLogic.Skin.label, GUILayout.Width(40f));

                        if (newVal != rModuleList[i].quality)
                        {
                            rModuleList[i].quality = Mathf.Clamp01(newVal);

                            if (affectingSymetryCounterparts)
                            {
                                foreach (ModuleReliabilityManager m in symetryCounterparts)
                                {
                                    m.ModifyQuality(rModuleList[i].GetType(), rModuleList[i].quality);
                                }
                            }

                            GameEvents.onEditorShipModified.Fire(EditorLogic.fetch.ship);
                        }
                    }
                    else
                    {
                        GUILayout.Label(rModuleList[i].quality.ToString("P0"), HighLogic.Skin.label);
                    }

                    GUILayout.EndHorizontal();

                    //Actual Info
                    rModuleList[i].DisplayDesc(inaccuracy);
                }
            }

            GUILayout.EndScrollView();

            if (HighLogic.LoadedSceneIsEditor)
            {
                //Disable symetry checkbox if there are no symetry counterparts.
                if (part.symmetryCounterparts.Count == 0)
                {
                    GUI.enabled = false;
                    affectingSymetryCounterparts = false;
                    prevAffectingSmetryCounterparts = false;
                }

                affectingSymetryCounterparts = GUILayout.Toggle(affectingSymetryCounterparts, "Affect symmetrical parts (" + symetryCounterparts.Length.ToString() + " others)", HighLogic.Skin.toggle);
                if (affectingSymetryCounterparts != prevAffectingSmetryCounterparts)
                {
                    foreach (ModuleReliabilityManager m in symetryCounterparts)
                    {
                        m.affectingSymetryCounterparts = affectingSymetryCounterparts;
                        m.prevAffectingSmetryCounterparts = affectingSymetryCounterparts;
                    }
                }

                GUI.enabled = true;

                prevAffectingSmetryCounterparts = affectingSymetryCounterparts;
            }
            else if (HighLogic.LoadedSceneIsFlight && KMUtil.DebugDeclared)
            {
                GUILayout.BeginVertical();

                GUILayout.Label("Average Reliability: " + GetAverageReliability().ToString("##0.00%"), HighLogic.Skin.label);
                GUILayout.Label("Inaccurate Reliability: " + (GetAverageReliability() + inaccuracy).ToString("##0.00%"), HighLogic.Skin.label);

                GUILayout.EndVertical();
            }

            //Drag Window
            GUI.DragWindow();
        }
        #endregion
    }
}