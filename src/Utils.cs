using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Reflection;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UIElements.Collections;

namespace Cerveza_Cristal;

public static class Utils
{
    public static readonly int VALUABLE_LAYER_MASK = LayerMask.NameToLayer("PhysGrabObject");

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

    public static ValuableDirector GetValuableDirector()
    {
        ValuableDirector result = ValuableDirector.instance;

        if (result == null)
        {
            throw new RepoSingletonNullException(typeof(ValuableDirector));
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

    // Uses the name of the Transform component for the name.
    public static List<ValuableVolume> GetLevelValuableVolumesByName(string valuableVolumeName)
    {
        List<ValuableVolume> result = new List<ValuableVolume>();

        if (!IsExtractionLevelRunning())
        {
            _logger.LogWarning("Could not get the valuable volumes for the level because no extraction level is running!");
            return result;
        }

        ValuableDirector valuableDirector;

        try
        {
            valuableDirector = GetValuableDirector();

            // Search all valuable volumes for the given name

            FieldInfo tinyVolumesField = valuableDirector.GetType().GetField("tinyVolumes", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField);
            List<ValuableVolume> tinyVolumes = (List<ValuableVolume>)tinyVolumesField.GetValue(valuableDirector);

            FieldInfo smallVolumesField = valuableDirector.GetType().GetField("smallVolumes", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField);
            List<ValuableVolume> smallVolumes = (List<ValuableVolume>)smallVolumesField.GetValue(valuableDirector);

            FieldInfo mediumVolumesField = valuableDirector.GetType().GetField("mediumVolumes", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField);
            List<ValuableVolume> mediumVolumes = (List<ValuableVolume>)mediumVolumesField.GetValue(valuableDirector);

            FieldInfo bigVolumesField = valuableDirector.GetType().GetField("bigVolumes", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField);
            List<ValuableVolume> bigVolumes = (List<ValuableVolume>)bigVolumesField.GetValue(valuableDirector);

            FieldInfo wideVolumesField = valuableDirector.GetType().GetField("wideVolumes", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField);
            List<ValuableVolume> wideVolumes = (List<ValuableVolume>)wideVolumesField.GetValue(valuableDirector);

            FieldInfo tallVolumesField = valuableDirector.GetType().GetField("tallVolumes", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField);
            List<ValuableVolume> tallVolumes = (List<ValuableVolume>)tallVolumesField.GetValue(valuableDirector);

            FieldInfo veryTallVolumesField = valuableDirector.GetType().GetField("veryTallVolumes", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField);
            List<ValuableVolume> veryTallVolumes = (List<ValuableVolume>)veryTallVolumesField.GetValue(valuableDirector);

            List<ValuableVolume> masterVolumeList = [.. tinyVolumes, .. smallVolumes, .. mediumVolumes,
            .. bigVolumes, .. wideVolumes, .. tallVolumes, .. veryTallVolumes];

            foreach (ValuableVolume v in masterVolumeList)
            {
                if (v != null)
                {
                    if (v.GetComponent<Transform>().name == valuableVolumeName)
                    {
                        result.Add(v);
                    }
                }
            }
        }
        catch (RepoSingletonNullException e)
        {
            _logger.LogWarning(string.Format("Could not get the valuable volumes for the level because of the following exception: {0}", e.Message));
            return result;
        }

        return result;
    }

    public static List<ValuableObject> ContainedValuables(ValuableVolume volume)
    {
        // Temporary map to make sure we don't count valuables twice.
        Dictionary<string, ValuableObject> valuableDict = new Dictionary<string, ValuableObject>();

        // If one of the volume colliders is colliding with a valuable, then the volume is deemed to contain a valuable.

        // Get the colliders
        List<Collider> colliders = new List<Collider>(volume.GetComponents<Collider>());

        foreach (Collider c in colliders)
        {
            // Create a bounding box around the collider.
            List<Collider> overlaps = new List<Collider>(Physics.OverlapBox(c.bounds.center, c.bounds.extents, Quaternion.identity, VALUABLE_LAYER_MASK));

            foreach (Collider overlap in overlaps)
            {
                ValuableObject valuableObject = overlap.GetComponentInParent<ValuableObject>();

                if (valuableObject != null)
                {
                    valuableDict.Add(valuableObject.name, valuableObject);
                }
            }
        }

        return new List<ValuableObject>(valuableDict.Values);
    }

    // TODO: Get this working for multiplayer as well (not super important however)
    public static void SpawnModValuable(ModValuableRegistry registry, ValuableAddition valuable)
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