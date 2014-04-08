using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;

namespace Kerbal_Mechanics
{
    class ModuleEngineReliability : ModuleEngines
    {
        //CONSTANTS AND STATICS
        #region CONSTANTS AND STATICS
        /// <summary>
        /// Time until the next running failure check.
        /// </summary>
        static readonly float timeTillFailCheck = 10f;
        #endregion

        //KSP FIELDS
        #region KSP FIELDS
        /// <summary>
        /// The quality of a part. This is tweaked by the player to determine price and reliability drain.
        /// </summary>
        [UI_FloatRange(maxValue = 1f, minValue = 0f, stepIncrement = 0.01f)]
        [KSPField(guiName = "Quality", guiActiveEditor = true, guiFormat = "P0", isPersistant = true)]
        public float quality = 0.75f;
        /// <summary>
        /// Reliability of the part. Determines the failure chance (between perfect and terrible).
        /// </summary>
        [KSPField(guiName = "Reliability", guiActive = true, guiFormat = "P1", isPersistant = true)]
        public float reliability = 1f;

        //RELIABILITY
        #region RELIABILITY
        /// <summary>
        /// The reliability drain of this part while running perfectly (100% quality).
        /// </summary>
        [KSPField] public float reliabilityDrainPerfect = 0.001f;
        /// <summary>
        /// The reliability drain of this part while running terribly (0% quality).
        /// </summary>
        [KSPField] public float reliabilityDrainTerrible = 0.025f;
        #endregion
        
        //FAILURE CHANCES
        #region FAILURE CHANCES
        /// <summary>
        /// Chance to fail while running under perfect conditions
        /// </summary>
        [KSPField]
        public float runningChanceToFailPerfect = 0.000001f;
        /// <summary>
        /// Chance to fail while starting under perfect conditions
        /// </summary>
        [KSPField]
        public float startingChanceToFailPerfect = 0.015625f;

        /// <summary>
        /// Chance to fail while running under the worst of conditions
        /// </summary>
        [KSPField]
        public float runningChanceToFailTerrible = 0.01f;
        /// <summary>
        /// Chance to fail while starting under the worst of conditions
        /// </summary>
        [KSPField]
        public float startingChanceToFailTerrible = 0.875f;
        #endregion

        /// <summary>
        /// The current failure of the part. Empty if part condition is nominal.
        /// </summary>
        [KSPField(guiName = "Failure", guiActive = false, guiActiveEditor = false, isPersistant = true)]
        string currentFailure = "";
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
        /// Gets the crrent starting chance to fail, based on the current reliability.
        /// </summary>
        public float CurrentStartingChanceToFail
        {
            get { return startingChanceToFailPerfect + ((startingChanceToFailTerrible - startingChanceToFailPerfect) * (1f - reliability)); }
        }

        /// <summary>
        /// Gets the current reliability drain enacted on this part, based on the quality.
        /// </summary>
        public float CurrentReliabilityDrain
        {
            get { return reliabilityDrainPerfect + ((reliabilityDrainTerrible - reliabilityDrainPerfect) * (1f - quality)); }
        }
        #endregion

        //OTHER VARS
        #region OTHER VARS
        /// <summary>
        /// Time since the last running failure check.
        /// </summary>
        float timeSinceFailCheck = 0f;

        /// <summary>
        /// The thrust at last frame.
        /// </summary>
        float lastFrameThrust = 0f;
        /// <summary>
        /// The thrust of this frame
        /// </summary>
        float currFrameThrust = 0f;

        float timeSinceLastMsg = 0f;

        /// <summary>
        /// The gimbal module of this engine. Null if no gimbal is attached.
        /// </summary>
        ModuleGimbal gimbal;

        /// <summary>
        /// The three possible failure strings.
        /// </summary>
        static string[] failureStrings = { "Ignition Coil Burnt", "Cooling Failure", "Gimbal Stuck" };
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

            gimbal = part.Modules.OfType<ModuleGimbal>().FirstOrDefault<ModuleGimbal>();

            BreakPart(currentFailure);
        }

