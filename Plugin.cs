using System;
using System.IO;
using System.Runtime.CompilerServices;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Cerveza_Cristal;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;

    private const string MOD_CONTENT_FOLDER = "nooterdooter_cerveza_cristal";
    private const string RESOURCES_FOLDER = "res";

    private Boolean spawned = false;
    private const float CRASH_TIME = 15;

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
        if (Time.time >= CRASH_TIME && !spawned)
        {
            AssetBundle assetBundle = AssetBundle.LoadFromFile(assetBundlePath);
            if (assetBundle == null)
            {
                Logger.LogError("Failed to load asset bundle!");
            }


            GameObject testValuable = assetBundle.LoadAsset<GameObject>("Cone");
            GameObject foo = Instantiate(testValuable);

            // Find truck.

            GameObject truck = null;
            foreach (GameObject g in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
            {
                TruckLandscapeScroller obj = g.GetComponentInChildren<TruckLandscapeScroller>();

                if (obj != null)
                {
                    truck = obj.gameObject;
                }
            }

            spawned = true;
            foo.transform.position = truck.transform.position + new Vector3(-35, 5, 0);
            foo.AddComponent(typeof(Rotate));

            //foo.transform.localScale = new Vector3(1.0f, 1.0f, 2.0f);
            Logger.LogInfo("Spawned a thing!");
        }

    }

}

class Rotate : MonoBehaviour
{
    public void Update()
    {
        gameObject.transform.rotation *= Quaternion.Euler(360 * Time.deltaTime, 0, 0);
    }
}
