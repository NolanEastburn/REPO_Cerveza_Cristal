using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
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

    [HarmonyPatch(typeof(ValuableDirector), nameof(ValuableDirector.SetupHost))]
    class TestPatch
    {
        static void Prefix()
        {
            Logger.LogInfo("Foobert doobert do");
        }
    }

    private void Awake()
    {
        // Harmony test
        HarmonyFileLog.Enabled = true;
        Harmony harmony = new Harmony("com.nooterdooter.cerveza_cristal");
        harmony.PatchAll();

        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }

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
            foreach (ModValuableRegistry.ValuableAddition addition in ModValuables.ValuableAdditions)
            {
                _modValuableRegistry.Register(addition);
            }

            // Add the ValuableRegistry to the list of registries.
            _modRegistries.Add(_modValuableRegistry);

            assetBundlesLoaded = true;
        }

        if (assetBundlesLoaded && !additionsRegistered && !_failedToLoadAssetBundle)
        {
            if (RunManager.instance != null)
            {
                foreach (IModRegistry registry in _modRegistries)
                {
                    registry.ApplyAdditionRegistrations(RunManager.instance);
                }


                additionsRegistered = true;
            }

            PhotonNetwork.PrefabPool = new ModPrefabPool(_modValuableRegistry, Logger);

            gameObject.SetActive(false);
        }

    }

}