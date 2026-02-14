using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using HarmonyLib.Tools;
using Photon.Pun;
using UnityEngine;

namespace Cerveza_Cristal;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class ModEntry : BaseUnityPlugin
{
    internal static new ManualLogSource Logger = null;

    private const string MOD_CONTENT_FOLDER = "nooterdooter_cerveza_cristal";
    private const string RESOURCES_FOLDER = "res";

    private bool assetBundlesLoaded = false;

    private bool additionsRegistered = false;

    private bool _failedToLoadAssetBundle = false;

    private ModPatches _patches { get; set; } = null;

    // Static variables
    private static string pluginRoot = Path.Combine(Paths.BepInExRootPath, "plugins");

    private static string assetBundlePath = Path.Combine(pluginRoot, MOD_CONTENT_FOLDER, RESOURCES_FOLDER, "AssetBundles", "primaryassetbundle");

    public static ModValuableRegistry ModValuableRegistry { get; set; }

    public static List<IModRegistry> ModRegistries { get; set; } = new List<IModRegistry>();

    public static IPunPrefabPool MultiplayerPool { get; set; } = null;


    public static Dictionary<string, GameObject> GetSingleplayerPool()
    {
        // Register it in the singleplayer pool
        RunManager rmInstance = RunManager.instance;
        FieldInfo field = rmInstance.GetType().GetField("singleplayerPool", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField);
        Dictionary<string, GameObject> pool = (Dictionary<string, GameObject>)field.GetValue(rmInstance);
        return pool;
    }

    private void Awake()
    {

        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }

    private bool _insertPressed = false;

    private void Update()
    {
        if (!assetBundlesLoaded && !_failedToLoadAssetBundle)
        {

            AssetBundle assetBundle = AssetBundle.LoadFromFile(assetBundlePath);
            if (assetBundle == null)
            {
                Logger.LogInfo(assetBundlePath);
                Logger.LogError("Failed to load asset bundle!");
                _failedToLoadAssetBundle = true;
                return;
            }

            // Create the valuables registry
            ModValuableRegistry = new ModValuableRegistry(assetBundle, Logger);

            // Register each valuable
            foreach (ValuableAddition addition in ModValuables.ValuableAdditions)
            {
                ModValuableRegistry.Register(addition);
            }

            // Add the ValuableRegistry to the list of registries.
            ModRegistries.Add(ModValuableRegistry);

            assetBundlesLoaded = true;
        }

        if (assetBundlesLoaded && !additionsRegistered && !_failedToLoadAssetBundle)
        {
            RunManager runManager; try
            {
                runManager = Utils.GetRunManager();

                Utils.SetLogger(Logger);

                ModPatches.Instance.Logger = Logger;
                ModPatches.Instance.SelectedLevel = LevelTypes.ARCTIC;
                ModPatches.Instance.ApplyPatches();

                foreach (IModRegistry registry in ModRegistries)
                {
                    registry.ApplyAdditionRegistrations(RunManager.instance);
                }

                MultiplayerPool = new ModPrefabPool(ModValuableRegistry, Logger);

                additionsRegistered = true;

                //gameObject.SetActive(false);
            }
            catch (RepoSingletonNullException)
            {
                // Do nothing. We are waiting for the static RunManager instance to not be null.
            }
        }

        // Periodic processing.
        if (Input.GetKey(KeyCode.Delete))
        {
            try
            {
                Utils.SpawnModValuable(ModValuableRegistry, ModValuables.BOTTLE);
            }
            catch (RepoInvalidActionException e)
            {
                Logger.LogWarning(string.Format("Could not spawn the bottle because of the following exception: {0}", e.Message));
            }
        }

        if (Input.GetKey(KeyCode.Insert) && !_insertPressed)
        {
            _insertPressed = true;

            List<GameObject> fridges = Utils.GetLevelGameObjectsByName("Kitchen Fridge");
            foreach (GameObject fridge in fridges)
            {
                foreach (ValuableObject v in Utils.ContainedValuables(fridge))
                {
                    Logger.LogInfo(string.Format("Found the following ValuableObject in the fridge: {0}", v.gameObject.name));
                }
            }
        }
        else if (!Input.GetKey(KeyCode.Insert) && _insertPressed)
        {
            _insertPressed = false;
        }
    }
}