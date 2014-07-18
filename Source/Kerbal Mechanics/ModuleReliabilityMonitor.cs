using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KerbalMechanics
{
    class ModuleReliabilityMonitor : ModuleReliabilityBase
    {
        //PROPERTIES
        #region PROPERTIES
        public override string ModuleName
        {
            get { return "Part Monitor"; }
        }
        #endregion

        //OTHER VARS
        #region OTHER VARS
        List<ModuleReliabilityManager> managerList;

        public bool alreadyStarted = false;
        #endregion

        //KSP METHODS
        #region KSP METHODS
        /// <summary>
        /// Called when the module is started.
        /// </summary>
        /// <param name="state">The start state.</param>
        public override void OnStart(PartModule.StartState state)
        {
            if (!alreadyStarted)
            {
                foreach (Part p in vessel.Parts)
                {
                    ModuleReliabilityMonitor m = p.Modules.OfType<ModuleReliabilityMonitor>().FirstOrDefault<ModuleReliabilityMonitor>();

                    if (m)
                    {
                        m.alreadyStarted = true;
                    }
                }
            }
            
            base.OnStart(state);
        }

        /// <summary>
        /// Called when the part is updated.
        /// </summary>
        public override void OnUpdate()
        {
            if (!alreadyStarted)
            {
                managerList = new List<ModuleReliabilityManager>();

                foreach (Part p in vessel.Parts)
                {
                    ModuleReliabilityManager m = p.Modules.OfType<ModuleReliabilityManager>().FirstOrDefault<ModuleReliabilityManager>();

                    if (m)
                    {
                        managerList.Add(m);
                    }
                }
            }

            base.OnUpdate();
        }
        #endregion

        //KSP EVENTS
        #region KSP EVENTS
        public override void PerformMaintenance()
        {
            throw new NotImplementedException();
        }
        #endregion

        //OTHER METHODS
        #region OTHER METHODS
        public override void DisplayDesc()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