        /// <summary>
        /// Called when the part is updated.
        /// </summary>
        public override void OnUpdate()
        {
            string currentStatus = status;

            base.OnUpdate();

            status = currentStatus;

            staged = EngineIgnited || staged;

            lastFrameThrust = currFrameThrust;
            currFrameThrust = FlightInputHandler.state.mainThrottle;

            if (EngineIgnited && !CheatOptions.InfiniteFuel && currentFailure == "")
            {
                if (currFrameThrust > 0f && lastFrameThrust == 0f)
                {
                    if (Random.Range(0f, 1f) < CurrentStartingChanceToFail)
                    {
                        BreakPart(failureStrings[0]);
                    }
                }

                if (timeSinceFailCheck < timeTillFailCheck)
                {
                    timeSinceFailCheck += TimeWarp.deltaTime;
                }
                else
                {
                    reliability -= CurrentReliabilityDrain;

                    timeSinceFailCheck = 0f;

                    if (Random.Range(0f, 1f) < CurrentRunningChanceToFail)
                    {
                        BreakPart(failureStrings[((gimbal != null) ? Random.Range(1, failureStrings.Length) : 1)]);
                    }
                }

                if (currentFailure == failureStrings[1])
                {
                    part.temperature += heatProduction * currentThrottle * TimeWarp.deltaTime;
                }
            }

            reliability = Mathf.Max(reliability, 0f);

            //if (timeSinceLastMsg < 5f)
            //{
            //    timeSinceLastMsg += TimeWarp.deltaTime;
            //}
            //else
            //{
            //    timeSinceLastMsg = 0f;
            //    Logger.DebugLog("Staged: " + staged.ToString());
            //}
        }

        /// <summary>
        /// Gets the information about the part module appended to the information provided by the base module.
        /// </summary>
        /// <returns></returns>
        public override string GetInfo()
        {
            string info = base.GetInfo();

            info += KMUtil.NewLine;
            info += "<b><color=#99ff00ff>Reliability:</color></b>";
            info += KMUtil.NewLine;

            info += "<b>Running Failure Chance</b>:";
            info += KMUtil.NewLine;

            info += "@ 100%: " + runningChanceToFailPerfect.ToString("P4");
            info += KMUtil.NewLine;

            info += "@ 1%:   " + runningChanceToFailTerrible.ToString("P1");
            info += KMUtil.NewLine;
            info += KMUtil.NewLine;

            info += "<b>Ignition Failure Chance</b>:";
            info += KMUtil.NewLine;

            info += "@ 100%: " + startingChanceToFailPerfect.ToString("P4");
            info += KMUtil.NewLine;

            info += "@ 1%:   " + startingChanceToFailTerrible.ToString("P1");

            return info;
        }
        #endregion

        //UNITY METHODS
        #region UNITY METHODS
        /// <summary>
        /// Called when Unity wants to update this module (which it considers a component).
        /// </summary>
        //public void Update()
        //{
        //    if (Input.GetKeyDown(KeyCode.Backslash))
        //    {
        //        BreakPart(failureStrings[2]);
        //    }
        //}
        #endregion

        //KSP EVENTS
        #region KSP EVENTS
        /// <summary>
        /// Fixes the engine.
        /// </summary>
        [KSPEvent(active = true, guiName = "Fix Engine", guiActive = false, guiActiveUnfocused = true, unfocusedRange = 3f, externalToEVAOnly = true)]
        public void FixEngine()
        {
            switch (currentFailure)
            {
                case "Ignition Coil Burnt":
                    {
                        Events["Shutdown"].guiActive = true;
                        Events["Activate"].guiActive = true;
                        break;
                    }
                case "Cooling Failure":
                    {
                        break;
                    }
                case "Gimbal Stuck":
                    {
                        gimbal.Events["LockGimbal"].guiActive = true;
                        gimbal.Events["FreeGimbal"].guiActive = true;
                        break;
                    }
            }

            Events["FixEngine"].guiActiveUnfocused = false;
            Fields["currentFailure"].guiActive = false;
            currentFailure = "";
            status = "Nominal";

            reliability += 0.2f;

            reliability = Mathf.Min(reliability, 1f);
        }
        #endregion

        // OTHER METHODS
        #region OTHER METHODS
        /// <summary>
        /// Breaks the part based on which failure string is passed.
        /// </summary>
        /// <param name="failure">A string describing the failure that has taken place.</param>
        void BreakPart(string failure)
        {
            if (!string.IsNullOrEmpty(failure))
            {
                Fields["currentFailure"].guiActive = true;
                currentFailure = failure;

                string message = "FAILURE: " + failure + " on " + part.partInfo.title + "!";

                switch (failure)
                {
                    case "Ignition Coil Burnt":
                        {
                            Shutdown();
                            Events["Activate"].guiActive = false;
                            Events["Shutdown"].guiActive = false;
                            break;
                        }
                    case "Cooling Failure":
                        {
                            break;
                        }
                    case "Gimbal Stuck":
                        {
                            gimbal.LockGimbal();
                            gimbal.Events["LockGimbal"].guiActive = false;
                            gimbal.Events["FreeGimbal"].guiActive = false;
                            break;
                        }
                }

                Events["FixEngine"].guiActiveUnfocused = true;
                status = "Damaged";

                ScreenMessages.PostScreenMessage(new ScreenMessage(message, 3f, ScreenMessageStyle.UPPER_LEFT, StyleManager.GetStyle("Upper Left - Red")));
                Logger.DebugLog("(" + reliability.ToString() + ") " + message);
            }
        }
        #endregion
    }
}
