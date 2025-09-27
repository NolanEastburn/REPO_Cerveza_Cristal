using System.IO;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using Photon.Pun;

namespace Cerveza_Cristal;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class ModEntry : BaseUnityPlugin
{
    internal static new ManualLogSource Logger = null;

    private const string MOD_CONTENT_FOLDER = "nooterdooter_cerveza_cristal";
    private const string RESOURCES_FOLDER = "res";

    private bool assetBundlesLoaded = false;

    private bool valueableAdded = false;

    private bool _failedToLoadAssetBundle = false;

    private static string pluginRoot = Path.Combine(Paths.BepInExRootPath, "plugins");

    private static string assetBundlePath = Path.Combine(pluginRoot, MOD_CONTENT_FOLDER, RESOURCES_FOLDER, "AssetBundles", "primaryassetbundle");

    private static ModValuableRegistry _modValuableRegistry { get; set; }

    private void Awake()
    {
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

            _modValuableRegistry.Register("Cone", new ModValuableRegistry.Data(name: "Test Valuable"));

            assetBundlesLoaded = true;
        }

        if (assetBundlesLoaded && !valueableAdded && !_failedToLoadAssetBundle)
        {
            if (RunManager.instance != null)
            {

                foreach (Level level in RunManager.instance.levels)
                {
                    foreach (LevelValuables lv in level.ValuablePresets)
                    {
                        foreach ((GameObject, ModValuableRegistry.Data) regEntry in _modValuableRegistry.Registry.Values)
                        {
                            switch (regEntry.Item2.ValuableVolumeType)
                            {

                                case ValuableVolume.Type.Tiny:
                                    lv.tiny.Add(regEntry.Item1);
                                    break;

                                case ValuableVolume.Type.Small:
                                    lv.small.Add(regEntry.Item1);
                                    break;

                                case ValuableVolume.Type.Medium:
                                    lv.medium.Add(regEntry.Item1);
                                    break;

                                case ValuableVolume.Type.Big:
                                    lv.big.Add(regEntry.Item1);
                                    break;

                                case ValuableVolume.Type.Wide:
                                    lv.wide.Add(regEntry.Item1);
                                    break;

                                case ValuableVolume.Type.Tall:
                                    lv.tall.Add(regEntry.Item1);
                                    break;

                                case ValuableVolume.Type.VeryTall:
                                    lv.veryTall.Add(regEntry.Item1);
                                    break;
                            }
                        }
                    }
                }

                valueableAdded = true;
            }

            PhotonNetwork.PrefabPool = new ModPrefabPool(_modValuableRegistry, Logger);

            gameObject.SetActive(false);
        }

    }

}