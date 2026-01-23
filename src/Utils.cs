using System;
using System.Diagnostics.Tracing;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UIElements.Collections;

namespace Cerveza_Cristal;

public static class Utils
{
    public enum LevelTypes
    {
        ARCTIC = 0,
        MANOR = 1,
        MUSEUM = 2,
        WIZARD = 3,
        NONE = 4
    }

    private static ManualLogSource _logger { get; set; } = null;

    private static LevelTypes _selectedLevel { get; set; } = LevelTypes.NONE;

    public static void SelectLevel(LevelTypes selectedLevel)
    {
        _selectedLevel = selectedLevel;
    }

    public static void SetLogger(ManualLogSource logger)
    {
        _logger = logger;
    }

    [HarmonyPatch(typeof(RunManager), nameof(RunManager.SetRunLevel))]
    public static class LevelPatches
    {
        static void Postfix(RunManager __instance)
        {
            if (_selectedLevel != LevelTypes.NONE)
            {
                __instance.levelCurrent = __instance.levels[(int)_selectedLevel];
            }
        }
    }

    public static void SpawnModValuable(ModValuableRegistry registry, ModValuableRegistry.ValuableAddition valuable)
    {
        // Get the current RunManager

        RunManager runManager = RunManager.instance;
        GameDirector director = GameDirector.instance;

        if (director != null)
        {
            _logger.LogWarning("Spawning a bottle!");
            UnityEngine.Object.Instantiate(registry.Registry[registry.GetRegistryName(valuable.ValuableData)].Item1, director.PlayerList[0].gameObject.transform.position, director.PlayerList[0].gameObject.transform.rotation);
        }
    }
}