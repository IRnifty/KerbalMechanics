using System.Linq;
using System.Text;
using System.Collections.Generic;
using UnityEngine;

namespace Kerbal_Mechanics
{
    class ModuleLightReliability : ModuleLight
    {
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
        public float chanceToFailPerfect = 0.000001f;
        /// <summary>
        /// Chance to fail while running under the worst of conditions
        /// </summary>
        [KSPField]
        public float chanceToFailTerrible = 0.01f;
        #endregion

        /// <summary>
        /// The status if this light.
        /// </summary>
        [KSPField(guiActive = true, guiName = "Status", isPersistant = true)]
        string status = "Nominal";
        #endregion

        //PROPERTIES
        #region PROPERTIES
        /// <summary>
        /// Gets the current running chance to fail, based on the current reliability.
        /// </summary>
        public float CurrentChanceToFail
        {
            get { return chanceToFailPerfect + ((chanceToFailTerrible - chanceToFailPerfect) * (1f - reliability)); }
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
        /// Is this light flickering?
        /// </summary>
        bool isFlickering = false;

        /// <summary>
        /// The maximum time this light will flicker. Randomized 3f ± 1f.
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
        /// The time since the last failure check.
        /// </summary>
        float timeSinceFailCheck = 0f;
        /// <summary>
        /// The time until the next failure check. Randomized after every check.
        /// </summary>
        float timeTillFailCheck = 5f;

        /// <summary>
        /// The Animation Component the part is using. Likely to disappear if Squad exposes their animation members.
        /// </summary>
        Animation lightAnim;
        /// <summary>
        /// The Animation State this part is in. Likely to disappear if Squad exposes their animation members.
        /// </summary>
        AnimationState lightAnimState;

        /// <summary>
        /// The list of lights this light manages.
        /// </summary>
        List<Light> lightLights;
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

            lightLights = new List<Light>((IEnumerable<Light>) part.FindModelComponents<Light>(lightName));

            foreach (Animation anim in part.FindModelComponents<Animation>())
            {
                if ((TrackedReference)anim[animationName] != (TrackedReference)null)
                {
                    lightAnimState = anim[animationName];
                    lightAnim = anim;
                }
            }

            if (status != "Nominal")
            {
                BustBulb();
            }
        }

        /// <summary>
        /// Called when the part is updated.
        /// </summary>
        public override void OnUpdate()
        {
            string currentStatus = status;

            base.OnUpdate();

            status = currentStatus;

            if (isOn)
            {
                if (timeSinceFailCheck < timeTillFailCheck)
                {
                    timeSinceFailCheck += TimeWarp.deltaTime;
                }
                else
                {
                    reliability -= CurrentReliabilityDrain;

                    timeSinceFailCheck = 0f;

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
                    maxFlickerTime = Random.Range(0.1f, 0.4f);
                    currentFlickerTime = 0f;
                    FlickerBulb();
                }

                if (currentOverallFlickeringTime < maxOverallFlickeringTime)
                {
                    currentOverallFlickeringTime += TimeWarp.deltaTime;
                }
                else
                {
                    BustBulb();
                }
            }

            reliability = Mathf.Max(reliability, 0f);
        }

        public override string GetInfo()
        {
            string info = base.GetInfo();

            info += KMUtil.NewLine;
            info += "<b><color=#99ff00ff>Reliability:</color></b>";
            info += KMUtil.NewLine;

            info += "<b>Failure Chance</b>:";
            info += KMUtil.NewLine;

            info += "@ 100%: " + chanceToFailPerfect.ToString("P4");
            info += KMUtil.NewLine;

            info += "@ 1%:   " + chanceToFailTerrible.ToString("P1");

            return info;
        }
        #endregion

        //UNITY METHODS
        #region UNITY METHODS
        //new public void Update()
        //{
        //    if (Input.GetKeyDown(KeyCode.Backslash))
        //    {
        //        BeginFlickering();
        //    }

        //    base.Update();
        //}
        #endregion

        //KSP EVENTS
        #region KSP EVENTS
        [KSPEvent(active = true, guiName = "Replace Bulb", guiActive = false, guiActiveUnfocused = true, unfocusedRange = 3f, externalToEVAOnly = true)]
        public void RelpaceBulb()
        {
            Events["LightsOff"].guiActive = true;
            Events["LightsOn"].guiActive = true;
            status = "Nominal";

            reliability = 1f;
        }
        #endregion

        //OTHER METHODS
        #region OTHER METHODS
        public void BustBulb()
        {
            isFlickering = false;

            LightsOff();

            string message = "FAILURE: " + part.partInfo.title + " has failed.";

            ScreenMessages.PostScreenMessage(new ScreenMessage(message, 3f, ScreenMessageStyle.UPPER_LEFT, StyleManager.GetStyle("Upper Left - Red")));
            Logger.DebugLog("(" + reliability.ToString() + ") " + message);
        }

        public void FlickerBulb()
        {
            if (isOn)
            {
                LightsOff();
            }
            else
            {
                LightsOn();
            }
        }

        public void BeginFlickering()
        {
            maxOverallFlickeringTime = Random.Range(2f, 4f);

            Events["LightsOff"].guiActive = false;
            Events["LightsOn"].guiActive = false;
            isFlickering = true;
            status = "Replacement Needed";
        }
        #endregion
    }
}