using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Kerbal_Mechanics
{
    public delegate void ModuleInjection(ConfigNode node, AvailablePart aPart);

    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    class ModuleInjectorPreStart : MonoBehaviour
    {
        public Dictionary<string, ModuleInjection> moduleInjections;
        public Dictionary<string, ModuleInjection> resourceInjections;

        private static ModuleInjectorPreStart instance;

        public static ModuleInjectorPreStart Instance
        {
            get { return instance; }
        }

        public static bool IsInitialized
        {
            get { return (instance != null); }
        }

        void Awake()
        {
            moduleInjections = new Dictionary<string, ModuleInjection>();
            resourceInjections = new Dictionary<string, ModuleInjection>();
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}
