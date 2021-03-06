﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KerbalMechanics
{
    class ModuleReliabilityLight : ModuleReliabilityBase
    {
        //KSP FIELDS
        #region KSP FIELDS
        /// <summary>
        /// Chance to fail while running under perfect conditions
        /// </summary>
        [KSPField]
        public double chanceToFailPerfect = 0.00001f;
        /// <summary>
        /// Chance to fail while running under the worst of conditions
        /// </summary>
        [KSPField]
        public double chanceToFailTerrible = 0.001f;

        /// <summary>
        /// How many parts are needed while the light is flickering?
        /// </summary>
        [KSPField]
        public int rocketPartsNeededFlickering = 5;
        #endregion

        //PROPERTIES
        #region PROPERTIES
        /// <summary>
        /// Gets the current running chance to fail, based on the current reliability.
        /// </summary>
        public double CurrentChanceToFail
        {
            get { return chanceToFailPerfect + ((chanceToFailTerrible - chanceToFailPerfect) * (1f - reliability)); }
        }
        /// <summary>
        /// The name of the module. Held for use by the module injecter.
        /// </summary>
        public override string ModuleName
        {
            get { return "Light"; }
        }
        #endregion

        //OTHER VARS
        #region OTHER VARS
        /// <summary>
        /// Is this light flickering?
        /// </summary>
        bool isFlickering = false;

        /// <summary>
        /// The maximum time this light will flicker. Randomized 5f - 30f.
        /// </summary>
        float maxOverallFlickeringTime = 3f;
        /// <summary>
        /// The current elapsed flickering time.
        /// </summary>
        float currentOverallFlickeringTime = 0f;

        /// <summary>
        /// The maximum time this individual flicker will last. Randomized 0.1f ± 0.05f.
        /// </summary>
        float maxFlickerTime = 0.1f;
        /// <summary>
        /// The current elapsed time of this individual flicker.
        /// </summary>
        float currentFlickerTime = 0f;

        /// <summary>
        /// The light module.
        /// </summary>
        ModuleLight mLight;
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

            if (HighLogic.LoadedSceneIsFlight)
            {
                mLight = part.Modules.OfType<ModuleLight>().FirstOrDefault<ModuleLight>();

                if (!mLight)
                {
                    Logger.DebugError("mLight is NULL!");
                }

                if (failure != "")
                {
                    BustBulb(false);
                }

                Fields["quality"].guiName = "Light " + Fields["quality"].guiName;
            }
        }

        /// <summary>
        /// Loads any additional fields not loaded automatically.
        /// </summary>
        /// <param name="node">The config node for this module.</param>
        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);

            if (node.HasValue("chanceToFailPerfect")) { chanceToFailPerfect = double.Parse(node.GetValue("chanceToFailPerfect")); }
            if (node.HasValue("chanceToFailTerrible")) { chanceToFailTerrible = double.Parse(node.GetValue("chanceToFailTerrible")); }
        }

        /// <summary>
        /// Updates the module.
        /// </summary>
        public override void OnUpdate()
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                if (mLight.isOn)
                {
                    if (failure == "Busted Light Bulb")
                    {
                        mLight.LightsOff();
                    }
                    else if (timeSinceFailCheck < timeTillFailCheck)
                    {
                        timeSinceFailCheck += TimeWarp.deltaTime;
                    }
                    else
                    {
                        timeSinceFailCheck = 0f;
                        reliability -= CurrentReliabilityDrain;

                        if (Random.Range(0f, 1f) < CurrentChanceToFail)
                        {
                            BeginFlickering();
                        }
                    }
                }

                if (isFlickering)
                {
                    if (currentFlickerTime < maxFlickerTime)
                    {
                        currentFlickerTime += TimeWarp.deltaTime;
                    }
                    else
                    {
                        currentFlickerTime = 0f;
                        FlickerBulb();
                    }

                    if (currentOverallFlickeringTime < maxOverallFlickeringTime)
                    {
                        currentOverallFlickeringTime += TimeWarp.deltaTime;
                    }
                    else
                    {
                        BustBulb(true);
                    }
                }
            }

            base.OnUpdate();
        }
        #endregion

        //KSP EVENTS
        #region KSP EVENTS
        /// <summary>
        /// Replaces the light bulb, raising reliability back to 100%.
        /// </summary>
        [KSPEvent(active = true, guiName = "Replace Bulb", guiActive = false, guiActiveUnfocused = true, unfocusedRange = 3f, externalToEVAOnly = true)]
        public void RelpaceBulb()
        {
            if (FlightGlobals.ActiveVessel.isEVA)
            {
                if (!KMUtil.IsModeCareer || CanRepair)
                {
                    Part kerbal = FlightGlobals.ActiveVessel.parts[0];

                    rocketPartsLeftToFix -= (int)kerbal.RequestResource("RocketParts", (double)System.Math.Min(rocketPartsLeftToFix, 10));

                    fixSound.audio.Play();

                    if (rocketPartsLeftToFix <= 0)
                    {
                        mLight.Events["LightsOff"].guiActive = true;
                        mLight.Events["LightsOn"].guiActive = true;

                        reliability = 1f;

                        broken = false;
                    }
                }
            }
        }

        [KSPEvent(active = false, guiActive = false, guiActiveEditor = false, guiActiveUnfocused = false, externalToEVAOnly = true, unfocusedRange = 3f, guiName = "Perform Maintenance")]
        public override void PerformMaintenance()
        {
            
        }
        #endregion

        //OTHER METHODS
        #region OTHER METHODS
        /// <summary>
        /// Starts the flickering cycle.
        /// </summary>
        void BeginFlickering()
        {
            if (!broken)
            {
                maxOverallFlickeringTime = Random.Range(5f, 30f);
                mLight.Events["LightsOff"].guiActive = false;
                mLight.Events["LightsOn"].guiActive = false;
                rocketPartsLeftToFix = rocketPartsNeededFlickering;
                isFlickering = true;
                failure = "Flickering Light Bulb";
                KMUtil.PostFailure(this, "'s light has begun flickering. Replace soon.");
                broken = true;
            }
        }

        /// <summary>
        /// Flickers the light bulb once.
        /// </summary>
        void FlickerBulb()
        {
            if (mLight.isOn)
            {
                mLight.LightsOff();
                maxFlickerTime = Random.Range(0.1f, 0.4f);
            }
            else
            {
                mLight.LightsOn();
                maxFlickerTime = Random.Range(0.5f, 5f);
            }
        }

        /// <summary>
        /// Busts the light bulb.
        /// </summary>
        void BustBulb(bool display)
        {
            isFlickering = false;
            mLight.LightsOff();
            rocketPartsLeftToFix = rocketPartsNeededToFix;
            failure = "Busted Light Bulb";

            if (display)
            {
                KMUtil.PostFailure(this, " has busted its light bulb.");
            }

            broken = true;
        }

        /// <summary>
        /// Gets the reliability information on this module.
        /// </summary>
        public override void DisplayDesc(double inaccuracySeverity)
        {
            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical();
            GUILayout.Label("Reliability:", HighLogic.Skin.label);
            GUILayout.Label(" ", HighLogic.Skin.label);
            GUILayout.Label("Chances of failure:", HighLogic.Skin.label);
            GUILayout.Label("@100%:", HighLogic.Skin.label);
            GUILayout.Label("@0%:", HighLogic.Skin.label);
            GUILayout.Label(" ", HighLogic.Skin.label);
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            GUILayout.Label((reliability + inaccuracySeverity).ToString("##0.#%"), HighLogic.Skin.label);
            GUILayout.Label(" ", HighLogic.Skin.label);
            GUILayout.Label(" ", HighLogic.Skin.label);
            GUILayout.Label(chanceToFailPerfect.ToString("##0.#####%"), HighLogic.Skin.label);
            GUILayout.Label(chanceToFailTerrible.ToString("##0.#####%"), HighLogic.Skin.label);
            GUILayout.Label(" ", HighLogic.Skin.label);
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
        }
        #endregion
    }
}