using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using HarmonyLib;
using HarmonyLib.Tools;
using Photon.Pun;
using UnityEngine;

namespace Cerveza_Cristal;

// Class will be a singleton.
public sealed class ModPatches
{
    // Instance management
    private static readonly Lazy<ModPatches> lazy
    = new Lazy<ModPatches>(() => new ModPatches());
    public static ModPatches Instance { get { return lazy.Value; } }

    // Instance Variables
    public LevelTypes SelectedLevel { get; set; } = LevelTypes.NONE;

    public ManualLogSource Logger { get; set; } = null;

    private Harmony _harmony { get; set; } = null;

    private ModPatches()
    {
        // Harmony test
        HarmonyFileLog.Enabled = true;
        _harmony = new Harmony(Utils.UNIQUE_ORG_STRING);
    }

    public void ApplyPatches()
    {
        _harmony.PatchAll();
    }


    // Patch classes
    [HarmonyPatch(typeof(LevelGenerator), "Start")]
    public static class ModAssetRestorePatch
    {
        static void Postfix()
        {
            Dictionary<string, GameObject> singleplayerPool = ModEntry.GetSingleplayerPool();

            if (singleplayerPool != null)
            {
                foreach ((GameObject, ValuableAddition) regEntry in ModEntry.Instance.ModValuableRegistry.RegistryDictionary.Values)
                {
                    singleplayerPool.Add(ModEntry.Instance.ModValuableRegistry.GetRegistryName(regEntry.Item2), regEntry.Item1);
                }
            }

            // Reset the multiplayer pool.
            PhotonNetwork.PrefabPool = ModEntry.Instance.MultiplayerPool;
        }
    }

    [HarmonyPatch(typeof(RunManager), nameof(RunManager.SetRunLevel))]
    public static class LevelPatches
    {
        static void Postfix(RunManager __instance)
        {
            if (Instance.SelectedLevel != LevelTypes.NONE)
            {
                __instance.levelCurrent = __instance.levels[(int)Instance.SelectedLevel];
            }
        }
    }

    [HarmonyPatch(typeof(LevelGenerator), "GenerateDone")]
    public static class BottleSpawnPatch
    {
        // child should be inactive before this is called!
        private static void ParentTo(GameObject child, GameObject parent)
        {
            // The GameObject that has the valuable as a child is either the root or a child GameObject.
            GameObject valuableContainerGo = null;

            if (parent.name.Contains("Valuable"))
            {
                valuableContainerGo = parent;
            }
            else
            {
                for (int i = 0; i < parent.transform.childCount; ++i)
                {
                    GameObject c = parent.transform.GetChild(i).gameObject;

                    if (c.name.Contains("Valuable"))
                    {
                        valuableContainerGo = c;
                        break;
                    }
                }
            }

            if (valuableContainerGo != null)
            {
                child.transform.SetParent(valuableContainerGo.transform);
            }
            else
            {
                ModEntry.Instance.Logger.LogWarning(string.Format("Could not parent {0} to {1} because {1} did not contain a GameObject called \"Valuable\"", child.name, parent.name));
            }
        }

        static List<ValuableObject> SmallContainedValuables(GameObject volume)
        {
            return Utils.ContainedValuables(volume).Where(v => v.volumeType == ValuableVolume.Type.Small).ToList();
        }

