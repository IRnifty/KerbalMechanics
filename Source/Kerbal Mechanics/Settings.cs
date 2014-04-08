using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Toolbar;

namespace Kerbal_Mechanics
{
    class Settings
    {
        ConfigNode settingsNode;

        static Settings instance;

        public static void Initialize()
        {
            instance = new Settings();
        }

        Settings()
        {
            settingsNode = ConfigNode.Load(KSPUtil.ApplicationRootPath + "GameData/KerbalMechanics/Config/Settings.cfg");
        }

        private class SettingsVisibility : IVisibility
        {
            public bool Visible
            {
                get { return (HighLogic.LoadedScene == GameScenes.EDITOR || HighLogic.LoadedScene == GameScenes.FLIGHT || HighLogic.LoadedScene == GameScenes.SPACECENTER); }
            }
        }
    }
}
