using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Kerbal_Mechanics
{
    /// <summary>
    /// The cooling reliability module. Adds cooling system failure and repair functionality to parts equipped with either ModuleEngines or ModuleEnginesFX.
    /// </summary>
    class ModuleReliabilityCooling : ModuleReliabilityBase
    {
        //CONSTANTS AND STATICS
        #region CONSTANTS AND STATICS
        /// <summary>
        /// Time until the next running failure check.
        /// </summary>
        static readonly float timeTillFailCheck = 10f;
        #endregion

        //FAILURE CHANCES
        #region FAILURE CHANCES
        /// <summary>
        /// Chance to fail while running under perfect conditions
        /// </summary>
        [KSPField]
        public float runningChanceToFailPerfect = 0.000001f;
        /// <summary>
        /// Chance to fail while running under the worst of conditions
        /// </summary>
        [KSPField]
        public float runningChanceToFailTerrible = 0.01f;

        /// <summary>
        /// Chance kicking the cooling will cause it to explode.
        /// </summary>
        [KSPField]
        public float chanceKickWillDestroy = 0.025f;
        /// <summary>
        /// Chance kicking the cooling will fix the problem.
        /// </summary>
        [KSPField]
        public float chanceKickWillFix = 0.1f;
        #endregion

        //PROPERTIES
        #region PROPERTIES
        /// <summary>
        /// Gets the current running chance to fail, based on the current reliability.
        /// </summary>
        public float CurrentRunningChanceToFail
        {
            get { return runningChanceToFailPerfect + ((runningChanceToFailTerrible - runningChanceToFailPerfect) * (1f - reliability)); }
        }
        /// <summary>
        /// The name of the module. Held for use by the module injecter.
        /// </summary>
        public override string ModuleName
        {
            get { return "Cooling System"; }
        }
        #endregion

        //OTHER VARS
        #region OTHER VARS
        /// <summary>
        /// Time since the last running failure check.
        /// </summary>
        float timeSinceFailCheck = 0f;

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

            Fields["quality"].guiName = "Cooling System " + Fields["quality"].guiName;
        }

        /// <summary>
        /// Updates the module.
        /// </summary>
        public override void OnUpdate()
        {
            base.OnUpdate();

            if (engine)
            {
                engine.staged = engine.EngineIgnited || engine.staged;

                if (engine.EngineIgnited && failure == "")
                {
                    if (timeSinceFailCheck < timeTillFailCheck)
                    {
                        timeSinceFailCheck += TimeWarp.deltaTime;
                    }
                    else
                    {
                        timeSinceFailCheck = 0f;
                        reliability -= CurrentReliabilityDrain * engine.currentThrottle;

                        if (Random.Range(0f, 1f) < CurrentRunningChanceToFail)
                        {
                            BreakCooling();
                        }
                    }
                }

                if (failure != "")
                {
                    part.temperature += engine.heatProduction * engine.currentThrottle * TimeWarp.deltaTime;
                }
            }
            else if (engineFX)
            {
                engineFX.staged = engineFX.EngineIgnited || engineFX.staged;

                if (engineFX.EngineIgnited && !CheatOptions.InfiniteFuel && failure == "")
                {
                    if (timeSinceFailCheck < timeTillFailCheck)
                    {
                        timeSinceFailCheck += TimeWarp.deltaTime;
                    }
                    else
                    {
                        timeSinceFailCheck = 0f;
                        reliability -= CurrentReliabilityDrain * engineFX.currentThrottle;

                        if (Random.Range(0f, 1f) < CurrentRunningChanceToFail)
                        {
                            BreakCooling();
                        }
                    }
                }

                if (failure != "")
                {
                    part.temperature += engineFX.heatProduction * engineFX.currentThrottle * TimeWarp.deltaTime;
                }
            }
        }
        #endregion

        //KSP EVENTS
        #region KSP EVENTS
        /// <summary>
        /// Fixes the cooling using spare parts.
        /// </summary>
        [KSPEvent(active = true, guiName = "Repair Cooling", guiActive = false, guiActiveUnfocused = true, unfocusedRange = 3f, externalToEVAOnly = true)]
        public void FixCooling()
        {
            if (FlightGlobals.ActiveVessel.isEVA)
            {
                Part kerbal = FlightGlobals.ActiveVessel.parts[0];

                rocketPartsLeftToFix -= (int)kerbal.RequestResource("RocketParts", (double)System.Math.Min(rocketPartsLeftToFix, 10));

                fixSound.audio.Play();

                if (rocketPartsLeftToFix <= 0)
                {
                    FixCooling(false);
                }
            }
        }

        /// <summary>
        /// Kick the engine, producing a small chance the engine will be fixed without using spare parts, and a smaller chance the engine will just explode.
        /// </summary>
        [KSPEvent(active = true, guiName = "Kick Cooling Pump", guiActive = false, guiActiveUnfocused = true, unfocusedRange = 3f, externalToEVAOnly = true)]
        public void KickCooling()
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
                FixCooling(true);
            }
        }
        #endregion

        // OTHER METHODS
        #region OTHER METHODS
        /// <summary>
        /// Breaks the cooling.
        /// </summary>
        void BreakCooling()
        {
            failure = "Cooling Failure";

            Events["FixCooling"].guiActiveUnfocused = true;
            Events["KickCooling"].guiActiveUnfocused = true;

            rocketPartsLeftToFix = rocketPartsNeededToFix;

            KMUtil.PostFailure(part, " has had a cooling system failure!");
        }

        /// <summary>
        /// Actually fixes the cooling.
        /// </summary>
        /// <param name="kicked">Reduces the amount of reliability returned to the fixed cooling if true.</param>
        public void FixCooling(bool kicked)
        {
            Events["FixEngine"].guiActiveUnfocused = false;
            Events["KickEngine"].guiActiveUnfocused = false;

            failure = "";

            reliability += 0.2f - (kicked ? 0f : 0.15f);

            reliability = Mathf.Min(reliability, 1f);
        }

        /// <summary>
        /// Gets the reliability information on this module.
        /// </summary>
        public override void DisplayDesc()
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
            GUILayout.Label(reliability.ToString("##0.#%"), HighLogic.Skin.label);
            GUILayout.Label(" ", HighLogic.Skin.label);
            GUILayout.Label(" ", HighLogic.Skin.label);
            GUILayout.Label(runningChanceToFailPerfect.ToString("##0.#####%"), HighLogic.Skin.label);
            GUILayout.Label(runningChanceToFailTerrible.ToString("##0.#####%"), HighLogic.Skin.label);
            GUILayout.Label(" ", HighLogic.Skin.label);
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
        }
        #endregion
    }
}