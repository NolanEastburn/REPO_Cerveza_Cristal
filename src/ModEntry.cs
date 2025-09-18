using System.IO;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using Photon.Pun;
using System;

namespace Cerveza_Cristal;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class ModEntry : BaseUnityPlugin
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
            v.physAttributePreset.mass = 2.0f;
            v.volumeType = ValuableVolume.Type.Medium;
            v.durabilityPreset = ScriptableObject.CreateInstance(typeof(Durability)) as Durability;
            //v.audioPreset = ScriptableObject.CreateInstance(typeof(PhysAudio)) as PhysAudio;
            v.particleColors = new Gradient();
            v.particleColors.colorKeys = new GradientColorKey[1];
            v.particleColors.colorKeys[0] = new GradientColorKey(Color.white, 0.0f);
            v.particleColors.alphaKeys = new GradientAlphaKey[1];
            v.particleColors.alphaKeys[0] = new GradientAlphaKey(1.0f, 0.0f);

            testValuable.AddComponent(typeof(Rotate));
            testValuable.AddComponent(typeof(PhotonTransformView));
            testValuable.AddComponent(typeof(PhysGrabObject));
            testValuable.AddComponent(typeof(RoomVolumeCheck));
            testValuable.AddComponent(typeof(Rigidbody));
            testValuable.AddComponent(typeof(PhysGrabObjectImpactDetector));
            testValuable.AddComponent(typeof(PhotonView));
            testValuable.AddComponent(typeof(BoxCollider));
            testValuable.AddComponent(typeof(PhysGrabObjectCollider));

            testValuable.tag = "Phys Grab Object";
            testValuable.name = "Test Valuable";


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

                Logger.LogInfo(RunManager.instance.levels[0].ValuablePresets[0].medium[0].name);
                foreach (Component c in RunManager.instance.levels[0].ValuablePresets[0].medium[0].GetComponentsInChildren<Collider>())
                {
                    Logger.LogInfo(c.gameObject.name);
                }
            }

            PhotonNetwork.PrefabPool = new ModPrefabPool(testValuable);
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
            ModEntry.Logger.LogInfo("Loaded!");
            loadMessage = false;
        }
    }
}

class ModPrefabPool : IPunPrefabPool
{
    private IPunPrefabPool _defaultPool { set; get; } = null;

    // TODO: Replace with registry!!!
    private GameObject _go { set; get; } = null;

    private bool IsModAddition(string prefabId)
    {
        // TODO: Update!!!
        return prefabId == "Valuables/03 Medium/Test Valuable";
    }

    public ModPrefabPool(GameObject go)
    {
        _defaultPool = new DefaultPool();
        _go = go;

        ModEntry.Logger.LogInfo("Created prefab pool!");
    }

    public void Destroy(GameObject gameObject)
    {
        if (IsModAddition(gameObject.name))
        {
            // TODO: Update!!!
        }
        else
        {
            _defaultPool.Destroy(gameObject);
        }
    }

    public GameObject Instantiate(string prefabId, Vector3 position, Quaternion rotation)
    {
        ModEntry.Logger.LogInfo("Attempting to instantiate " + prefabId);

        if (IsModAddition(prefabId))
        {
            return UnityEngine.Object.Instantiate(_go, position, rotation);
        }
        else
        {
            return _defaultPool.Instantiate(prefabId, position, rotation);
        }
    }
}