        static void Postfix()
        {
            // Only run on an extraction level
            if (Utils.IsExtractionLevelRunning())
            {
                /* 
                    To spawn the bottle, there are 6 cases.
                    1. No fridges are in the map. Destroy all bottles.
                    2. Bottle is already in a fridge. No action for that bottle.
                    3. No bottles spawned. Don't do anything.
                    4. Bottle spawned outside of fridge and there is an empty fridge. Move the bottle to that fridge.
                    5. Bottle spawned outside of fridge and all fridges are full. Swap a non-bottle in the fridge with the bottle outside the fridge.
                    6. Bottle spawned outside of fridge and all fridges are full with bottles already. Destroy the bottle that is outside the fridge.
                */

                // Determine if there are any bottles that need to be moved.
                List<GameObject> bottles = Utils.GetLevelModValuableInstances(ModValuables.BOTTLE);
                if (bottles.Count > 0)
                {
                    // Iterate through all the bottles and process them.
                    foreach (GameObject bottle in bottles)
                    {
                        List<GameObject> fridges = Utils.GetLevelGameObjectsByName("Fridge");

                        if (fridges.Count > 0)
                        {
                            // Determine if the bottle is already in a fridge.
                            bool inFridge = false;

                            foreach (GameObject fridge in fridges)
                            {
                                foreach (ValuableObject valuable in SmallContainedValuables(fridge))
                                {
                                    if (valuable.gameObject == bottle)
                                    {
                                        inFridge = true;
                                        break;
                                    }
                                }

                                if (inFridge)
                                {
                                    break;
                                }
                            }

                            // No processing needed if it is already in the fridge.
                            if (inFridge)
                            {
                                continue;
                            }


                            // Not in fridge, see if a fridge can accept the bottle.

                            /*
                                Operate in the following priority:
                                1. Empty fridges
                                2. Fridges without a bottle in it already
                            */

                            // Look for empty fridges first

                            bool aFridgeIsEmpty = false;
                            GameObject emptyFridge = null;
                            foreach (GameObject fridge in fridges)
                            {
                                if (SmallContainedValuables(fridge).Count == 0)
                                {
                                    aFridgeIsEmpty = true;
                                    emptyFridge = fridge;
                                    break;
                                }
                            }

                            if (aFridgeIsEmpty)
                            {
                                // Move to the fridge.
                                bottle.GetComponent<Rigidbody>().Sleep();
                                bottle.SetActive(false);
                                ParentTo(child: bottle, parent: emptyFridge);
                                bottle.transform.localPosition = new Vector3(x: 0, y: 0, z: -0.2f);
                                bottle.SetActive(true);
                                bottle.GetComponent<Rigidbody>().isKinematic = false;
                                bottle.GetComponent<Rigidbody>().WakeUp();

                                ModEntry.Instance.Logger.LogInfo("Moved bottle to empty fridge!");
                            }
                            else
                            {
                                // Search for a fridge that does not contain a bottle.
                                bool hasBottle = true;
                                foreach (GameObject fridge in fridges)
                                {
                                    foreach (ValuableObject v in SmallContainedValuables(fridge))
                                    {
                                        if (!v.gameObject.name.Contains(ModValuables.BOTTLE.Name))
                                        {
                                            hasBottle = false;

                                            // Swap the contained valuable with the bottle.

                                            bottle.GetComponent<Rigidbody>().Sleep();
                                            v.gameObject.GetComponent<Rigidbody>().Sleep();

                                            GameObject currentBottleParent = bottle.transform.parent.gameObject;
                                            GameObject currentSwapParent = v.gameObject.transform.parent.gameObject;
                                            Vector3 bottleLocalPos = bottle.transform.localPosition;
                                            Vector3 swapLocalPos = v.gameObject.transform.localPosition;

                                            v.gameObject.SetActive(false);
                                            bottle.SetActive(false);

                                            ParentTo(child: bottle, parent: currentSwapParent);
                                            ParentTo(child: v.gameObject, parent: currentBottleParent);

                                            bottle.transform.localPosition = swapLocalPos;
                                            v.gameObject.transform.localPosition = bottleLocalPos;

                                            v.gameObject.SetActive(true);
                                            bottle.SetActive(true);

                                            v.gameObject.GetComponent<Rigidbody>().isKinematic = false;
                                            bottle.GetComponent<Rigidbody>().isKinematic = false;
                                            v.gameObject.GetComponent<Rigidbody>().WakeUp();
                                            bottle.GetComponent<Rigidbody>().WakeUp();

                                            ModEntry.Instance.Logger.LogInfo(string.Format("Swapped the bottle with a {0}", v.name));

                                            break;
                                        }
                                    }

                                    if (!hasBottle)
                                    {
                                        break;
                                    }
                                }

                                // If all fridges have bottles, delete the bottle
                                if (hasBottle)
                                {
                                    // TODO: Reduce target by the bottle's worth
                                    ValuableDirector valuableDirector;

                                    try
                                    {
                                        valuableDirector = Utils.GetValuableDirector();

                                        // TODO: Use reflection to modify this value.
                                        // Also put this into a Util function.

                                    }
                                    catch (RepoSingletonNullException e)
                                    {
                                        ModEntry.Instance.Logger.LogWarning(string.Format("Could not reduce total haul when removing a bottle because of the following exception: {0}", e.Message));
                                    }

                                    UnityEngine.Object.Destroy(bottle);

                                    ModEntry.Instance.Logger.LogInfo("Destroyed bottle because all fridges have bottles already!");
                                }
                            }

                        }
                        // No fridges, delete the bottle.
                        else
                        {
                            // TODO: Reduce target by the bottle's worth.
                            UnityEngine.Object.Destroy(bottle);

                            ModEntry.Instance.Logger.LogInfo("Destroyed bottle because no fridges are in the map :(");
                        }
                    }
                }

            }
        }
    }

}