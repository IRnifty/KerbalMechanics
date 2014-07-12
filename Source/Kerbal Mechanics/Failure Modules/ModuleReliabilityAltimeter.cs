using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Kerbal_Mechanics
{
    class ModuleReliabilityAltimeter : ModuleReliabilityBase
    {
        //KSP FIELDS
        #region KSP FIELDS
        /// <summary>
        /// The chance this part's altimeter has to fail in perfect condition when its breaking g force is reached.
        /// </summary>
        [KSPField]
        public double chanceToFailPerfect = 0.1;

        /// <summary>
        /// The chance this part's altimeter has to fail in terrible condition when its breaking g force is reached.
        /// </summary>
        [KSPField]
        public double chanceToFailTerrible = 0.5;

        /// <summary>
        /// Maximum g force the vessel can take before the altimeter breaks with perfect reliability.
        /// </summary>
        [KSPField]
        public double maxGeesPerfect = 12.5;
        /// <summary>
        /// Maximum g force the vessel can take before the altimeter breaks with terrible reliability.
        /// </summary>
        [KSPField]
        public double maxGeesTerrible = 7.5;
        #endregion

        //PROPERTIES
        #region PROPERTIES
        /// <summary>
        /// Gets the current chance to fail, based on current relibility.
        /// </summary>
        public double CurrentChanceToFail
        {
            get { return chanceToFailPerfect + ((chanceToFailTerrible - chanceToFailPerfect) * (1f - reliability)); }
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
        /// The module name.
        /// </summary>
        public override string ModuleName
        {
            get { return "Altimeter"; }
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
                InstrumentReliabilityManager.AddAltimetermodule(this);

                if (failure != "")
                {
                    BreakAltimeter();
                }
            }
        }

        /// <summary>
        /// Called every time this part is updated.
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
                    reliability -= CurrentReliabilityDrain + (CurrentReliabilityDrain * CurrentOverGees);
                }

                if (vessel.geeForce > CurrentMaxGees)
                {
                    if (UnityEngine.Random.Range(0f, 1f) < CurrentChanceToFail * TimeWarp.deltaTime)
                    {
                        BreakAltimeter();
                    }
                }
            }
            
            base.OnUpdate();
        }
        #endregion

        //KSP EVENTS
        #region KSP EVENTS
        [KSPEvent(active = false, guiActive = false, guiActiveEditor = false, guiActiveUnfocused = true, externalToEVAOnly = true, unfocusedRange = 3f, guiName = "Fix Altimeter")]
        public void FixAltimeter()
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
                    broken = false;
                }
            }
        }

        [KSPEvent(active = false, guiActive = false, guiActiveEditor = false, guiActiveUnfocused = true, externalToEVAOnly = true, unfocusedRange = 3f, guiName = "Perform Maintenance")]
        public override void PerformMaintenance()
        {
            if (FlightGlobals.ActiveVessel.isEVA)
            {
                Part kerbal = FlightGlobals.ActiveVessel.parts[0];

                rocketPartsLeftToFix -= (int)kerbal.RequestResource("RocketParts", (double)System.Math.Min(rocketPartsLeftToFix, 2));

                fixSound.audio.Play();

                reliability += 0.1;
                reliability = reliability.Clamp(0, 1);
            }
        }
        #endregion

        //OTHER METHODS
        #region OTHER METHODS
        /// <summary>
        /// Breaks this module's altimeter.
        /// </summary>
        void BreakAltimeter()
        {
            if (!broken)
            {
                failure = "Altimeter Stuck";
                rocketPartsLeftToFix = rocketPartsNeededToFix;
                KMUtil.PostFailure(part, "'s altimeter has become stuck!");

                broken = true;
            }
        }

        /// <summary>
        /// Displays reliability information.
        /// </summary>
        public override void DisplayDesc()
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
            GUILayout.Label(reliability.ToString("##0.00%"), HighLogic.Skin.label);
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