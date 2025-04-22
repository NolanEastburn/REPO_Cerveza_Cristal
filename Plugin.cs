using System;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;

namespace Cerveza_Cristal;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;

    private Boolean spawned = false;
    private const float CRASH_TIME = 5;
        
    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

    }

    private void Update()
    {

        if(Time.time >= CRASH_TIME && !spawned)
        {
            // Find truck.

            GameObject truck = null;

            foreach(GameObject g in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
            {
                TruckLandscapeScroller obj = g.GetComponentInChildren<TruckLandscapeScroller>();

                if(obj != null)
                {
                    truck = obj.gameObject;
                }
            }

            spawned = true;
            GameObject thing = UnityEngine.Object.Instantiate(AssetManager.instance.surplusValuableBig, truck.transform.position + new Vector3(-35, 10, 0), Quaternion.identity);
            thing.transform.localScale = new Vector3(5, 10, 5);
            Logger.LogInfo("Spawned a thing!");
        }

    }

}
