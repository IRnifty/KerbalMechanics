using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KerbalMechanics
{
    /// <summary>
    /// The ignitor reliability module. Adds ignitor failure and repair functionality to parts equipped with either ModuleEngines or ModuleEnginesFX.
    /// </summary>
    class ModuleReliabilityIgnitor : ModuleReliabilityBase
    {
        //KSP FIELDS
        #region KSP FIELDS
        /// <summary>
        /// Chance to fail while starting under perfect conditions
        /// </summary>
        [KSPField]
        public double startingChanceToFailPerfect = 0.015625f;
        /// <summary>
        /// Chance to fail while starting under the worst of conditions
        /// </summary>
        [KSPField]
        public double startingChanceToFailTerrible = 0.875f;

        /// <summary>
        /// Chance kicking the ignitor will cause it to explode.
        /// </summary>
        [KSPField]
        public float chanceKickWillDestroy = 0.025f;
        /// <summary>
        /// Chance kicking the ignitor will fix the problem.
        /// </summary>
        [KSPField]
        public float chanceKickWillFix = 0.1f;
        #endregion

        //PROPERTIES
        #region PROPERTIES
        /// <summary>
        /// Gets the crrent starting chance to fail, based on the current reliability.
        /// </summary>
        public double CurrentStartingChanceToFail
        {
            get { return startingChanceToFailPerfect + ((startingChanceToFailTerrible - startingChanceToFailPerfect) * (1f - reliability)); }
        }
        /// <summary>
        /// The name of the module. Held for use by the module injecter.
        /// </summary>
        public override string ModuleName
        {
            get { return "Ignition Coil"; }
        }
        #endregion

        //OTHER VARS
        #region OTHER VARS
        /// <summary>
        /// The thrust at last frame.
        /// </summary>
        float lastFrameThrust = 0f;
        /// <summary>
        /// The thrust of this frame
        /// </summary>
        float currFrameThrust = 0f;

        /// <summary>
        /// The engine module of this part. Null if the engine module is of type ModuleEnginesFX.
        /// </summary>
        ModuleEngines engine;

        /// <summary>
        /// The engine module of this part. Null if the engine module is of type ModuleEngines.
        /// </summary>
        ModuleEnginesFX engineFX;
        #endregion

        //KSP METHODS
        #region KSP METHODS
        /// <summary>
        /// Called when the part is started.
        /// </summary>
        /// <param name="state">The start state.</param>
        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            if (HighLogic.LoadedSceneIsFlight)
            {
                engine = part.Modules.OfType<ModuleEngines>().FirstOrDefault<ModuleEngines>();
                if (!engine)
                {
                    engineFX = part.Modules.OfType<ModuleEnginesFX>().FirstOrDefault<ModuleEnginesFX>();

                    if (!engineFX)
                    {
                        Logger.DebugError("Both engine and engineFX are NULL!");
                    }
                }

                if (engine)
                {
                    engine.Fields["status"].guiName = "Pump Status";
                }
                else if (engineFX)
                {
                    engineFX.Fields["status"].guiName = "Pump Status";
                }

                Fields["quality"].guiName = "Ignitor " + Fields["quality"].guiName;

                if (failure != "")
                {
                    BreakIgnitor(false);
                }
            }
        }

        /// <summary>
        /// Loads any additional fields not loaded automatically.
        /// </summary>
        /// <param name="node">The config node for this module.</param>
        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);

            if (node.HasValue("startingChanceToFailPerfect")) { startingChanceToFailPerfect = double.Parse(node.GetValue("startingChanceToFailPerfect")); }
            if (node.HasValue("startingChanceToFailTerrible")) { startingChanceToFailTerrible = double.Parse(node.GetValue("startingChanceToFailTerrible")); }
        }

        /// <summary>
        /// Updates the module.
        /// </summary>
        public override void OnUpdate()
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                lastFrameThrust = currFrameThrust;

                if (engine)
                {
                    currFrameThrust = engine.thrustPercentage;

                    engine.staged = engine.EngineIgnited || engine.staged;

                    if (engine.EngineIgnited && failure == "")
                    {
                        if (lastFrameThrust == 0f && currFrameThrust > 0f)
                        {
                            if (Random.Range(0f, 1f) < CurrentStartingChanceToFail)
                            {
                                BreakIgnitor(true);
                            }
                        }

                        if (timeSinceFailCheck < timeTillFailCheck)
                        {
                            timeSinceFailCheck += TimeWarp.deltaTime;
                        }
                        else
                        {
                            timeSinceFailCheck = 0f;
                            reliability -= CurrentReliabilityDrain + ((CurrentReliabilityDrain * 9) * engine.currentThrottle);
                        }
                    }
                }
                else if (engineFX)
                {
                    currFrameThrust = engineFX.thrustPercentage;

                    engineFX.staged = engineFX.EngineIgnited || engineFX.staged;

                    if (engineFX.EngineIgnited && failure == "")
                    {
                        if (currFrameThrust > 0f && lastFrameThrust == 0f)
                        {
                            if (Random.Range(0f, 1f) < CurrentStartingChanceToFail)
                            {
                                BreakIgnitor(true);
                            }
                        }

                        if (timeSinceFailCheck < timeTillFailCheck)
                        {
                            timeSinceFailCheck += TimeWarp.deltaTime;
                        }
                        else
                        {
                            timeSinceFailCheck = 0f;
                            reliability -= CurrentReliabilityDrain + ((CurrentReliabilityDrain * 9) * engineFX.currentThrottle);
                        }
                    }
                }
            }

            base.OnUpdate();
        }
        #endregion

        //KSP EVENTS
        #region KSP EVENTS
        /// <summary>
        /// Fixes the ignitor using spare parts.
        /// </summary>
        [KSPEvent(active = true, guiName = "Repair Ignitor", guiActive = false, guiActiveUnfocused = false, unfocusedRange = 3f, externalToEVAOnly = true)]
        public void FixIgnitor()
        {
            if (FlightGlobals.ActiveVessel.isEVA)
            {
                Part kerbal = FlightGlobals.ActiveVessel.parts[0];

                rocketPartsLeftToFix -= (int)kerbal.RequestResource("RocketParts", (double)System.Math.Min(rocketPartsNeededToFix, 10));

                fixSound.audio.Play();

                if (rocketPartsLeftToFix <= 0)
                {
                    FixIgnitor(false);
                }
            }
        }

        /// <summary>
        /// Kick the ignitor, producing a small chance the engine will be fixed without using spare parts, and a smaller chance the engine will just explode.
        /// </summary>
        [KSPEvent(active = true, guiName = "Kick Ignitor", guiActive = false, guiActiveUnfocused = false, unfocusedRange = 3f, externalToEVAOnly = true)]
        public void KickIgnitor()
        {
            bashSound.audio.clip = SoundManager.GetSound("Hammer" + Random.Range(1, 7).ToString());
            bashSound.audio.Play();

            float rand = Random.Range(0f, 1f);

            if (rand < chanceKickWillDestroy)
            {
                part.explode();
            }
            else if (rand > (1f - chanceKickWillFix))
            {
                FixIgnitor(true);
            }
        }

        [KSPEvent(active = false, guiActive = false, guiActiveEditor = false, guiActiveUnfocused = true, externalToEVAOnly = true, unfocusedRange = 3f, guiName = "Perform Maintenance")]
        public override void PerformMaintenance()
        {
            if (FlightGlobals.ActiveVessel.isEVA)
            {
                Part kerbal = FlightGlobals.ActiveVessel.parts[0];

                double partsGotten = kerbal.RequestResource("RocketParts", 2);

                fixSound.audio.Play();

                reliability += 0.05 * partsGotten;
                reliability = reliability.Clamp(0, 1);
            }
        }
        #endregion

        // OTHER METHODS
        #region OTHER METHODS
        /// <summary>
        /// Breaks the ignitor.
        /// </summary>
        void BreakIgnitor(bool display)
        {
            if (!broken)
            {
                failure = "Ignition Coil Burnt";

                if (engine)
                {
                    engine.Shutdown();
                    engine.Events["Activate"].guiActive = false;
                    engine.Events["Shutdown"].guiActive = false;
                    engine.Actions["OnAction"].active = false;
                    engine.Actions["ShutdownAction"].active = false;
                    engine.Actions["ActivateAction"].active = false;
                }
                else if (engineFX)
                {
                    engineFX.Shutdown();
                    engineFX.Events["Activate"].guiActive = false;
                    engineFX.Events["Shutdown"].guiActive = false;
                    engineFX.Actions["OnAction"].active = false;
                    engineFX.Actions["ShutdownAction"].active = false;
                    engineFX.Actions["ActivateAction"].active = false;
                }

                Events["FixIgnitor"].guiActiveUnfocused = true;
                Events["KickIgnitor"].guiActiveUnfocused = true;

                rocketPartsLeftToFix = rocketPartsNeededToFix;

                if (display)
                {
                    KMUtil.PostFailure(part, "'s ignition coil has burnt up!");
                }

                broken = true;
            }
        }

        /// <summary>
        /// Actually fixes the engine.
        /// </summary>
        /// <param name="kicked">Reduces the amount of reliability returned to the fixed engine if true.</param>
        public void FixIgnitor(bool kicked)
        {
            if (engine)
            {
                engine.Events["Shutdown"].guiActive = true;
                engine.Events["Activate"].guiActive = true;
                engine.Actions["OnAction"].active = true;
                engine.Actions["ShutdownAction"].active = true;
                engine.Actions["ActivateAction"].active = true;
            }
            else if (engineFX)
            {
                engineFX.Events["Shutdown"].guiActive = true;
                engineFX.Events["Activate"].guiActive = true;
                engineFX.Actions["OnAction"].active = true;
                engineFX.Actions["ShutdownAction"].active = true;
                engineFX.Actions["ActivateAction"].active = true;
            }

            Events["FixIgnitor"].guiActiveUnfocused = false;
            Events["KickIgnitor"].guiActiveUnfocused = false;

            failure = "";

            reliability += 0.2f - (kicked ? 0f : 0.15f);

            reliability = reliability.Clamp(0, 1);

            broken = false;
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
            GUILayout.Label((reliability + inaccuracySeverity).ToString("##0.00%"), HighLogic.Skin.label);
            GUILayout.Label(" ", HighLogic.Skin.label);
            GUILayout.Label(" ", HighLogic.Skin.label);
            GUILayout.Label(startingChanceToFailPerfect.ToString("##0.#####%"), HighLogic.Skin.label);
            GUILayout.Label(startingChanceToFailTerrible.ToString("##0.#####%"), HighLogic.Skin.label);
            GUILayout.Label(" ", HighLogic.Skin.label);
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
        }
        #endregion
    }
}