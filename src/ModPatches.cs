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
                foreach ((GameObject, ValuableAddition) regEntry in ModEntry.ModValuableRegistry.RegistryDictionary.Values)
                {
                    singleplayerPool.Add(ModEntry.ModValuableRegistry.GetRegistryName(regEntry.Item2), regEntry.Item1);
                }
            }

            // Reset the multiplayer pool.
            PhotonNetwork.PrefabPool = ModEntry.MultiplayerPool;
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

}