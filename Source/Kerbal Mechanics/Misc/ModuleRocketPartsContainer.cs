using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KerbalMechanics
{
    class ModuleRocketPartsContainer : PartModule
    {
        //KSP FIELDS
        #region KSP FIELDS

        [KSPEvent(active = true, guiActive = false, guiActiveEditor = false, guiActiveUnfocused = true, externalToEVAOnly = true, unfocusedRange = 3f, guiName = "")]
        public void GetRocketPartsAmount()
        {

        }
        #endregion

        //OTHER VARS
        #region OTHER VARS
        /// <summary>
        /// The RocketParts resource attached to this part
        /// </summary>
        PartResource rocketParts;
        #endregion

        //KSP METHODS
        #region KSP METHODS
        /// <summary>
        /// Called when this part is started.
        /// </summary>
        /// <param name="state">The start state.</param>
        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);

            foreach (PartResource r in part.Resources)
            {
                if (r.resourceName == "RocketParts")
                {
                    rocketParts = r;
                    break;
                }
            }

            if (!rocketParts)
            {
                Logger.DebugError("Cannot find RocketParts resource on \"" + part.name + "\"!");
            }

            rocketParts.flowMode = PartResource.FlowMode.Both;
            part.fuelCrossFeed = false;
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            Events["GetRocketPartsAmount"].guiName = "Amount: " + ((int)rocketParts.amount).ToString() + "/" + ((int)rocketParts.maxAmount).ToString();
        }
        #endregion

        //KSP EVENTS
        #region KSP EVENTS
        /// <summary>
        /// Takes RocketParts resource from this part and places them into the EVA vessel.
        /// </summary>
        [KSPEvent(active = true, guiActive = false, guiActiveEditor = false, guiActiveUnfocused = true, externalToEVAOnly = true, unfocusedRange = 3f, guiName = "Take Parts")]
        public void TakeParts()
        {
            if (!FlightGlobals.ActiveVessel.isEVA)
            {
                Logger.DebugError("Attempt to take parts made from non-EVA vessel!");
                return;
            }

            Part kerbalTaking = FlightGlobals.ActiveVessel.parts[0];

            PartResource kerbalResourceBay = null;

            foreach (PartResource r in kerbalTaking.Resources)
            {
                if (r.resourceName == "RocketParts")
                {
                    kerbalResourceBay = r;
                    break;
                }
            }

            if (!kerbalResourceBay)
            {
                Logger.DebugError("No resource named RocketParts found in Kerbal \"" + kerbalTaking.partInfo.title + "\"!");
                return;
            }

            double amountTransferring = part.RequestResource("RocketParts", kerbalResourceBay.maxAmount - kerbalResourceBay.amount);

            kerbalResourceBay.amount += amountTransferring;
        }

        /// <summary>
        /// Stores RocketParts resource from the EVA vessel and places them into this part.
        /// </summary>
        [KSPEvent(active = true, guiActive = false, guiActiveEditor = false, guiActiveUnfocused = true, externalToEVAOnly = true, unfocusedRange = 3f, guiName = "Store Parts")]
        public void StoreParts()
        {
            if (!FlightGlobals.ActiveVessel.isEVA)
            {
                Logger.DebugError("Attempt to store parts made from non-EVA vessel!");
                return;
            }

            Part kerbalStoring = FlightGlobals.ActiveVessel.parts[0];

            PartResource kerbalResourceBay = null;

            foreach (PartResource r in kerbalStoring.Resources)
            {
                if (r.resourceName == "RocketParts")
                {
                    kerbalResourceBay = r;
                    break;
                }
            }

            if (!kerbalResourceBay)
            {
                Logger.DebugError("No resource named RocketParts found in Kerbal \"" + kerbalStoring.partInfo.title + "\"!");
                return;
            }

            Logger.DebugLog("Amount in Kerbal: " + kerbalResourceBay.amount.ToString());
            double amountTransferring = kerbalStoring.RequestResource("RocketParts", rocketParts.maxAmount - rocketParts.amount);
            Logger.DebugLog("Amount transferring: " + amountTransferring.ToString());
            
            rocketParts.amount += amountTransferring;
        }
        #endregion
    }
}
