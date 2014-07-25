using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KerbalMechanics
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    class LoadingLoader : MonoBehaviour
    {
        void Awake()
        {
            string[] args = Environment.GetCommandLineArgs();

            foreach(string arg in args)
            {
                if (arg == "-KMDEBUG")
                {
                    KMUtil.DebugDeclared = true;
                }
            }
        }
    }
}
