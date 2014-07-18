using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KerbalMechanics
{
    class AltimeterModuleList
    {
        public Vessel vessel;

        public List<ModuleReliabilityAltimeter> altimeterList;

        public AltimeterModuleList (Vessel ship, ModuleReliabilityAltimeter initial)
        {
            vessel = ship;
            altimeterList = new List<ModuleReliabilityAltimeter>();
            altimeterList.Add(initial);
        }
    }
}
