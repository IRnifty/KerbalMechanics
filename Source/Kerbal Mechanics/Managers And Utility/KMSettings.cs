using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KerbalMechanics
{
    class KMSettings
    {
        public static KMSettings Instance
        {
            get { return instance; }
        }

        static KMSettings instance;

        public bool stopTimeWarpOnFailure = true;
        public bool alertMessageOnFailure = true;
        public bool highlightFailedPart = true;

        public static void Init()
        {
            instance = new KMSettings();
            instance.LoadSettings();
        }

        public void LoadSettings()
        {
            ConfigNode settings = ConfigNode.Load("GameData/KerbalMechanics/Settings.cfg");

            if (settings != null)
            {
                settings = settings.GetNode("SETTINGS");

                if (settings.HasValue("stopTimeWarpOnFailure")) { stopTimeWarpOnFailure = bool.Parse(settings.GetValue("stopTimeWarpOnFailure")); }
                if (settings.HasValue("alertMessageOnFailure")) { alertMessageOnFailure = bool.Parse(settings.GetValue("alertMessageOnFailure")); }
                if (settings.HasValue("highlightFailedPart")) { highlightFailedPart = bool.Parse(settings.GetValue("highlightFailedPart")); }
            }
            else
            {
                SaveSettings();
            }
        }

        public void SaveSettings()
        {
            ConfigNode settings = new ConfigNode("SETTINGS");
            settings.AddValue("stopTimeWarpOnFailure", stopTimeWarpOnFailure);
            settings.AddValue("alertMessageOnFailure", alertMessageOnFailure);
            settings.AddValue("highlightFailedPart", highlightFailedPart);

            ConfigNode root = new ConfigNode();
            root.AddNode(settings);

            root.Save("GameData/KerbalMechanics/Settings.cfg");
        }
    }
}
