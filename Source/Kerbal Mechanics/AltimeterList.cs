using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kerbal_Mechanics
{
    class AltimeterList
    {
        public Vessel vessel;

        public List<ModuleReliabilityAltimeter> altimeterList;

        public AltimeterList (Vessel ship, ModuleReliabilityAltimeter initial)
        {
            vessel = ship;
            altimeterList = new List<ModuleReliabilityAltimeter>();
            altimeterList.Add(initial);
        }
    }
}
