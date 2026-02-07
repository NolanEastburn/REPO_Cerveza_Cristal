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

    private static string pluginRoot = Path.Combine(Paths.BepInExRootPath, "plugins");

    private static string assetBundlePath = Path.Combine(pluginRoot, MOD_CONTENT_FOLDER, RESOURCES_FOLDER, "AssetBundles", "primaryassetbundle");

    private static ModValuableRegistry _modValuableRegistry { get; set; }

    private static List<IModRegistry> _modRegistries { get; set; } = new List<IModRegistry>();

    private static IPunPrefabPool _multiplayerPool { get; set; } = null;

    [HarmonyPatch(typeof(LevelGenerator), "Start")]
    class ModAssetRestorePatch
    {
        static void Postfix()
        {
            Dictionary<string, GameObject> singleplayerPool = GetSingleplayerPool();

            if (singleplayerPool != null)
            {
                foreach ((GameObject, ValuableAddition) regEntry in _modValuableRegistry.RegistryDictionary.Values)
                {
                    singleplayerPool.Add(_modValuableRegistry.GetRegistryName(regEntry.Item2), regEntry.Item1);
                }
            }

            // Reset the multiplayer pool.
            PhotonNetwork.PrefabPool = _multiplayerPool;
        }
    }

    private static Dictionary<string, GameObject> GetSingleplayerPool()
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
            _modValuableRegistry = new ModValuableRegistry(assetBundle, Logger);

            // Register each valuable
            foreach (ValuableAddition addition in ModValuables.ValuableAdditions)
            {
                _modValuableRegistry.Register(addition);
            }

            // Add the ValuableRegistry to the list of registries.
            _modRegistries.Add(_modValuableRegistry);

            assetBundlesLoaded = true;
        }

        if (assetBundlesLoaded && !additionsRegistered && !_failedToLoadAssetBundle)
        {
            RunManager runManager; try
            {
                runManager = Utils.GetRunManager();

                Utils.SetLogger(Logger);
                Utils.SelectLevel(Utils.LevelTypes.ARCTIC);

                // Harmony test
                HarmonyFileLog.Enabled = true;
                Harmony harmony = new Harmony(Utils.UNIQUE_ORG_STRING);

                harmony.PatchAll();

                foreach (IModRegistry registry in _modRegistries)
                {
                    registry.ApplyAdditionRegistrations(RunManager.instance);
                }

                _multiplayerPool = new ModPrefabPool(_modValuableRegistry, Logger);

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
                Utils.SpawnModValuable(_modValuableRegistry, ModValuables.BOTTLE);
            }
            catch (RepoInvalidActionException e)
            {
                Logger.LogWarning(string.Format("Could not spawn the bottle because of the following exception: {0}", e.Message));
            }
        }

        if (Input.GetKey(KeyCode.Insert) && !_insertPressed)
        {
            _insertPressed = true;

            List<ValuableVolume> fridges = Utils.GetLevelValuableVolumesByName("Kitchen Fridge");
            Logger.LogInfo(string.Format("Fridge Count: {0}", fridges.Count));
        }
        else if (!Input.GetKey(KeyCode.Insert) && _insertPressed)
        {
            _insertPressed = false;
        }
    }
}