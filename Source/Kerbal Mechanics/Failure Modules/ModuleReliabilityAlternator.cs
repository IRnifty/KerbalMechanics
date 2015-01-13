using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KerbalMechanics
{
    class ModuleReliabilityAlternator : ModuleReliabilityBase
    {
        //KSP FIELDS
        #region KSP FIELDS
        /// <summary>
        /// The chance this part's alternator has to fail in perfect condition under normal g forces.
        /// </summary>
        [KSPField]
        public double idleChanceToFailPerfect = 0.000001;
        /// <summary>
        /// The chance this part's alternator has to fail in terrible condition under normal g forces.
        /// </summary>
        [KSPField]
        public double idleChanceToFailTerrible = 0.00025;

        /// <summary>
        /// The chance this part's alternator has to fail in perfect condition when its breaking g force is reached.
        /// </summary>
        [KSPField]
        public double stressedChanceToFailPerfect = 0.1;

        /// <summary>
        /// The chance this part's alternator has to fail in terrible condition when its breaking g force is reached.
        /// </summary>
        [KSPField]
        public double stressedChanceToFailTerrible = 0.5;

        /// <summary>
        /// Maximum g force the vessel can take before the altimeter breaks with perfect reliability.
        /// </summary>
        [KSPField]
        public double maxGeesPerfect = 15;
        /// <summary>
        /// Maximum g force the vessel can take before the altimeter breaks with terrible reliability.
        /// </summary>
        [KSPField]
        public double maxGeesTerrible = 10;
        #endregion

        //PROPERTIES
        #region PROPERTIES
        /// <summary>
        /// Gets the current chance to fail, based on current relibility under high g forces.
        /// </summary>
        public double CurrentIdleChanceToFail
        {
            get { return idleChanceToFailPerfect + ((idleChanceToFailTerrible - idleChanceToFailPerfect) * (1f - reliability)); }
        }
        /// <summary>
        /// Gets the current chance to fail, based on current relibility under high g forces.
        /// </summary>
        public double CurrentStressedChanceToFail
        {
            get { return stressedChanceToFailPerfect + ((stressedChanceToFailTerrible - stressedChanceToFailPerfect) * (1f - reliability)); }
        }

        /// <summary>
        /// Gets the current maximum g force this altimeter module can take before attempting to break.
        /// </summary>
        public double CurrentMaxGees
        {
            get { return maxGeesTerrible + ((maxGeesPerfect - maxGeesTerrible) * (1f - reliability)); }
        }

        /// <summary>
        /// Gets the number of gees this vessel is over by, multiplied times ten.
        /// </summary>
        double CurrentOverGees
        {
            get { return System.Math.Max(vessel.geeForce - 5, 0) * 10; }
        }

        /// <summary>
        /// Gets the module name.
        /// </summary>
        public override string ModuleName
        {
            get { return "Alternator"; }
        }
        #endregion

        //OTHER VARS
        #region OTHER VARS
        /// <summary>
        /// This part's alternator.
        /// </summary>
        ModuleAlternator alternator;
        #endregion

        //KSP METHODS
        #region KSP METHODS
        /// <summary>
        /// Called when this module is started.
        /// </summary>
        /// <param name="state"></param>
        public override void OnStart(PartModule.StartState state)
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                alternator = part.Modules.OfType<ModuleAlternator>().FirstOrDefault<ModuleAlternator>();

                if (!alternator)
                {
                    Logger.DebugError("Part \"" + part.partInfo.name + "\" has no alternator!");
                    return;
                }

                if (failure != "")
                {
                    BreakAlternator(false);
                }
            }
            
            base.OnStart(state);
        }

        /// <summary>
        /// Loads any additional fields not loaded automatically.
        /// </summary>
        /// <param name="node">The config node for this module.</param>
        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);

            if (node.HasValue("idleChanceToFailPerfect")) { idleChanceToFailPerfect = double.Parse(node.GetValue("idleChanceToFailPerfect")); }
            if (node.HasValue("idleChanceToFailTerrible")) { idleChanceToFailTerrible = double.Parse(node.GetValue("idleChanceToFailTerrible")); }
            if (node.HasValue("stressedChanceToFailPerfect")) { stressedChanceToFailPerfect = double.Parse(node.GetValue("stressedChanceToFailPerfect")); }
            if (node.HasValue("stressedChanceToFailTerrible")) { stressedChanceToFailTerrible = double.Parse(node.GetValue("stressedChanceToFailTerrible")); }
        }

        /// <summary>
        /// Updates the module.
        /// </summary>
        public override void OnUpdate()
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                if (timeSinceFailCheck < timeTillFailCheck)
                {
                    timeSinceFailCheck += TimeWarp.deltaTime;
                }
                else
                {
                    timeSinceFailCheck = 0f;
                    reliability -= CurrentReliabilityDrain + (CurrentReliabilityDrain * CurrentOverGees) + (CurrentReliabilityDrain * 5 * FlightInputHandler.state.mainThrottle);

                    if (UnityEngine.Random.Range(0f, 1f) < CurrentIdleChanceToFail * FlightInputHandler.state.mainThrottle)
                    {
                        BreakAlternator(true);
                    }
                }

                if (vessel.geeForce > CurrentMaxGees)
                {
                    if (UnityEngine.Random.Range(0f, 1f) < CurrentStressedChanceToFail * TimeWarp.deltaTime)
                    {
                        BreakAlternator(true);
                    }
                }
            }
            
            base.OnUpdate();
        }
        #endregion

        //KSP EVENTS
        #region KSP EVENTS
        /// <summary>
        /// Fixes the alternator.
        /// </summary>
        [KSPEvent(active = true, guiActive = false, guiActiveEditor = false, guiActiveUnfocused = false, externalToEVAOnly = true, unfocusedRange = 3f, guiName = "Fix Alternator")]
        public void FixAlternator()
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
                        failure = "";
                        reliability += 0.25;
                        reliability = reliability.Clamp(0, 1);

                        Events["FixAlternator"].guiActiveUnfocused = true;

                        broken = false;
                    }
                }
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

        //OTHER METHODS
        #region OTHER METHODS

        void BreakAlternator(bool display)
        {
            if (alternator && !broken)
            {
                alternator.enabled = false;
                failure = "Alternator Failure";
                rocketPartsLeftToFix = rocketPartsNeededToFix;

                Events["FixAlternator"].guiActiveUnfocused = true;

                if (display)
                {
                    KMUtil.PostFailure(part, "has a burned out its alternator!");
                }
                broken = true;
            }
        }

        /// <summary>
        /// Displays the reliability information on this module.
        /// </summary>
        public override void DisplayDesc(double inaccuracySeverity)
        {
            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical();
            GUILayout.Label("Reliability:", HighLogic.Skin.label);
            GUILayout.Label(" ", HighLogic.Skin.label);
            GUILayout.Label("G Force Threshold:", HighLogic.Skin.label);
            GUILayout.Label("@100%:", HighLogic.Skin.label);
            GUILayout.Label("@0%:", HighLogic.Skin.label);
            GUILayout.Label(" ", HighLogic.Skin.label);
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            GUILayout.Label((reliability + inaccuracySeverity).ToString("##0.00%"), HighLogic.Skin.label);
            GUILayout.Label(" ", HighLogic.Skin.label);
            GUILayout.Label(" ", HighLogic.Skin.label);
            GUILayout.Label(maxGeesPerfect.ToString("#0.#g"), HighLogic.Skin.label);
            GUILayout.Label(maxGeesTerrible.ToString("#0.#g"), HighLogic.Skin.label);
            GUILayout.Label(" ", HighLogic.Skin.label);
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
        }
        #endregion
    }
}
