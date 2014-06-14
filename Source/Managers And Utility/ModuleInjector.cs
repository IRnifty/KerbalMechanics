using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Kerbal_Mechanics
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    class ModuleInjector : MonoBehaviour
    {
        public void Awake()
        {
            ModuleInjectorPreStart injection = ModuleInjectorPreStart.Instance;

            try
            {
                injection.moduleInjections.Add("ModuleReliabilityIgnitor", new ModuleInjection(AddIgnitor));
                injection.moduleInjections.Add("ModuleReliabilityCooling", new ModuleInjection(AddCooling));
                injection.moduleInjections.Add("ModuleReliabilityGimbal", new ModuleInjection(AddGimbal));
                injection.moduleInjections.Add("ModuleReliabilityDecoupler", new ModuleInjection(AddDecoupler));
                injection.moduleInjections.Add("ModuleReliabilityLight", new ModuleInjection(AddLight));
                injection.resourceInjections.Add("RocketParts", new ModuleInjection(AddRocketParts));

                ConfigNode node = ConfigNode.Load(KSPUtil.ApplicationRootPath + "GameData/KerbalMechanics/Injections.cfg") ?? new ConfigNode();
                InjectModules(node);
            }
            catch
            {
                Logger.DebugLog("Injection halted. Modules and resources already injected.");
            }
        }

        void InjectModules(ConfigNode node)
        {
            foreach (ConfigNode partNode in node.GetNodes("PART"))
            {
                bool addedReliability = false;

                // Check if the part node has a name
                if (!partNode.HasValue("partName"))
                {
                    Logger.DebugWarning("PART node missing partName value!");
                    continue;
                }

                // Find the part
                string partName = partNode.GetValue("partName").Replace('_', '.');
                AvailablePart aPart = PartLoader.getPartInfoByName(partName);
                if (aPart == null)
                {
                    Logger.DebugError("Part \"" + partName + "\" not found in PartLoader!");
                    continue;
                }

                // Iterate through all MODULE nodes in the PART node, and inject them
                ConfigNode[] moduleNodes = partNode.GetNodes("MODULE");
                foreach (ConfigNode moduleNode in moduleNodes)
                {
                    if (!moduleNode.HasValue("moduleName"))
                    {
                        Logger.DebugError("Missing moduleName value in PART node \"" + partName + "\"!");
                        continue;
                    }

                    // Iterate through modules available for injection, and inject if it matches the node's moduleName value.
                    string moduleToAdd = moduleNode.GetValue("moduleName");
                    foreach (KeyValuePair<string, ModuleInjection> entry in ModuleInjectorPreStart.Instance.moduleInjections)
                    {
                        if (entry.Key == moduleToAdd)
                        {
                            entry.Value(moduleNode, aPart);
                        }
                    }

                    addedReliability = (addedReliability || moduleToAdd.Contains("Reliability"));
                }

                if (addedReliability)
                {
                    //Add the Monitor module
                    AddManager(aPart);
                }

                // Iterate through all RESOURCE nodes in the PART node, and inject them
                ConfigNode[] resourceNodes = partNode.GetNodes("RESOURCE");
                foreach (ConfigNode resourceNode in resourceNodes)
                {
                    if (!resourceNode.HasValue("resourceName"))
                    {
                        Logger.DebugError("Missing resourceName value in PART node \"" + partName + "\"!");
                        continue;
                    }

                    // Iterate through resources available for injection, and inject if it matches the node's resourceName value.
                    string resourceToAdd = resourceNode.GetValue("resourceName");
                    foreach (KeyValuePair<string, ModuleInjection> entry in ModuleInjectorPreStart.Instance.resourceInjections)
                    {
                        if (entry.Key == resourceToAdd)
                        {
                            entry.Value(resourceNode, aPart);
                        }
                    }
                }
            }
        }

        void AddRocketParts(ConfigNode node, AvailablePart aPart)
        {
            // Get or add module
            PartResource[] resources = aPart.partPrefab.GetComponents<PartResource>();

            string resourceName = node.GetValue("resourceName");

            foreach (PartResource r in resources)
            {
                if (r.name == resourceName)
                {
                    Logger.DebugWarning("Resource \"" + resourceName + "\" already added to \"" + aPart.name + "\"!");
                    return;
                }
            }

            PartResource resource = aPart.partPrefab.gameObject.AddComponent<PartResource>();
            if (!resource)
            {
                Logger.DebugError("Problem adding resource to engine!");
                return;
            }
            resource.SetInfo(PartResourceLibrary.Instance.resourceDefinitions[resourceName]);
            resource.maxAmount = double.Parse(node.GetValue("maxAmount"));
            resource.flowState = true;
            resource.flowMode = PartResource.FlowMode.Both;
            resource.part = aPart.partPrefab;
        }

        void AddIgnitor(ConfigNode node, AvailablePart aPart)
        {
            // Get or add module
            ModuleReliabilityIgnitor rModule = aPart.partPrefab.GetComponent<ModuleReliabilityIgnitor>();
            if (rModule)
            {
                Logger.DebugWarning("ModuleReliabilityIgnitor already added to \"" + aPart.name + "\"!");
            }
            else
            {
                rModule = aPart.partPrefab.AddModule("ModuleReliabilityIgnitor") as ModuleReliabilityIgnitor;
                if (!rModule)
                {
                    Logger.DebugError("Problem adding module to engine!");
                    return;
                }
            }

            //Configure the base values
            ConfigureBaseValues(node, rModule);

            if (node.HasValue("startingChanceToFailPerfect")) { rModule.startingChanceToFailPerfect = float.Parse(node.GetValue("startingChanceToFailPerfect")); }
            if (node.HasValue("startingChanceToFailTerrible")) { rModule.startingChanceToFailTerrible = float.Parse(node.GetValue("startingChanceToFailTerrible")); }
            if (node.HasValue("chanceKickWillDestroy")) { rModule.chanceKickWillDestroy = float.Parse(node.GetValue("chanceKickWillDestroy")); }
            if (node.HasValue("chanceKickWillFix")) { rModule.chanceKickWillFix = float.Parse(node.GetValue("chanceKickWillFix")); }
        }

        void AddCooling(ConfigNode node, AvailablePart aPart)
        {
            // Get or add module
            ModuleReliabilityCooling rModule = aPart.partPrefab.GetComponent<ModuleReliabilityCooling>();
            if (rModule)
            {
                Logger.DebugWarning("ModuleReliabilityCooling already added to \"" + aPart.name + "\"!");
            }
            else
            {
                rModule = aPart.partPrefab.AddModule("ModuleReliabilityCooling") as ModuleReliabilityCooling;
                if (!rModule)
                {
                    Logger.DebugError("Problem adding module to engine!");
                    return;
                }
            }

            //Configure the base values
            ConfigureBaseValues(node, rModule);

            if (node.HasValue("runningChanceToFailPerfect")) { rModule.runningChanceToFailPerfect = float.Parse(node.GetValue("runningChanceToFailPerfect")); }
            if (node.HasValue("runningChanceToFailTerrible")) { rModule.runningChanceToFailTerrible = float.Parse(node.GetValue("runningChanceToFailTerrible")); }
            if (node.HasValue("chanceKickWillDestroy")) { rModule.chanceKickWillDestroy = float.Parse(node.GetValue("chanceKickWillDestroy")); }
            if (node.HasValue("chanceKickWillFix")) { rModule.chanceKickWillFix = float.Parse(node.GetValue("chanceKickWillFix")); }
        }

        void AddGimbal(ConfigNode node, AvailablePart aPart)
        {
            // Get or add module
            ModuleReliabilityGimbal rModule = aPart.partPrefab.GetComponent<ModuleReliabilityGimbal>();
            if (rModule)
            {
                Logger.DebugWarning("ModuleReliabilityGimbal already added to \"" + aPart.name + "\"!");
            }
            else
            {
                rModule = aPart.partPrefab.AddModule("ModuleReliabilityGimbal") as ModuleReliabilityGimbal;
                if (!rModule)
                {
                    Logger.DebugError("Problem adding module to engine!");
                    return;
                }
            }

            //Configure the base values
            ConfigureBaseValues(node, rModule);

            if (node.HasValue("chanceToFailPerfect")) { rModule.chanceToFailPerfect = float.Parse(node.GetValue("chanceToFailPerfect")); }
            if (node.HasValue("chanceToFailTerrible")) { rModule.chanceToFailTerrible = float.Parse(node.GetValue("chanceToFailTerrible")); }
            if (node.HasValue("chanceKickWillLock")) { rModule.chanceKickWillLock = float.Parse(node.GetValue("chanceKickWillLock")); }
            if (node.HasValue("chanceKickWillFix")) { rModule.chanceKickWillFix = float.Parse(node.GetValue("chanceKickWillFix")); }
        }

        void AddDecoupler(ConfigNode node, AvailablePart aPart)
        {
            // Get or add module
            ModuleReliabilityDecoupler rModule = aPart.partPrefab.GetComponent<ModuleReliabilityDecoupler>();
            if (rModule)
            {
                Logger.DebugWarning("ModuleReliabilityDecoupler already added to \"" + aPart.name + "\"!");
            }
            else
            {
                rModule = aPart.partPrefab.AddModule("ModuleReliabilityDecoupler") as ModuleReliabilityDecoupler;
                if (!rModule)
                {
                    Logger.DebugError("Problem adding module to decoupler!");
                    return;
                }
            }

            //Configure the base values
            ConfigureBaseValues(node, rModule);

            if (node.HasValue("chanceOfExplosion")) { rModule.chanceOfExplosion = float.Parse(node.GetValue("chanceOfExplosion")); }
            if (node.HasValue("chanceOfExplosionEVA")) { rModule.chanceOfExplosionEVA = float.Parse(node.GetValue("chanceOfExplosionEVA")); }
            if (node.HasValue("chanceOfNothing")) { rModule.chanceOfNothing = float.Parse(node.GetValue("chanceOfNothing")); }
            if (node.HasValue("chanceOfNothingEVA")) { rModule.chanceOfNothingEVA = float.Parse(node.GetValue("chanceOfNothingEVA")); }
        }

        void AddLight(ConfigNode node, AvailablePart aPart)
        {
            // Get or add module
            ModuleReliabilityLight rModule = aPart.partPrefab.GetComponent<ModuleReliabilityLight>();
            if (rModule)
            {
                Logger.DebugWarning("ModuleReliabilityDecoupler already added to \"" + aPart.name + "\"!");
            }
            else
            {
                rModule = aPart.partPrefab.AddModule("ModuleReliabilityLight") as ModuleReliabilityLight;
                if (!rModule)
                {
                    Logger.DebugError("Problem adding module to decoupler!");
                    return;
                }
            }

            //Configure the base values
            ConfigureBaseValues(node, rModule);

            if (node.HasValue("chanceToFailPerfect")) { rModule.chanceToFailPerfect = float.Parse(node.GetValue("chanceToFailPerfect")); }
            if (node.HasValue("chanceToFailTerrible")) { rModule.chanceToFailTerrible = float.Parse(node.GetValue("chanceToFailTerrible")); }
            if (node.HasValue("rocketPartsNeededFlickering")) { rModule.rocketPartsNeededFlickering = int.Parse(node.GetValue("rocketPartsNeededFlickering")); }
        }

        void AddManager(AvailablePart aPart)
        {
            // Get or add module
            ModuleReliabilityManager rModule = aPart.partPrefab.GetComponent<ModuleReliabilityManager>();
            if (!rModule)
            {
                rModule = aPart.partPrefab.AddModule("ModuleReliabilityManager") as ModuleReliabilityManager;
                if (!rModule)
                {
                    Logger.DebugError("Problem adding module to decoupler!");
                    return;
                }
                rModule.PreStart();
                SetInfo(aPart, rModule);
            }
        }

        void SetInfo(AvailablePart aPart, ModuleReliabilityManager module)
        {
            aPart.moduleInfos.Add(new AvailablePart.ModuleInfo()
            {
                info = module.GetDesc().Trim(),
                moduleName = "Reliability"
            });
        }

        void ConfigureBaseValues(ConfigNode node, ModuleReliabilityBase module)
        {
            if (node.HasValue("quality")) { module.quality = float.Parse(node.GetValue("quality")); }
            if (node.HasValue("reliability")) { module.reliability = float.Parse(node.GetValue("reliability")); }
            if (node.HasValue("failure")) { module.failure = node.GetValue("failure"); }
            if (node.HasValue("rocketPartsNeededToFix")) { module.rocketPartsNeededToFix = int.Parse(node.GetValue("rocketPartsNeededToFix")); }
            if (node.HasValue("reliabilityDrainPerfect")) { module.reliabilityDrainPerfect = float.Parse(node.GetValue("reliabilityDrainPerfect")); }
            if (node.HasValue("reliabilityDrainTerrible")) { module.reliabilityDrainTerrible = float.Parse(node.GetValue("reliabilityDrainTerrible")); }
        }
    }
}