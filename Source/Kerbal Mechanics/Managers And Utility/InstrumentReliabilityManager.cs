using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Kerbal_Mechanics
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    class InstrumentReliabilityManager : MonoBehaviour
    {
        //Altimeter
        Tumblers realAltimeter;
        Tumblers dummyAltimeter;
        List<AltimeterList> vessels;
        List<ModuleReliabilityAltimeter> altimeterModules;
        double falseAltReading = -1;

        //Static Instance
        static InstrumentReliabilityManager instance;
        public static InstrumentReliabilityManager Instance
        {
            get { return instance; }
        }

        void Awake ()
        {
            instance = this;

            realAltimeter = FlightUIController.fetch.alt;
            dummyAltimeter = Instantiate(realAltimeter) as Tumblers;
            dummyAltimeter.transform.parent = realAltimeter.transform.parent;
            dummyAltimeter.transform.position -= Vector3.right * 1000f;
            dummyAltimeter.enabled = false;
            FlightUIController.fetch.alt = dummyAltimeter;
            vessels = new List<AltimeterList>();
            altimeterModules = new List<ModuleReliabilityAltimeter>();
        }

        void Start ()
        {
            
        }

        void LateUpdate ()
        {
            bool allAltsBroke = true;
            foreach(AltimeterList vessel in vessels)
            {
                if (vessel.vessel == FlightGlobals.ActiveVessel && !FlightGlobals.ActiveVessel.isEVA)
                {
                    foreach(ModuleReliabilityAltimeter alt in vessel.altimeterList)
                    {
                        if (alt.failure == "")
                        {
                            allAltsBroke = false;
                            break;
                        }
                    }
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
                realAltimeter.setValue(dummyAltimeter.value);
            }
        }

        public static void AddAltimetermodule(ModuleReliabilityAltimeter altimeter)
        {
            foreach(AltimeterList l in instance.vessels)
            {
                if (l.vessel == altimeter.vessel)
                {
                    l.altimeterList.Add(altimeter);
                    return;
                }
            }

            instance.vessels.Add(new AltimeterList(altimeter.vessel, altimeter));
        }
    }
}
