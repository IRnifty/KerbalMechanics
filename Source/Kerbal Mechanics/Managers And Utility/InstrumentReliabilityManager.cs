using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KerbalMechanics
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    class InstrumentReliabilityManager : MonoBehaviour
    {
        //Altimeter
        Tumblers realAltimeter;
        Tumblers dummyAltimeter;
        List<ModuleReliabilityAltimeter> altimeterModules;
        double falseAltReading = -1;

        //Static Instance
        static InstrumentReliabilityManager instance;
        public static InstrumentReliabilityManager Instance
        {
            get { return instance; }
        }

        //Misc.
        int lastNumOfParts;

        void Awake ()
        {
            instance = this;

            realAltimeter = FlightUIController.fetch.alt;
            dummyAltimeter = Instantiate(realAltimeter) as Tumblers;
            dummyAltimeter.transform.parent = realAltimeter.transform.parent;
            dummyAltimeter.transform.position -= Vector3.right * 1000f;
            dummyAltimeter.enabled = false;
            FlightUIController.fetch.alt = dummyAltimeter;
            altimeterModules = new List<ModuleReliabilityAltimeter>();
        }

        void Start ()
        {
            RecompileList(FlightGlobals.ActiveVessel);
            GameEvents.onVesselChange.Add(new EventData<Vessel>.OnEvent(RecompileList));
            lastNumOfParts = FlightGlobals.ActiveVessel.Parts.Count;
        }

        void LateUpdate ()
        {
            if (lastNumOfParts != FlightGlobals.ActiveVessel.Parts.Count)
            {
                RecompileList(FlightGlobals.ActiveVessel);
            }

            bool allAltsBroke = altimeterModules.Count > 0;
            foreach(ModuleReliabilityAltimeter alt in altimeterModules)
            {
                if (alt.failure == "")
                {
                    allAltsBroke = false;
                    break;
                }
            }

            if (allAltsBroke)
            {
                if (falseAltReading == -1)
                {
                    falseAltReading = (double)UnityEngine.Random.Range(0, int.MaxValue);
                }
                realAltimeter.setValue(falseAltReading);
            }
            else
            {
                if (falseAltReading != -1)
                {
                    falseAltReading = -1;
                }
                realAltimeter.setValue(dummyAltimeter.value);
            }
        }

        void RecompileList(Vessel vessel)
        {
            if (vessel == FlightGlobals.ActiveVessel)
            {
                lastNumOfParts = FlightGlobals.ActiveVessel.Parts.Count;

                altimeterModules = new List<ModuleReliabilityAltimeter>();
                foreach (Part part in FlightGlobals.ActiveVessel.Parts)
                {
                    ModuleReliabilityAltimeter alt = part.Modules.OfType<ModuleReliabilityAltimeter>().FirstOrDefault<ModuleReliabilityAltimeter>();

                    if (alt)
                    {
                        altimeterModules.Add(alt);
                    }
                }
            }
        }
    }
}
