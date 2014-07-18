using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KerbalMechanics
{
    abstract class ModuleReliabilityInstrument : ModuleReliabilityBase
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
        protected double CurrentOverGees
        {
            get { return System.Math.Max(vessel.geeForce - 5, 0) * 10; }
        }

        /// <summary>
        /// The module name.
        /// </summary>
        public abstract override string ModuleName
        {
            get;
        }
        #endregion

        public abstract override void PerformMaintenance();

        public abstract override void DisplayDesc();
    }
}
