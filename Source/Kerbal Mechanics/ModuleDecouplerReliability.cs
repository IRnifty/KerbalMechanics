using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Kerbal_Mechanics
{
    class ModuleDecouplerReliability : ModuleDecouple
    {
        //CONSTANTS AND STATICS
        #region CONSTANTS AND STATICS
        static readonly string hammerBashSource = "KerbalMechanics/Sounds/";
        #endregion

        //KSP FIELDS
        #region KSP FIELDS
        [KSPField] public float chanceOfExplosion = 0.125f;
        [KSPField] public float chanceOfExplosionEVA = 0.125f;
        [KSPField] public float chanceOfNothing = 0.25f;
        [KSPField] public float chanceOfNothingEVA = 0.675f;
        #endregion

        //OTHER VARS
        #region OTHER VARS
        FXGroup hammerBash;
        #endregion

        // KSP METHODS
        #region KSP METHODS
        /// <summary>
        /// Called when the part is started by Unity.
        /// </summary>
        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            if (!SoundManager.IsInitialized)
            {
                for (int i = 1; i < 7; i++)
                {
                    SoundManager.LoadSound(hammerBashSource + "Hammer" + i.ToString(), "Hammer" + i.ToString());
                }
            }
            hammerBash = new FXGroup("hammerBash");
            SoundManager.CreateFXSound(part, hammerBash, "Hammer1", false);
        }

        /// <summary>
        /// Called when the part is activated.
        /// </summary>
        public override void OnActive()
        {
            float rand = Random.Range(0f, 1f);

            if (rand < chanceOfExplosion)
            {
                part.explode();
                PostFailure(" has exploded due to improper explosive rigging.");
            }
            else if (rand < chanceOfNothing)
            {
                Events["Decouple"].guiActive = false;
                PostFailure(" failed to detonate separation explosive.");
            }
            else
            {
                base.OnActive();
            }
        }

        /// <summary>
        /// Called when the decoupler is activated from the context menu.
        /// </summary>
        new public void Decouple()
        {
            float rand = 0.45f;
            float splosionChance = FlightGlobals.ActiveVessel.isEVA ? chanceOfExplosionEVA : chanceOfExplosion;

            if (rand < chanceOfExplosion)
            {
                part.explode();
                PostFailure(" has exploded due to improper explosive rigging.");
            }
            else if (rand < chanceOfNothing)
            {
                Events["Decouple"].guiActive = false;
            }
            else
            {
                base.Decouple();
            }
        }

        public override string GetInfo()
        {
            string info = base.GetInfo();

            info += KMUtil.NewLine;
            info += "<b><color=#99ff00ff>Reliability:</color></b>";
            info += KMUtil.NewLine;

            info += "<b>Chance of Simple Failure</b>:";
            info += KMUtil.NewLine;

            info += "Normal: " + chanceOfNothing.ToString("P1");
            info += KMUtil.NewLine;

            info += "On EVA: " + chanceOfNothingEVA.ToString("P1");
            info += KMUtil.NewLine;

            info += "<b>Chance of Explosion</b>:";
            info += KMUtil.NewLine;

            info += "Normal: " + chanceOfExplosion.ToString("P1");
            info += KMUtil.NewLine;

            info += "On EVA: " + chanceOfExplosionEVA.ToString("P1");
            info += KMUtil.NewLine;

            return info;
        }
        #endregion

        //KSP EVENTS
        #region KSP EVENTS
        /// <summary>
        /// Called when "Bash with Hammer" is selected from the EVA context menu.
        /// </summary>
        [KSPEvent(active = true, guiName = "Bash With Hammer", externalToEVAOnly = true, guiActiveUnfocused = true, unfocusedRange = 3f)]
        public void BashWithHammer()
        {
            hammerBash.audio.clip = SoundManager.GetSound("Hammer" + Random.Range(1, 7).ToString());
            hammerBash.audio.Play();

            float rand = Random.Range(0f, 1f);

            if (rand < chanceOfExplosionEVA)
            {
                part.explode();
            }
            else if (rand >= chanceOfNothingEVA)
            {
                base.Decouple();
            }
        }
        #endregion

        //OTHER METHODS
        #region OTHER METHODS
        void PostFailure(string failure)
        {
            string message = "FAILURE: " + part.partInfo.title + failure;

            ScreenMessages.PostScreenMessage(new ScreenMessage(message, 3f, ScreenMessageStyle.UPPER_LEFT, StyleManager.GetStyle("Upper Left - Red")));
            Logger.DebugLog(message);
        }
        #endregion
    }
}
