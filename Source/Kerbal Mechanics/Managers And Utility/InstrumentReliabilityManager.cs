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

        //Thrust Gauge
        Gauge realThrust;
        Gauge dummyThrust;
        List<ModuleReliabilityThrust> thrustModules;
        float falseThrustReading = -1;

        //Monitor
        List<ModuleReliabilityMonitor> monitorModules;
        List<ModuleReliabilityManager> managerModules;
        Rect windowRect;
        public bool displayingGUI = false;
        public bool highlightingReliability = false;
        public bool highlightFailedParts = false;
        public double averageMonitorReliability = 0;

        //Misc.
        int lastNumOfParts;

        //Static Instance
        static InstrumentReliabilityManager instance;
        public static InstrumentReliabilityManager Instance
        {
            get { return instance; }
        }

        void Awake ()
        {
            instance = this;

            //Altimeter
            realAltimeter = FlightUIController.fetch.alt;
            dummyAltimeter = Instantiate(realAltimeter) as Tumblers;
            dummyAltimeter.transform.parent = realAltimeter.transform.parent;
            dummyAltimeter.transform.position -= Vector3.right * 1000f;
            dummyAltimeter.enabled = false;
            FlightUIController.fetch.alt = dummyAltimeter;
            altimeterModules = new List<ModuleReliabilityAltimeter>();

            //Thrust Gauge
            realThrust = FlightUIController.fetch.thr;
            dummyThrust = Instantiate(realThrust) as Gauge;
            dummyThrust.transform.parent = realThrust.transform.parent;
            dummyThrust.transform.position -= Vector3.right * 1000f;
            dummyThrust.enabled = false;
            FlightUIController.fetch.thr = dummyThrust;
            thrustModules = new List<ModuleReliabilityThrust>();

            //Monitor
            monitorModules = new List<ModuleReliabilityMonitor>();
            managerModules = new List<ModuleReliabilityManager>();
        }

        void Start ()
        {
            RecompileLists(FlightGlobals.ActiveVessel);
            GameEvents.onVesselChange.Add(new EventData<Vessel>.OnEvent(RecompileLists));
            lastNumOfParts = FlightGlobals.ActiveVessel.Parts.Count;

            windowRect = new Rect(20f, 20f, 200f, 250f);
        }

        void LateUpdate ()
        {
            if (lastNumOfParts != FlightGlobals.ActiveVessel.Parts.Count)
            {
                RecompileLists(FlightGlobals.ActiveVessel);
            }

            #region Altimeters
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
            #endregion

            #region Thrust Gauges
            bool allThrBroke = thrustModules.Count > 0;
            foreach (ModuleReliabilityThrust thr in thrustModules)
            {
                if (thr.failure == "")
                {
                    allThrBroke = false;
                    break;
                }
            }
            if (allThrBroke)
            {
                if (falseThrustReading == -1f)
                {
                    falseThrustReading = UnityEngine.Random.Range(0f, 1f);
                }
                realThrust.setValue(falseThrustReading);
            }
            else
            {
                if (falseThrustReading != -1f)
                {
                    falseThrustReading = -1f;
                }
                realThrust.setValue(dummyThrust.Value);
            }
            #endregion

            #region Reliability Monitors
            averageMonitorReliability = 0;
            foreach(ModuleReliabilityMonitor mon in monitorModules)
            {
                averageMonitorReliability += mon.reliability;
            }
            averageMonitorReliability /= monitorModules.Count;
            #endregion
        }

        void OnGUI()
        {
            if (displayingGUI)
            {
                windowRect = GUI.Window(GetInstanceID(), windowRect, DisplayWindow, "Ship Status Report", HighLogic.Skin.window);
            }
        }

        void DisplayWindow(int windowID)
        {
            //X Button
            if (GUI.Button(new Rect(windowRect.width - 25f, 5f, 20f, 20f), "X", HighLogic.Skin.button))
            {
                displayingGUI = false;
            }

            GUILayout.BeginVertical();

            highlightFailedParts = GUILayout.Toggle(highlightFailedParts, "Highlight Failed Parts", HighLogic.Skin.toggle);
            highlightingReliability = GUILayout.Toggle(highlightingReliability, "Map Ship Status", HighLogic.Skin.toggle);

            GUILayout.Label(windowRect.ToString(), HighLogic.Skin.label);

            GUILayout.EndVertical();

            GUI.DragWindow();
        }

        void RecompileLists(Vessel vessel)
        {
            if (vessel == FlightGlobals.ActiveVessel)
            {
                lastNumOfParts = FlightGlobals.ActiveVessel.Parts.Count;

                altimeterModules = new List<ModuleReliabilityAltimeter>();
                thrustModules = new List<ModuleReliabilityThrust>();
                monitorModules = new List<ModuleReliabilityMonitor>();
                managerModules = new List<ModuleReliabilityManager>();
                foreach (Part part in FlightGlobals.ActiveVessel.Parts)
                {
                    ModuleReliabilityAltimeter alt = part.Modules.OfType<ModuleReliabilityAltimeter>().FirstOrDefault<ModuleReliabilityAltimeter>();

                    if (alt)
                    {
                        altimeterModules.Add(alt);
                    }

                    ModuleReliabilityThrust thr = part.Modules.OfType<ModuleReliabilityThrust>().FirstOrDefault<ModuleReliabilityThrust>();

                    if (thr)
                    {
                        thrustModules.Add(thr);
                    }

                    ModuleReliabilityMonitor mon = part.Modules.OfType<ModuleReliabilityMonitor>().FirstOrDefault<ModuleReliabilityMonitor>();

                    if (mon)
                    {
                        monitorModules.Add(mon);
                    }

                    ModuleReliabilityManager man = part.Modules.OfType<ModuleReliabilityManager>().FirstOrDefault<ModuleReliabilityManager>();

                    if (man)
                    {
                        managerModules.Add(man);
                    }
                }
            }
        }
    }
}
