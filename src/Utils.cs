using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UIElements.Collections;

namespace Cerveza_Cristal;

public static class Utils
{
    public const string UNIQUE_ORG_STRING = "com.nooterdooter.cerveza_cristal";

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

    public static RunManager GetRunManager()
    {
        RunManager result = RunManager.instance;

        if (result == null)
        {
            throw new RepoSingletonNullException(typeof(RunManager));
        }

        return result;
    }

    public static GameDirector GetGameDirector()
    {
        GameDirector result = GameDirector.instance;

        if (result == null)
        {
            throw new RepoSingletonNullException(typeof(GameDirector));
        }

        return result;
    }

    public static bool IsExtractionLevelRunning()
    {
        try
        {
            RunManager runManager = GetRunManager();

            Level level = runManager.levelCurrent;

            List<Level> nonExtractionLevels = new List<Level>() {runManager.levelArena, runManager.levelLobby, runManager.levelLobbyMenu,
             runManager.levelMainMenu, runManager.levelRecording, runManager.levelShop, runManager.levelSplashScreen, runManager.levelTutorial};

            return level != null && !nonExtractionLevels.Contains(level);
        }
        catch (RepoSingletonNullException e)
        {
            _logger.LogWarning(string.Format("Could not determine if an extraction level is running due to the following exception: {0}", e.Message));
            return false;
        }
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
        // Check to make sure that an extraction level has been started.
        if (!IsExtractionLevelRunning())
        {
            throw new RepoInvalidActionException(action: string.Format("spawn the following custom valuable: {0}", registry.GetRegistryName(valuable)), reason: "an extraction level is not running");
        }

        GameDirector director;
        try
        {
            director = GetGameDirector();
            Transform playerTransform = director.PlayerList[0].gameObject.transform;
            UnityEngine.Object.Instantiate(registry.GetRegistryEntry(valuable).Item1, playerTransform.position, playerTransform.rotation);
        }
        catch (RepoSingletonNullException e)
        {
            _logger.LogWarning(string.Format("Could not spawn {0} due to the following exception: {1}", valuable.AssetName, e.Message));
        }

    }
}