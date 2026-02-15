using System;
using System.Collections.Generic;
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
        static void Postfix(LevelGenerator __instance)
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
                                foreach (ValuableObject valuable in Utils.ContainedValuables(fridge))
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
                            foreach (GameObject fridge in fridges)
                            {
                                if (Utils.ContainedValuables(fridge).Count == 0)
                                {
                                    aFridgeIsEmpty = true;
                                    break;
                                }
                            }

                            if (aFridgeIsEmpty)
                            {
                                // Move to the fridge.

                                // TODO: Add this!
                            }
                            else
                            {
                                // Search for a fridge that does not contain a bottle.
                                bool hasBottle = true;
                                foreach (GameObject fridge in fridges)
                                {
                                    foreach (ValuableObject v in Utils.ContainedValuables(fridge))
                                    {
                                        if (!v.gameObject.name.Contains(ModValuables.BOTTLE.Name))
                                        {
                                            hasBottle = false;

                                            // Swap the contained valuable with the bottle.
                                            v.gameObject.SetActive(false);
                                            bottle.SetActive(false);

                                            Vector3 bottlePos = bottle.transform.position;

                                            bottle.transform.position = v.gameObject.transform.position;
                                            v.gameObject.transform.position = bottlePos;

                                            v.gameObject.SetActive(true);
                                            bottle.SetActive(true);

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
                                    UnityEngine.Object.Destroy(bottle);
                                }
                            }

                        }
                        // No fridges, delete the bottle.
                        else
                        {
                            // TODO: Reduce target by the bottle's worth.
                            UnityEngine.Object.Destroy(bottle);
                        }
                    }
                }

            }
        }
    }

}