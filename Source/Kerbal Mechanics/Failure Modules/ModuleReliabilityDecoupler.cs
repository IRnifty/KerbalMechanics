using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Kerbal_Mechanics
{
    /// <summary>
    /// The decoupler reliability module. Adds failure and repair functionality to parts equipped with either ModuleDecouple or ModuleAnchoredDecoupler.
    /// </summary>
    class ModuleReliabilityDecoupler : ModuleReliabilityBase
    {
        //KSP FIELDS
        #region KSP FIELDS
        /// <summary>
        /// The chance of explosion upon activation.
        /// </summary>
        [KSPField]
        public float chanceOfExplosion = 0.125f;
        /// <summary>
        /// The chance of explosion when bashed with a hammer.
        /// </summary>
        [KSPField]
        public float chanceOfExplosionEVA = 0.125f;
        /// <summary>
        /// The chance of nothing happening upon activation.
        /// </summary>
        [KSPField]
        public float chanceOfNothing = 0.5f;
        /// <summary>
        /// The chance of nothing happening when bashed with a hammer.
        /// </summary>
        [KSPField]
        public float chanceOfNothingEVA = 0.675f;
        #endregion

        //PROPERTIES
        #region PROPERTIES
        /// <summary>
        /// The name of the module. Held for use by the module injecter.
        /// </summary>
        public override string ModuleName
        {
            get { return "Decoupler"; }
        }
        #endregion

        //OTHER VARS
        #region OTHER VARS

        /// <summary>
        /// The decoupler module. Null if the decoupler module is of type ModuleAnchoredDecoupler.
        /// </summary>
        ModuleDecouple decoupler;
        /// <summary>
        /// The decoupler module. Null if the decoupler module is of type ModuleDecouple.
        /// </summary>
        ModuleAnchoredDecoupler aDecoupler;
        #endregion

        // KSP METHODS
        #region KSP METHODS
        /// <summary>
        /// Called when the part is started by Unity.
        /// </summary>
        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            if (HighLogic.LoadedSceneIsFlight)
            {
                GameEvents.onStageActivate.Add(new EventData<int>.OnEvent(DetermineFailure));

                decoupler = part.Modules.OfType<ModuleDecouple>().FirstOrDefault<ModuleDecouple>();

                if (!decoupler)
                {
                    aDecoupler = part.Modules.OfType<ModuleAnchoredDecoupler>().FirstOrDefault<ModuleAnchoredDecoupler>();
                }

                if (decoupler)
                {
                    if (failure != "")
                    {
                        decoupler.isDecoupled = true;
                    }
                    decoupler.Events["Decouple"].active = false;
                    decoupler.Actions["DecoupleAction"].active = false;
                }
                else if (aDecoupler)
                {
                    if (failure != "")
                    {
                        aDecoupler.isDecoupled = true;
                    }
                    aDecoupler.Events["Decouple"].active = false;
                    aDecoupler.Actions["DecoupleAction"].active = false;
                }
                else
                {
                    Logger.DebugError("Part \"" + part.partName + "\" contains neither a decouple or anchored decoupler module!");
                    return;
                }

                Fields["reliability"].guiActive = false;
            }
        }
        #endregion

        //KSP ACTIONS
        #region KSP ACTIONS
        /// <summary>
        /// KSP Action. Calls DetermineFailure when an action group this is assigned to (if any) is activated.
        /// </summary>
        /// <param name="param">Action parameter</param>
        [KSPAction("Decouple")]
        public void DetermineFailureAction(KSPActionParam param)
        {
            DetermineFailure(-1);
        }
        #endregion

        //KSP EVENTS
        #region KSP EVENTS
        /// <summary>
        /// Called when "Decouple" is selected from the EVA context menu.
        /// </summary>
        [KSPEvent(active = false, guiName = "Re-rig Decoupler", externalToEVAOnly = true, guiActiveUnfocused = true, unfocusedRange = 3f)]
        public void ReRigDecoupler()
        {
            if (FlightGlobals.ActiveVessel.isEVA)
            {
                Part kerbal = FlightGlobals.ActiveVessel.parts[0];

                rocketPartsLeftToFix -= (int)kerbal.RequestResource("RocketParts", (double)System.Math.Min(rocketPartsLeftToFix, 10));

                fixSound.audio.Play();

                if (rocketPartsLeftToFix <= 0)
                {
                    if (decoupler)
                    {
                        decoupler.isDecoupled = false;
                        decoupler.Decouple();
                    }
                    else if (aDecoupler)
                    {
                        aDecoupler.isDecoupled = false;
                        aDecoupler.Decouple();
                    }

                    failure = "";
                }
            }
        }

        /// <summary>
        /// Called when "Bash with Hammer" is selected from the EVA context menu.
        /// </summary>
        [KSPEvent(active = true, guiName = "Bash Decoupler", externalToEVAOnly = true, guiActiveUnfocused = true, unfocusedRange = 3f)]
        public void BashWithHammer()
        {
            bashSound.audio.clip = SoundManager.GetSound("Hammer" + Random.Range(1, 7).ToString());
            bashSound.audio.Play();

            float rand = Random.Range(0f, 1f);

            if (rand < chanceOfExplosionEVA / Mathf.Clamp01(quality / 0.75f))
            {
                part.explode();
            }
            else if (rand >= chanceOfNothingEVA / Mathf.Clamp01(quality / 0.75f))
            {
                if (decoupler)
                {
                    decoupler.isDecoupled = false;
                    decoupler.Decouple();
                }
                else if (aDecoupler)
                {
                    aDecoupler.isDecoupled = false;
                    aDecoupler.Decouple();
                }

                failure = "";
                KMUtil.SetPartHighlight(part, KMUtil.KerbalGreen, Part.HighlightType.OnMouseOver);
            }
        }

        /// <summary>
        /// Runs a test to determine whether the decoupler should fail.
        /// </summary>
        /// <param name="stage">The stage on which this method was called. -1 if called by an action group.</param>
        [KSPEvent(guiActive = true, guiName = "Decouple")]
        public void DetermineFailureEvent()
        {
            DetermineFailure(-1);
        }

        [KSPEvent(active = false, guiActive = false, guiActiveEditor = false, guiActiveUnfocused = false, externalToEVAOnly = true, unfocusedRange = 3f, guiName = "Perform Maintenance")]
        public override void PerformMaintenance()
        {
            
        }
        #endregion

        //OTHER METHODS
        #region OTHER METHODS
        /// <summary>
        /// Runs a test to determine whether the decoupler should fail.
        /// </summary>
        /// <param name="stage">The stage on which this method was called. -1 if called by an action group.</param>
        void DetermineFailure(int stage)
        {
            if (decoupler || aDecoupler)
            {
                if ((stage == part.inverseStage || stage == -1) && FlightGlobals.ActiveVessel == vessel && failure == "")
                {
                    float rand = Random.Range(0f, 1f);

                    if (rand < chanceOfExplosion / Mathf.Clamp01(quality / 0.75f))
                    {
                        part.explode();
                        KMUtil.PostFailure(part, " has exploded due to improper detonator rigging.");
                    }
                    else if (rand < chanceOfNothing / Mathf.Clamp01(quality / 0.75f))
                    {
                        if (decoupler)
                        {
                            decoupler.isDecoupled = true;
                        }
                        else if (aDecoupler)
                        {
                            aDecoupler.isDecoupled = true;
                        }

                        Events["Decouple"].active = true;
                        rocketPartsLeftToFix = rocketPartsNeededToFix;
                        failure = "Decouple failure";
                        KMUtil.PostFailure(part, " failed to decouple due to improper explosive rigging.");
                    }
                    else if (stage == -1)
                    {
                        if (decoupler)
                        {
                            decoupler.Decouple();
                        }
                        else if (aDecoupler)
                        {
                            aDecoupler.Decouple();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the reliability information on this module.
        /// </summary>
        public override void DisplayDesc()
        {
            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical();
            GUILayout.Label("Chances of failure:", HighLogic.Skin.label);
            GUILayout.Label("Silent Malfunction:", HighLogic.Skin.label);
            GUILayout.Label("Explosive Disassembly:", HighLogic.Skin.label);
            GUILayout.Label(" ", HighLogic.Skin.label);
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            GUILayout.Label(" ", HighLogic.Skin.label);
            GUILayout.Label((chanceOfNothing - chanceOfExplosion).ToString("##0.#####%"), HighLogic.Skin.label);
            GUILayout.Label(chanceOfExplosion.ToString("##0.#####%"), HighLogic.Skin.label);
            GUILayout.Label(" ", HighLogic.Skin.label);
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
        }
        #endregion
    }
}
