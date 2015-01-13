using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KerbalMechanics
{
    /// <summary>
    /// The base class for all reliability modules.
    /// </summary>
    abstract class ModuleReliabilityBase : PartModule
    {
        //CONSTANTS AND STATICS
        #region CONSTANTS AND STATICS
        /// <summary>
        /// Time until the next running failure check.
        /// </summary>
        protected static readonly float timeTillFailCheck = 10f;
        #endregion

        //KSP FIELDS
        #region KSP FIELDS
        /// <summary>
        /// The quality of a part. This is tweaked by the player to determine price and reliability drain.
        /// </summary>
        [KSPField(isPersistant = true)]
        public float quality = -1f;

        /// <summary>
        /// Reliability of the part. Determines the failure chance (between perfect and terrible).
        /// </summary>
        [KSPField(isPersistant = true)]
        public double reliability = -1f;

        /// <summary>
        /// Is the part broken?
        /// </summary>
        [KSPField(guiName = "Failure", isPersistant = true)]
        public string failure = "";

        /// <summary>
        /// The required crew skill to repair this part. Level 0 is completely untrained, and level 5 is completely trained.
        /// </summary>
        [KSPField]
        public int repairSkill = 0;

        /// <summary>
        /// How many rocket parts does it take in total to fix this part?
        /// </summary>
        [KSPField]
        public int rocketPartsNeededToFix = 50;

        /// <summary>
        /// How many rocket parts are still needed to fix this part?
        /// </summary>
        [KSPField(isPersistant = true)]
        public int rocketPartsLeftToFix = 50;

        /// <summary>
        /// Hacked field. This method is designed to display the current failure state while on EVA. The GUI name of this "field" is updated in OnUpdate.
        /// </summary>
        [KSPEvent(active = true, guiActive = false, guiActiveEditor = false, guiActiveUnfocused = true, externalToEVAOnly = true, unfocusedRange = 3f, guiName = "")]
        public void UnfocusedFailure()
        {

        }

        /// <summary>
        /// Hacked field. This method is designed to display the current amount of parts needed to fix this part while on EVA. The GUI name of this "field" is updated in OnUpdate.
        /// </summary>
        [KSPEvent(active = true, guiActive = false, guiActiveEditor = false, guiActiveUnfocused = true, externalToEVAOnly = true, unfocusedRange = 3f, guiName = "")]
        public void UnfocusedPartsNeeded()
        {

        }

        //RELIABILITY
        #region RELIABILITY
        /// <summary>
        /// The reliability drain of this part while running perfectly (100% quality).
        /// </summary>
        [KSPField]
        public int lifeTimePerfect = 425;

        /// <summary>
        /// The reliability drain of this part while running terribly (0% quality).
        /// </summary>
        [KSPField]
        public int lifeTimeTerrible = 43;
        #endregion
        #endregion

        //PROPERTIES
        #region PROPERTIES
        /// <summary>
        /// Gets the current reliability drain enacted on this part, based on the quality.
        /// </summary>
        public double CurrentReliabilityDrain
        {
            get
            {
                double days =  KMUtil.GetPointOnCurve(reliabilityCurve, quality).y;

                return (double)(1 / (days * 2160)); // 2160 = 6 hour Kerbin day in intervals of 10 seconds.
            }
        }

        /// <summary>
        /// Returns true if the current Kerbal can repair this reliability module.
        /// </summary>
        protected bool CanRepair
        {
            get { return FlightGlobals.ActiveVessel.VesselValues.RepairSkill.value >= repairSkill; }
        }

        /// <summary>
        /// The name of the module. Held for use by the module injector.
        /// </summary>
        public abstract string ModuleName
        {
            get;
        }
        #endregion

        //OTHER VARS
        #region OTHER VARS
        /// <summary>
        /// Time since the last running failure check.
        /// </summary>
        protected float timeSinceFailCheck = 0f;

        /// <summary>
        /// The fix sound.
        /// </summary>
        protected FXGroup fixSound;

        /// <summary>
        /// The bash sound.
        /// </summary>
        protected FXGroup bashSound;

        /// <summary>
        /// The list of points used for creating a reliability curve.
        /// </summary>
        private Vector2d[] reliabilityCurve;

        /// <summary>
        /// Is the part officially broken?
        /// </summary>
        protected bool broken = false;
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

            if (quality == -1f)
            {
                quality = 0.75f;
            }

            if (reliability == -1f)
            {
                reliability = 1f;

                // Penalty for low quality parts.
                if (quality < 0.5f)
                {
                    reliability -= 0.1f;

                    if (quality < 0.25f)
                    {
                        reliability -= 0.1f;
                    }
                }
            }

            reliabilityCurve = new Vector2d[] { new Vector2d(0, lifeTimeTerrible),
			new Vector2d(0.75, lifeTimeTerrible),
			new Vector2d(0.25, lifeTimePerfect),
			new Vector2d(1, lifeTimePerfect) };

            SoundManager.LoadSound(KMUtil.soundSource + "Fix", "Fix");

            fixSound = new FXGroup("fixSound");
            SoundManager.CreateFXSound(part, fixSound, "Fix", false);

            for (int i = 1; i < 7; i++)
            {
                SoundManager.LoadSound(KMUtil.soundSource + "Hammer" + i.ToString(), "Hammer" + i.ToString());
            }

            bashSound = new FXGroup("hammerBash");
            SoundManager.CreateFXSound(part, bashSound, "Hammer1", false);

            Events["PerformMaintenance"].guiName = "Maintain " + ModuleName;
        }

        /// <summary>
        /// Loads any additional fields not loaded automatically.
        /// </summary>
        /// <param name="node">The config node for this module.</param>
        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);

            if (node.HasValue("reliability")) { reliability = double.Parse(node.GetValue("reliability")); }
        }

        /// <summary>
        /// Updates the module.
        /// </summary>
        public override void OnUpdate()
        {
            base.OnUpdate();

            if (HighLogic.LoadedSceneIsFlight)
            {
                Fields["failure"].guiActive = (failure != "");

                if (failure != "")
                {
                    Events["UnfocusedFailure"].active = true;
                    Events["UnfocusedFailure"].guiName = "Failure: " + failure;

                    Events["UnfocusedPartsNeeded"].active = true;
                    Events["UnfocusedPartsNeeded"].guiName = "Parts Needed: " + rocketPartsLeftToFix.ToString();

                    Events["PerformMaintenance"].active = false;
                }
                else
                {
                    Events["UnfocusedFailure"].active = false;
                    Events["UnfocusedPartsNeeded"].active = false;
                    Events["PerformMaintenance"].active = reliability < 0.99f;
                }

                reliability = reliability.Clamp(0, 1);
            }
        }
        #endregion

        //KSP EVENTS
        #region KSP EVENTS
        public abstract void PerformMaintenance();
        #endregion

        //OTHER METHODS
        #region OTHER METHODS
        /// <summary>
        /// Abstract. Requires child classes to implement a method which provides reliability information about the module.
        /// </summary>
        public abstract void DisplayDesc(double inaccuracySeverity);
        #endregion
    }
}
