using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KerbalMechanics
{
    /// <summary>
    /// Represents a function which will be run in order to inject any given module.
    /// </summary>
    /// <param name="node">The config node of the module to be injected.</param>
    /// <param name="aPart">The AvailablePart to inject the module into.</param>
    public delegate void ModuleInjection(ConfigNode node, AvailablePart aPart);

    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    class ModuleInjectorPreStart : MonoBehaviour
    {
        /// <summary>
        /// The list of ModuleInjection objects to fire if encountered in the injection file. Used to inject actual modules.
        /// </summary>
        public Dictionary<string, ModuleInjection> moduleInjections;
        /// <summary>
        /// The list of ModuleInjection objects to fire if encountered in the injection file. Used to inject resources.
        /// </summary>
        public Dictionary<string, ModuleInjection> resourceInjections;

        /// <summary>
        /// The static instance of this object.
        /// </summary>
        private static ModuleInjectorPreStart instance;
        /// <summary>
        /// Gets the static instance of this object.
        /// </summary>
        public static ModuleInjectorPreStart Instance
        {
            get { return instance; }
        }

        /// <summary>
        /// Returns true if this object is already initialized.
        /// </summary>
        public static bool IsInitialized
        {
            get { return (instance != null); }
        }

        /// <summary>
        /// Fired when this object is created, before Start.
        /// </summary>
        void Awake()
        {
            moduleInjections = new Dictionary<string, ModuleInjection>();
            resourceInjections = new Dictionary<string, ModuleInjection>();
            instance = this;
            DontDestroyOnLoad(gameObject);

            
        }
    }
}
