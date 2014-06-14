using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Kerbal_Mechanics
{
    /// <summary>
    /// The base class for all reliability modules.
    /// </summary>
    abstract class ModuleReliabilityBase : PartModule
    {
        //KSP FIELDS
        #region KSP FIELDS
        /// <summary>
        /// The quality of a part. This is tweaked by the player to determine price and reliability drain.
        /// </summary>
        [KSPField(isPersistant = true)]
        public float quality = 0.75f;

        /// <summary>
        /// Reliability of the part. Determines the failure chance (between perfect and terrible).
        /// </summary>
        [KSPField(isPersistant = true)]
        public float reliability = 1f;

        /// <summary>
        /// Is the part broken?
        /// </summary>
        [KSPField(guiName = "Failure", guiActive = false, guiActiveEditor = false, isPersistant = true)]
        public string failure = "";

        /// <summary>
        /// How many rocket parts does it take in total to fix this part?
        /// </summary>
        [KSPField]
        public int rocketPartsNeededToFix = 50;

        /// <summary>
        /// How many rocket parts are still needed to fix this part?
        /// </summary>
        [KSPField(guiName = "Parts Needed", guiActive = false, guiActiveEditor = false, isPersistant = true)]
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
        public float reliabilityDrainPerfect = 0.001f;

        /// <summary>
        /// The reliability drain of this part while running terribly (0% quality).
        /// </summary>
        [KSPField]
        public float reliabilityDrainTerrible = 0.025f;
        #endregion
        #endregion

        //PROPERTIES
        #region PROPERTIES
        /// <summary>
        /// Gets the current reliability drain enacted on this part, based on the quality.
        /// </summary>
        public float CurrentReliabilityDrain
        {
            get { return reliabilityDrainPerfect + ((reliabilityDrainTerrible - reliabilityDrainPerfect) * (1f - quality)); }
        }
        /// <summary>
        /// The name of the module. Held for use by the module injecter.
        /// </summary>
        public virtual string ModuleName
        {
            get { return "Reliability"; }
        }
        #endregion

        //OTHER VARS
        #region OTHER VARS
        /// <summary>
        /// The fix sound.
        /// </summary>
        protected FXGroup fixSound;

        /// <summary>
        /// The bash sound.
        /// </summary>
        protected FXGroup bashSound;
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

            SoundManager.LoadSound(KMUtil.soundSource + "Fix", "Fix");

            fixSound = new FXGroup("fixSound");
            SoundManager.CreateFXSound(part, fixSound, "Fix", false);

            for (int i = 1; i < 7; i++)
            {
                SoundManager.LoadSound(KMUtil.soundSource + "Hammer" + i.ToString(), "Hammer" + i.ToString());
            }

            bashSound = new FXGroup("hammerBash");
            SoundManager.CreateFXSound(part, bashSound, "Hammer1", false);
        }

        /// <summary>
        /// Updates the module.
        /// </summary>
        public override void OnUpdate()
        {
            base.OnUpdate();

            Fields["failure"].guiActive = (failure != "");

            if (failure != "")
            {
                Events["UnfocusedFailure"].active = true;
                Events["UnfocusedFailure"].guiName = "Failure: " + failure;

                Events["UnfocusedPartsNeeded"].active = true;
                Events["UnfocusedPartsNeeded"].guiName = "Parts Needed: " + rocketPartsLeftToFix.ToString();
            }
            else
            {
                Events["UnfocusedFailure"].active = false;
                Events["UnfocusedPartsNeeded"].active = false;
            }

            reliability = Mathf.Max(reliability, 0f);
        }
        #endregion

        //OTHER METHODS
        #region OTHER METHODS
        public abstract void DisplayDesc();
        #endregion
    }
}
