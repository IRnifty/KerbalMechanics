using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KerbalMechanics
{
    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    class EditorLoader : MonoBehaviour
    {
        List<string> blackList;

        public List<string> LeakBlackList
        {
            get { return blackList; }
        }

        static EditorLoader instance;

        public static EditorLoader Instance
        {
            get { return instance; }
        }

        public EditorLoader() : base()
        {
            instance = this;

            ConfigNode blackListNode = ConfigNode.Load(KSPUtil.ApplicationRootPath + "GameData/KerbalMechanics/Blacklist.cfg") ?? new ConfigNode();
            blackListNode = blackListNode.GetNode("BLACKLIST") ?? new ConfigNode("BLACKLIST");
            blackList = new List<string>();
            string[] blVals = blackListNode.GetValues("ignore");
            foreach(string val in blVals)
            {
                blackList.Add(val);
            }
        }
    }
}
