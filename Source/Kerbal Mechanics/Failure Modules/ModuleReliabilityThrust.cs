using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KerbalMechanics
{
    class ModuleReliabilityThrust : ModuleReliabilityInstrument
    {
        //PROPERTIES
        #region PROPERTIES
        /// <summary>
        /// The module name.
        /// </summary>
        public override string ModuleName
        {
            get { return "Thrust Gauge"; }
        }
        #endregion

        //KSP METHODS
        #region KSP METHODS
        /// <summary>
        /// Called when this module is started.
        /// </summary>
        /// <param name="state">The start state.</param>
        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);

            if (HighLogic.LoadedSceneIsFlight)
            {
                if (failure != "")
                {
                    BreakThrustGauge(false);
                }
            }
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
                    reliability -= CurrentReliabilityDrain;
                }

                if (vessel.geeForce > CurrentMaxGees)
                {
                    reliability -= CurrentReliabilityDrain * (vessel.geeForce - CurrentMaxGees) * TimeWarp.deltaTime;

                    if (UnityEngine.Random.Range(0f, 1f) < CurrentChanceToFail * TimeWarp.deltaTime)
                    {
                        BreakThrustGauge(true);
                    }
                }
            }

            base.OnUpdate();
        }
        #endregion

        //KSP EVENTS
        #region KSP EVENTS
        /// <summary>
        /// Fixes the thrust gauge.
        /// </summary>
        [KSPEvent(active = true, guiActive = false, guiActiveEditor = false, guiActiveUnfocused = false, externalToEVAOnly = true, unfocusedRange = 3f, guiName = "Fix Thrust Gauge")]
        public void FixThrustGauge()
        {
            if (FlightGlobals.ActiveVessel.isEVA)
            {
                Part kerbal = FlightGlobals.ActiveVessel.parts[0];

                rocketPartsLeftToFix -= (int)kerbal.RequestResource("RocketParts", (double)System.Math.Min(rocketPartsLeftToFix, 10));

                fixSound.audio.Play();

                if (rocketPartsLeftToFix <= 0)
                {
                    failure = "";
                    reliability += 0.25;
                    reliability = reliability.Clamp(0, 1);

                    Events["FixThrustGauge"].guiActiveUnfocused = false;

                    broken = false;
                }
            }
        }

        /// <summary>
        /// Performs preventative maintenance on the module.
        /// </summary>
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
        /// <summary>
        /// Breaks the thrust gauge.
        /// </summary>
        /// <param name="display">If true, posts the failure to the screen message board.</param>
        void BreakThrustGauge(bool display)
        {
            if (!broken)
            {
                failure = "Thrust Gauge Stuck";
                rocketPartsLeftToFix = rocketPartsNeededToFix;

                Events["FixThrustGauge"].guiActiveUnfocused = true;

                if (display)
                {
                    KMUtil.PostFailure(part, "'s thrust gauge has become stuck!");
                }

                broken = true;
            }
        }

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