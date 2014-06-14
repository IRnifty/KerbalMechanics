using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Kerbal_Mechanics
{
    class ModuleReliabilityGimbal : ModuleReliabilityBase
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
        /// Chance to fail while running under perfect conditions.
        /// </summary>
        [KSPField]
        public float chanceToFailPerfect = 0.00001f;
        /// <summary>
        /// Chance to fail while running under terrible conditions.
        /// </summary>
        [KSPField]
        public float chanceToFailTerrible = 0.001f;

        /// <summary>
        /// The chance a kick will permenently lock the gimbal.
        /// </summary>
        [KSPField]
        public float chanceKickWillLock = 0.05f;

        /// <summary>
        /// The chance a kick will fix the gimbal.
        /// </summary>
        [KSPField]
        public float chanceKickWillFix = 0.10f;

        /// <summary>
        /// Is this part permanently locked?
        /// </summary>
        [KSPField(guiActive = true, guiName = "Permanent Lock", isPersistant = true)]
        public bool permanentLock = false;
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
        /// The name of the module. Held for use by the module injecter.
        /// </summary>
        public override string ModuleName
        {
            get { return "Gimbal"; }
        }
        #endregion

        //OTHER VARS
        #region OTHER VARS
        /// <summary>
        /// Time since the last running failure check.
        /// </summary>
        float timeSinceFailCheck = 0f;

        /// <summary>
        /// The gimbal module of this engine. Null if no gimbal is attached.
        /// </summary>
        ModuleGimbal gimbal;
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

            gimbal = part.Modules.OfType<ModuleGimbal>().FirstOrDefault<ModuleGimbal>();

            if (!gimbal)
            {
                Logger.DebugError("The gimbal module is NULL!");
            }

            if (permanentLock)
            {
                BreakGimbal(false);
            }

            Fields["quality"].guiName = "Gimbal " + Fields["quality"].guiName;
        }

        /// <summary>
        /// Called when the part is updated.
        /// </summary>
        public override void OnUpdate()
        {
            base.OnUpdate();

            if (gimbal && FlightInputHandler.state.mainThrottle > 0f)
            {
                if (timeSinceFailCheck < timeTillFailCheck)
                {
                    timeSinceFailCheck += TimeWarp.deltaTime;
                }
                else
                {
                    timeSinceFailCheck = 0f;
                    reliability -= CurrentReliabilityDrain;

                    if (Random.Range(0f, 1f) < CurrentChanceToFail)
                    {
                        BreakGimbal(true);
                    }
                }
            }
        }
        #endregion

        //KSP EVENTS
        #region KSP EVENTS
        public void FixGimbal()
        {
            if (FlightGlobals.ActiveVessel.isEVA)
            {
                Part kerbal = FlightGlobals.ActiveVessel.parts[0];

                rocketPartsNeededToFix -= (int)kerbal.RequestResource("RocketParts", (double)System.Math.Min(rocketPartsLeftToFix, 10));

                fixSound.audio.Play();

                if (rocketPartsLeftToFix <= 0)
                {
                    FixGimbal(false);
                }
            }
        }

        public void KickGimbal()
        {
            bashSound.audio.clip = SoundManager.GetSound("Hammer" + Random.Range(1, 7).ToString());
            bashSound.audio.Play();

            float rand = Random.Range(0f, 1f);

            if (rand < chanceKickWillLock)
            {
                permanentLock = true;
                KMUtil.PostFailure(part, " has permanently locked due to kicking!");
            }
            else if (rand > (1f - chanceKickWillFix))
            {
                FixGimbal(false);
            }
        }
        #endregion

        //OTHER METHODS
        #region OTHER METHODS
        /// <summary>
        /// Breaks the gimbal.
        /// </summary>
        /// <param name="display">Whether or not to post the failure.</param>
        void BreakGimbal (bool display)
        {
            failure = "Gimbal Stuck";

            gimbal.LockGimbal();
            gimbal.Events["LockGimbal"].guiActive = false;
            gimbal.Events["FreeGimbal"].guiActive = false;
            gimbal.Actions["ToggleAction"].active = false;
            rocketPartsLeftToFix = rocketPartsNeededToFix;

            if (display)
            {
                KMUtil.PostFailure(part, "'s gimbal has frozen!");
            }
        }

        /// <summary>
        /// Actually fixes the gimbal.
        /// </summary>
        /// <param name="kicked">Whether or not the gimbal was kicked.</param>
        void FixGimbal(bool kicked)
        {
            failure = "";

            gimbal.FreeGimbal();
            gimbal.Events["LockGimbal"].guiActive = true;
            gimbal.Events["FreeGimbal"].guiActive = true;
            gimbal.Actions["ToggleAction"].active = true;

            reliability += 0.2f - (kicked ? 1.5f : 0f);
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
            GUILayout.Label(KMUtil.FormatPercent(reliability, 2), HighLogic.Skin.label);
            GUILayout.Label(" ", HighLogic.Skin.label);
            GUILayout.Label(" ", HighLogic.Skin.label);
            GUILayout.Label(KMUtil.FormatPercent(chanceToFailPerfect), HighLogic.Skin.label);
            GUILayout.Label(KMUtil.FormatPercent(chanceToFailTerrible), HighLogic.Skin.label);
            GUILayout.Label(" ", HighLogic.Skin.label);
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
        }
        #endregion
    }
}
