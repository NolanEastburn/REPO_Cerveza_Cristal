using System.IO;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using Photon.Pun;

namespace Cerveza_Cristal;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;

    private const string MOD_CONTENT_FOLDER = "nooterdooter_cerveza_cristal";
    private const string RESOURCES_FOLDER = "res";

    private bool assetBundlesLoaded = false;

    private bool valueableAdded = false;

    private static string pluginRoot = Path.Combine(Paths.BepInExRootPath, "plugins");

    private static string assetBundlePath = Path.Combine(pluginRoot, MOD_CONTENT_FOLDER, RESOURCES_FOLDER, "AssetBundles", "primaryassetbundle");

    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }

    private void Update()
    {
        GameObject testValuable = null;
        if (!assetBundlesLoaded)
        {
            AssetBundle assetBundle = AssetBundle.LoadFromFile(assetBundlePath);
            if (assetBundle == null)
            {
                Logger.LogError("Failed to load asset bundle!");
            }

            testValuable = assetBundle.LoadAsset<GameObject>("Cone");

            ValuableObject v = testValuable.AddComponent(typeof(ValuableObject)) as ValuableObject;
            v.valuePreset = ScriptableObject.CreateInstance(typeof(Value)) as Value;
            v.valuePreset.valueMin = 1000.0f;
            v.valuePreset.valueMax = 1000.0f;
            v.physAttributePreset = ScriptableObject.CreateInstance(typeof(PhysAttribute)) as PhysAttribute;
            v.physAttributePreset.mass = 100.0f;
            v.volumeType = ValuableVolume.Type.Medium;
            
            testValuable.AddComponent(typeof(Rotate));
            testValuable.AddComponent(typeof(PhotonTransformView));
            testValuable.AddComponent(typeof(PhysGrabObject));
            testValuable.AddComponent(typeof(RoomVolumeCheck));
            testValuable.AddComponent(typeof(Rigidbody));
            testValuable.AddComponent(typeof(BoxCollider));
            

            assetBundlesLoaded = true;
        }

        if (assetBundlesLoaded && !valueableAdded)
        {
            if (RunManager.instance != null)
            {

                foreach (Level level in RunManager.instance.levels)
                {
                    foreach (LevelValuables lv in level.ValuablePresets)
                    {
                        lv.medium.Add(testValuable);
                    }

                    Logger.LogInfo("Added test valueable to level " + level.name);
                }

                valueableAdded = true;
            }
        }

    }

}

class Rotate : MonoBehaviour
{
    private bool loadMessage = true;

    public void Update()
    {
        if (loadMessage)
        {
            Plugin.Logger.LogInfo("Loaded!");
            loadMessage = false;
        }
    }
}
